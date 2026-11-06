using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.PaymentMethods;
using Way2Pay.Payments.Application.Payments.Providers;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;
using static Way2Pay.Payments.Application.Tests.Payments.ExecutionTestContext;

namespace Way2Pay.Payments.Application.Tests.Payments;

public sealed class ExecutePaymentOperationTests
{
    [Fact]
    public async Task Execute_SavesRouteAndAttemptBeforeCallingProvider()
    {
        var context = new ExecutionTestContext();
        context.Provider.OnExecute = (request, _) =>
        {
            Assert.Equal(1, context.UnitOfWork.SaveCalls);
            Assert.NotNull(context.Operation.Route);
            Assert.Equal(request.AttemptId, Assert.Single(context.Operation.Attempts).Id);
            Assert.Equal(PaymentAttemptStatus.Processing, context.Operation.Attempts[0].Status);
            return Task.FromResult(ProviderOperationResult.Succeeded("provider-tx"));
        };
        var result = await context.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Captured, result.Value.PaymentStatus);
        Assert.Equal(2, context.UnitOfWork.SaveCalls);
        Assert.Single(context.Provider.Requests);
    }

    [Fact]
    public async Task Execute_WhenClaimSaveConflicts_DoesNotCallProvider()
    {
        var context = new ExecutionTestContext();
        context.UnitOfWork.OnSave = _ => throw new PaymentConcurrencyException(new InvalidOperationException("Version conflict"));
        var result = await context.ExecuteAsync();

        Assert.False(result.IsSuccess);
        Assert.Equal("Payments.ConcurrencyConflict", result.Error!.Code);
        Assert.Empty(context.Provider.Requests);
        Assert.Equal(1, context.UnitOfWork.SaveCalls);
        Assert.Equal(1, context.UnitOfWork.DiscardCalls);
    }

    [Fact]
    public async Task Execute_WhenResultSaveFails_PropagatesWithoutResendingProviderRequest()
    {
        var context = new ExecutionTestContext();
        context.UnitOfWork.OnSave = call =>
        {
            if (call == 2)
                throw new IOException("Result storage is unavailable.");
        };

        await Assert.ThrowsAsync<IOException>(() => context.ExecuteAsync());
        Assert.Single(context.Provider.Requests);
        Assert.Equal(2, context.UnitOfWork.SaveCalls);
    }

    [Theory]
    [InlineData("timeout")]
    [InlineData("network")]
    public async Task Execute_WhenProviderOutcomeIsAmbiguous_PersistsUnknownAndDoesNotRetry(string failure)
    {
        var context = new ExecutionTestContext();
        Exception exception = failure == "timeout"
            ? new TimeoutException() : new HttpRequestException();
        context.Provider.OnExecute = (_, _) => Task.FromException<ProviderOperationResult>(exception);
        var result = await context.ExecuteAsync();
        var replay = await context.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.True(replay.IsSuccess);
        Assert.Equal(PaymentAttemptStatus.Unknown, replay.Value.AttemptStatus);
        Assert.Equal(PaymentOperationStatus.Unknown, context.Operation.Status);
        Assert.Equal(PaymentTestData.Amount(0), context.Payment.Balance.Captured);
        Assert.Single(context.Provider.Requests);
        Assert.Equal(2, context.UnitOfWork.SaveCalls);
    }

    [Fact]
    public async Task Execute_WhenCallerCancelsAfterConfirmedSuccess_SavesOutcomeWithoutCallerToken()
    {
        var context = new ExecutionTestContext();
        using var cancellation = new CancellationTokenSource();
        context.Provider.OnExecute = (_, _) =>
        {
            cancellation.Cancel();
            return Task.FromResult(ProviderOperationResult.Succeeded("provider-tx"));
        };
        var result = await context.ExecuteAsync(cancellation.Token);

        Assert.True(result.IsSuccess);
        Assert.Equal(PaymentStatus.Captured, result.Value.PaymentStatus);
        Assert.Equal(cancellation.Token, context.UnitOfWork.SaveTokens[0]);
        Assert.False(context.UnitOfWork.SaveTokens[1].CanBeCanceled);
    }

    [Fact]
    public async Task Execute_AfterConfirmedFailure_AdvancesOnlyOnNextInvocation()
    {
        var context = new ExecutionTestContext();
        context.Provider.Result = ProviderOperationResult.Failed("declined");
        var first = await context.ExecuteAsync();
        Assert.True(first.IsSuccess);
        Assert.Equal(FirstProvider, Assert.Single(context.Provider.Requests).ProviderAccountId);
        Assert.Equal(PaymentOperationStatus.Processing, first.Value.OperationStatus);

        context.Provider.Result = ProviderOperationResult.Succeeded("second-provider-tx");
        var second = await context.ExecuteAsync();
        Assert.True(second.IsSuccess);
        Assert.Equal(PaymentStatus.Captured, second.Value.PaymentStatus);
        Assert.Equal(SecondProvider, context.Provider.Requests[1].ProviderAccountId);
        Assert.Equal(2, context.Provider.Requests.Count);
        Assert.Equal(1, context.Routing.Calls);
    }

    [Fact]
    public async Task Execute_WhenSkippedProviderRecovers_ContinuesSavedRouteWithoutReturningToIt()
    {
        var context = new ExecutionTestContext();
        context.Payment.SetOperationRoute(context.Operation.Id, 1, 1,
            [FirstProvider, SecondProvider, ThirdProvider], null, "Previously saved route");
        context.Configuration.Snapshot = context.CreateConfiguration(firstActive: false);
        context.Provider.Result = ProviderOperationResult.Failed("declined");
        await context.ExecuteAsync();
        context.Configuration.Snapshot = context.CreateConfiguration(version: 2);
        context.Provider.Result = ProviderOperationResult.Succeeded("third-provider-tx");
        var result = await context.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { SecondProvider, ThirdProvider }, context.Provider.Requests.Select(request => request.ProviderAccountId));
        Assert.Equal(1L, context.Operation.Route!.ConfigurationVersion);
        Assert.Equal(0, context.Routing.Calls);
    }

    [Fact]
    public async Task Execute_WithProviderToken_UsesOnlyItsOwningAccount()
    {
        var context = new ExecutionTestContext(PaymentMethod.FromProviderToken(Guid.NewGuid()));
        context.Availability.Result = new PaymentMethodAvailability(true, SecondProvider);
        var result = await context.ExecuteAsync();

        Assert.True(result.IsSuccess);
        var request = Assert.Single(context.Provider.Requests);
        Assert.Equal(SecondProvider, request.ProviderAccountId);
        Assert.Equal(context.Payment.PaymentMethod, request.PaymentMethodSource);
        Assert.Equal(SecondProvider, Assert.Single(context.Operation.Route!.Steps).ProviderAccountId);
    }

    [Fact]
    public async Task Execute_WhenOperationAlreadySucceeded_ReplaysWithoutCallingDependenciesAgain()
    {
        var context = new ExecutionTestContext();
        var first = await context.ExecuteAsync();
        var replay = await context.ExecuteAsync();

        Assert.True(first.IsSuccess);
        Assert.True(replay.IsSuccess);
        Assert.Equal(first.Value, replay.Value);
        Assert.Single(context.Provider.Requests);
        Assert.Equal(1, context.Configuration.Calls);
        Assert.Equal(2, context.UnitOfWork.SaveCalls);
    }
}
