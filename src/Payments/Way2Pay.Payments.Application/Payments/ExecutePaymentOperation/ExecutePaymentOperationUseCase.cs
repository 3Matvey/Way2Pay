using Way2Pay.Payments.Application.Common.Interfaces;
using Way2Pay.Payments.Application.Common.Results;
using Way2Pay.Payments.Application.Payments.Configuration;
using Way2Pay.Payments.Application.Payments.Providers;
using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Application.Payments.ExecutePaymentOperation;

/// <summary>Persists and executes at most one new provider attempt per invocation.</summary>
/// <remarks>
/// Completed operations and unresolved attempts return their current state without a provider call.
/// Each saved route step is tried at most once by this use case. Only confirmed failures allow advancement.
/// Unavailable steps before the selected step are skipped permanently for this operation.
/// A result-save concurrency conflict allows one reload and save retry, without another provider call.
/// A crash or failed result save leaves the persisted attempt unresolved for reconciliation, never automatic resend.
/// </remarks>
public sealed class ExecutePaymentOperationUseCase(
    IPaymentRepository paymentRepository,
    IMerchantPaymentConfigurationProvider configurationProvider,
    IPaymentRoutingEngine routingEngine,
    IPaymentProviderGateway providerGateway,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IUseCase
{
    public async Task<Result<ExecutePaymentOperationResponse>> ExecuteAsync(
        ExecutePaymentOperationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var validation = ValidateRequest(request);
        if (!validation.IsSuccess)
            return validation.Error;
        var payment = await paymentRepository.GetByIdAsync(request.MerchantId, request.PaymentId, cancellationToken);
        if (payment is null)
            return PaymentErrors.NotFound();
        return await ExecuteOperationAsync(payment, request.OperationId, cancellationToken);
    }

    private static Result ValidateRequest(ExecutePaymentOperationRequest request)
    {
        if (request.MerchantId == Guid.Empty)
            return PaymentErrors.InvalidMerchantId();
        if (request.PaymentId == Guid.Empty)
            return PaymentErrors.InvalidPaymentId();
        if (request.OperationId == Guid.Empty)
            return PaymentErrors.InvalidOperationId();
        return Result.Success();
    }

    private async Task<Result<ExecutePaymentOperationResponse>> ExecuteOperationAsync(
        Payment payment, Guid operationId, CancellationToken cancellationToken)
    {
        var operation = payment.Operations.SingleOrDefault(candidate => candidate.Id == operationId);
        if (operation is null)
            return PaymentErrors.OperationNotFound();
        if (operation.Status is PaymentOperationStatus.Succeeded or PaymentOperationStatus.Failed
            || operation.Attempts.Any(attempt => attempt.Status != PaymentAttemptStatus.Failed))
            return CreateResponse(payment, operation);
        return await PrepareAttemptAsync(payment, operation, cancellationToken);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> PrepareAttemptAsync(
        Payment payment, PaymentOperation operation, CancellationToken cancellationToken)
    {
        var configuration = await configurationProvider.GetAsync(payment.MerchantId, cancellationToken);
        if (configuration is null)
            return PaymentErrors.MerchantUnavailable();
        if (configuration.MerchantId != payment.MerchantId)
            throw new InvalidOperationException("The configuration belongs to a different merchant.");
        var eligible = PaymentExecutionRouting.GetEligibleAccounts(payment, operation, configuration);
        if (operation.Route is null)
            PaymentExecutionRouting.SetRoute(payment, operation, configuration, eligible, routingEngine);
        return await ExecuteNextStepAsync(payment, operation, eligible, cancellationToken);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> ExecuteNextStepAsync(
        Payment payment, PaymentOperation operation, IReadOnlyList<Guid> eligibleAccounts, CancellationToken cancellationToken)
    {
        var remaining = PaymentExecutionRouting.GetRemainingSteps(operation);
        var step = remaining.FirstOrDefault(candidate => eligibleAccounts.Contains(candidate.ProviderAccountId));
        if (step is null)
            return await SaveWithoutAttemptAsync(payment, operation, remaining.Length == 0, cancellationToken);
        var attempt = payment.StartAttempt(operation.Id, step.ProviderAccountId);
        var providerRequest = CreateProviderRequest(payment, operation, attempt);
        var saved = await SaveAsync(payment, operation, cancellationToken);
        if (!saved.IsSuccess)
            return saved.Error;
        return await CallProviderAsync(payment, operation, attempt, providerRequest, cancellationToken);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> SaveWithoutAttemptAsync(
        Payment payment, PaymentOperation operation, bool routeExhausted, CancellationToken cancellationToken)
    {
        if (routeExhausted)
            payment.FailOperation(operation.Id, timeProvider.GetUtcNow());
        var saved = await SaveAsync(payment, operation, cancellationToken);
        if (!saved.IsSuccess || routeExhausted)
            return saved;
        return PaymentErrors.ProviderAccountUnavailable();
    }

    private static ProviderOperationRequest CreateProviderRequest(
        Payment payment, PaymentOperation operation, PaymentAttempt attempt)
    {
        var methodReference = PaymentExecutionRouting.IsInitial(operation)
            ? payment.PaymentMethod.ProviderReferences.Single(reference => reference.ProviderAccountId == attempt.ProviderAccountId)
            : null;
        return new ProviderOperationRequest(payment.MerchantId, payment.Id, operation.Id, attempt.Id,
            attempt.ProviderAccountId, operation.Type, operation.Amount, payment.PaymentMethod.Type,
            operation.OriginalProviderTransactionId, methodReference);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> CallProviderAsync(
        Payment payment, PaymentOperation operation, PaymentAttempt attempt,
        ProviderOperationRequest request, CancellationToken cancellationToken)
    {
        var result = await GetProviderResultAsync(request, cancellationToken);
        var receivedAt = timeProvider.GetUtcNow();
        ApplyProviderResult(payment, operation, attempt, result, receivedAt);
        // Caller cancellation must not discard a monetary outcome obtained after the persisted claim.
        var saved = await SaveAsync(payment, operation, CancellationToken.None);
        if (saved.IsSuccess)
            return saved;
        return await ReloadAndSaveResultAsync(request, result, receivedAt);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> ReloadAndSaveResultAsync(
        ProviderOperationRequest request, ProviderOperationResult result, DateTimeOffset receivedAt)
    {
        var payment = await paymentRepository.GetByIdAsync(request.MerchantId, request.PaymentId, CancellationToken.None);
        if (payment is null)
            return PaymentErrors.NotFound();
        var operation = payment.Operations.SingleOrDefault(candidate => candidate.Id == request.OperationId);
        if (operation is null)
            return PaymentErrors.OperationNotFound();
        return await SaveReloadedResultAsync(payment, operation, request.AttemptId, result, receivedAt);
    }

    private async Task<Result<ExecutePaymentOperationResponse>> SaveReloadedResultAsync(
        Payment payment, PaymentOperation operation, Guid attemptId,
        ProviderOperationResult result, DateTimeOffset receivedAt)
    {
        var attempt = operation.Attempts.Single(candidate => candidate.Id == attemptId);
        var resolution = ShouldApplyResult(operation, attempt, result);
        if (!resolution.IsSuccess)
            return resolution.Error;
        if (!resolution.Value)
            return CreateResponse(payment, operation);
        ApplyProviderResult(payment, operation, attempt, result, receivedAt);
        return await SaveAsync(payment, operation, CancellationToken.None);
    }

    /// <summary>Returns true to apply the result, false to retain the saved state, or a conflicting-result error.</summary>
    private static Result<bool> ShouldApplyResult(
        PaymentOperation operation, PaymentAttempt attempt, ProviderOperationResult result)
    {
        if (attempt.ProviderTransactionId is not null && result.ProviderTransactionId is not null
            && attempt.ProviderTransactionId != result.ProviderTransactionId)
            return PaymentErrors.ProviderResultConflict();
        if (attempt.Status is PaymentAttemptStatus.Succeeded or PaymentAttemptStatus.Failed)
            return ResolveCompletedAttempt(attempt, result);
        return ResolveUnresolvedAttempt(operation, attempt, result);
    }

    private static Result<bool> ResolveCompletedAttempt(PaymentAttempt attempt, ProviderOperationResult result)
    {
        if (result.Outcome == ProviderOperationOutcome.Unknown)
            return false;
        var matches = result.Outcome == ProviderOperationOutcome.Succeeded
            ? attempt.Status == PaymentAttemptStatus.Succeeded
            : attempt.Status == PaymentAttemptStatus.Failed && attempt.FailureCode == result.FailureCode;
        if (!matches || (result.ProviderTransactionId is not null && result.ProviderTransactionId != attempt.ProviderTransactionId))
            return PaymentErrors.ProviderResultConflict();
        return false;
    }

    private static Result<bool> ResolveUnresolvedAttempt(
        PaymentOperation operation, PaymentAttempt attempt, ProviderOperationResult result)
    {
        if (operation.Status is PaymentOperationStatus.Succeeded or PaymentOperationStatus.Failed
            || operation.Attempts.LastOrDefault()?.Id != attempt.Id)
            return PaymentErrors.ProviderResultConflict();
        if (attempt.Status == PaymentAttemptStatus.Unknown && result.Outcome == ProviderOperationOutcome.Unknown)
            return result.ProviderTransactionId is not null && attempt.ProviderTransactionId is null;
        return true;
    }

    private async Task<ProviderOperationResult> GetProviderResultAsync(
        ProviderOperationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await providerGateway.ExecuteAsync(request, cancellationToken);
        }
        catch (Exception exception) when (exception is OperationCanceledException or TimeoutException or HttpRequestException)
        {
            return ProviderOperationResult.Unknown();
        }
    }

    private void ApplyProviderResult(
        Payment payment, PaymentOperation operation, PaymentAttempt attempt,
        ProviderOperationResult result, DateTimeOffset receivedAt)
    {
        switch (result.Outcome)
        {
            case ProviderOperationOutcome.Succeeded:
                payment.RecordAttemptSuccess(operation.Id, attempt.Id, result.ProviderTransactionId!, receivedAt);
                break;
            case ProviderOperationOutcome.Failed:
                payment.RecordAttemptFailure(operation.Id, attempt.Id, result.FailureCode!, receivedAt, result.ProviderTransactionId);
                if (PaymentExecutionRouting.GetRemainingSteps(operation).Length == 0)
                    payment.FailOperation(operation.Id, receivedAt);
                break;
            case ProviderOperationOutcome.Unknown:
                payment.MarkAttemptUnknown(operation.Id, attempt.Id, result.ProviderTransactionId);
                break;
            default:
                throw new InvalidOperationException("The provider returned an unsupported outcome.");
        }
    }

    private async Task<Result<ExecutePaymentOperationResponse>> SaveAsync(
        Payment payment, PaymentOperation operation, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return CreateResponse(payment, operation);
        }
        catch (PaymentConcurrencyException)
        {
            await unitOfWork.DiscardChangesAsync(CancellationToken.None);
            return PaymentErrors.ConcurrencyConflict();
        }
    }

    private static ExecutePaymentOperationResponse CreateResponse(Payment payment, PaymentOperation operation)
    {
        var attempt = operation.Attempts.LastOrDefault();
        return new ExecutePaymentOperationResponse(payment.Id, operation.Id, payment.Status, operation.Status,
            attempt?.Id, attempt?.Status);
    }
}
