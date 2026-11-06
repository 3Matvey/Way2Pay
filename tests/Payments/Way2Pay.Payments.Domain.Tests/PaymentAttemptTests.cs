using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using static Way2Pay.Payments.Domain.Tests.PaymentTestData;

namespace Way2Pay.Payments.Domain.Tests;

public sealed class PaymentAttemptTests
{
    [Fact]
    public void RecordAttemptFailure_WhenConfirmed_AllowsNextProviderWithoutCapturingMoney()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.RecordAttemptFailure(operation.Id, attempt.Id, "declined", Now, "failed-tx");
        var nextAttempt = payment.StartAttempt(operation.Id, NextProviderId);

        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
        Assert.Equal("declined", attempt.FailureCode);
        Assert.Equal("failed-tx", attempt.ProviderTransactionId);
        Assert.Equal(Now, attempt.CompletedAt);
        Assert.Equal(NextProviderId, nextAttempt.ProviderAccountId);
        Assert.Equal(2, nextAttempt.Number);
        Assert.Equal(PaymentStatus.Created, payment.Status);
        Assert.Equal(Amount(0), payment.Balance.Captured);
    }

    [Fact]
    public void MarkAttemptUnknown_LeavesMonetaryOutcomeUnconfirmed()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.MarkAttemptUnknown(operation.Id, attempt.Id, "pending-tx");

        Assert.Equal(PaymentAttemptStatus.Unknown, attempt.Status);
        Assert.Equal(PaymentOperationStatus.Unknown, operation.Status);
        Assert.Equal("pending-tx", attempt.ProviderTransactionId);
        Assert.Null(attempt.CompletedAt);
        Assert.Null(operation.CompletedAt);
        Assert.Equal(Amount(0), payment.Balance.Captured);
    }

    [Fact]
    public void UnknownAttempt_BlocksRetryAndFinalFailure()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.MarkAttemptUnknown(operation.Id, attempt.Id);
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt(operation.Id, NextProviderId));
        Assert.Throws<InvalidOperationException>(() => payment.FailOperation(operation.Id, Now));
        Assert.Equal(PaymentOperationStatus.Unknown, operation.Status);
        Assert.Single(operation.Attempts);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void RecordAttemptSuccess_WhenUnknown_ResolvesOriginalAttemptAndCapturesAmount()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.MarkAttemptUnknown(operation.Id, attempt.Id, "pending-tx");
        payment.RecordAttemptSuccess(operation.Id, attempt.Id, "pending-tx", Now);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(payment.Amount, payment.Balance.Captured);
        Assert.Equal(PaymentOperationStatus.Succeeded, operation.Status);
        Assert.Equal(PaymentAttemptStatus.Succeeded, attempt.Status);
        Assert.Equal(Now, attempt.CompletedAt);
        Assert.Single(operation.Attempts);
    }

    [Fact]
    public void RecordAttemptFailure_WhenUnknown_AllowsRetryAndRetainsTransactionId()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.MarkAttemptUnknown(operation.Id, attempt.Id, "pending-tx");
        payment.RecordAttemptFailure(operation.Id, attempt.Id, "declined", Now);
        var nextAttempt = payment.StartAttempt(operation.Id, NextProviderId);

        Assert.Equal("pending-tx", attempt.ProviderTransactionId);
        Assert.Equal(PaymentAttemptStatus.Failed, attempt.Status);
        Assert.Equal(PaymentAttemptStatus.Processing, nextAttempt.Status);
        Assert.Equal(Amount(0), payment.Balance.Captured);
    }

    [Fact]
    public void RecordAttemptSuccess_WhenTransactionIdChanges_RejectsWithoutResolvingAttempt()
    {
        var (payment, operation, attempt) = StartCharge();
        payment.MarkAttemptUnknown(operation.Id, attempt.Id, "original-tx");
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() =>
            payment.RecordAttemptSuccess(operation.Id, attempt.Id, "different-tx", Now));
        Assert.Equal("original-tx", attempt.ProviderTransactionId);
        Assert.Equal(PaymentAttemptStatus.Unknown, attempt.Status);
        Assert.Equal(Amount(0), payment.Balance.Captured);
        Assert.Equal(version, payment.Version);
    }

    [Theory]
    [InlineData(PaymentOperationType.Charge)]
    [InlineData(PaymentOperationType.Refund)]
    public void RecordAttemptSuccess_WhenRepeated_RejectsWithoutApplyingMoneyTwice(PaymentOperationType type)
    {
        var payment = type == PaymentOperationType.Refund ? CreateCapturedPayment() : CreatePayment();
        var operation = CompleteOperation(payment, type);
        var attempt = Assert.Single(operation.Attempts);
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() => payment.RecordAttemptSuccess(
            operation.Id, attempt.Id, attempt.ProviderTransactionId!, Now));
        Assert.Equal(Amount(100), payment.Balance.Captured);
        Assert.Equal(type == PaymentOperationType.Refund ? Amount(100) : Amount(0), payment.Balance.Refunded);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void StartAttempt_WithoutSavedRoute_Rejects()
    {
        var payment = CreatePayment();
        var operation = payment.StartOperation(PaymentOperationType.Charge, payment.Amount);
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt(operation.Id, ProviderId));
        Assert.Empty(operation.Attempts);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void StartAttempt_WhenProviderIsOutsideRoute_Rejects()
    {
        var payment = CreatePayment();
        var operation = StartRoutedOperation(payment, PaymentOperationType.Charge);
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() => payment.StartAttempt(operation.Id, NextProviderId));
        Assert.Empty(operation.Attempts);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void SetOperationRoute_WhenAlreadyAssigned_RejectsReplacement()
    {
        var payment = CreatePayment();
        var operation = StartRoutedOperation(payment, PaymentOperationType.Charge);
        var version = payment.Version;

        Assert.Throws<InvalidOperationException>(() => payment.SetOperationRoute(
            operation.Id, 2, 2, [NextProviderId], null, "Replacement route"));
        Assert.Equal(ProviderId, Assert.Single(operation.Route!.Steps).ProviderAccountId);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void StateChanges_IncludingOperationsAndAttempts_IncrementAggregateVersion()
    {
        var payment = CreatePayment();
        var operation = payment.StartOperation(PaymentOperationType.Charge, payment.Amount);
        Assert.Equal(1L, payment.Version);
        payment.SetOperationRoute(operation.Id, 1, 1, [ProviderId, NextProviderId], null, "Test route");
        Assert.Equal(2L, payment.Version);
        var attempt = payment.StartAttempt(operation.Id, ProviderId);
        Assert.Equal(3L, payment.Version);
        payment.MarkAttemptUnknown(operation.Id, attempt.Id);
        Assert.Equal(4L, payment.Version);
        payment.RecordAttemptFailure(operation.Id, attempt.Id, "declined", Now);
        Assert.Equal(5L, payment.Version);
        var nextAttempt = payment.StartAttempt(operation.Id, NextProviderId);
        Assert.Equal(6L, payment.Version);
        payment.RecordAttemptSuccess(operation.Id, nextAttempt.Id, "captured-tx", Now);
        Assert.Equal(7L, payment.Version);
    }
}
