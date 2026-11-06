using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;
using static Way2Pay.Payments.Domain.Tests.PaymentTestData;

namespace Way2Pay.Payments.Domain.Tests;

public sealed class PaymentLifecycleTests
{
    [Fact]
    public void Charge_WhenSuccessful_CapturesAmountAndCompletesOperation()
    {
        var payment = CreatePayment();
        var operation = CompleteOperation(payment, PaymentOperationType.Charge);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(payment.Amount, payment.Balance.Captured);
        Assert.Equal(Amount(0), payment.Balance.Authorized);
        Assert.Equal(PaymentOperationStatus.Succeeded, operation.Status);
        Assert.Equal(PaymentAttemptStatus.Succeeded, Assert.Single(operation.Attempts).Status);
        Assert.Equal(Now, operation.CompletedAt);
    }

    [Fact]
    public void Authorize_WhenSuccessful_ReservesAmountWithoutCapturing()
    {
        var payment = CreatePayment();
        CompleteOperation(payment, PaymentOperationType.Authorize);

        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal(payment.Amount, payment.Balance.Authorized);
        Assert.Equal(Amount(0), payment.Balance.Captured);
        Assert.Equal(Amount(0), payment.Balance.Refundable);
    }

    [Fact]
    public void Capture_WhenAuthorized_ReplacesReservationWithCapturedAmount()
    {
        var payment = CreateAuthorizedPayment();
        CompleteOperation(payment, PaymentOperationType.Capture);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(Amount(0), payment.Balance.Authorized);
        Assert.Equal(payment.Amount, payment.Balance.Captured);
        Assert.Equal(payment.Amount, payment.Balance.Refundable);
    }

    [Fact]
    public void Void_WhenAuthorized_CancelsWithoutCapturing()
    {
        var payment = CreateAuthorizedPayment();
        CompleteOperation(payment, PaymentOperationType.Void);

        Assert.Equal(PaymentStatus.Canceled, payment.Status);
        Assert.Equal(Amount(0), payment.Balance.Authorized);
        Assert.Equal(Amount(0), payment.Balance.Captured);
        Assert.Equal(Amount(0), payment.Balance.Refundable);
    }

    [Theory]
    [InlineData(PaymentOperationStatus.Pending)]
    [InlineData(PaymentOperationStatus.Processing)]
    [InlineData(PaymentOperationStatus.Unknown)]
    public void StartOperation_WhenAnotherOperationIsActive_RejectsWithoutChangingVersion(
        PaymentOperationStatus status)
    {
        var payment = CreatePayment();
        var operation = StartRoutedOperation(payment, PaymentOperationType.Charge);
        if (status != PaymentOperationStatus.Pending)
        {
            var attempt = payment.StartAttempt(operation.Id, ProviderId);
            if (status == PaymentOperationStatus.Unknown)
                payment.MarkAttemptUnknown(operation.Id, attempt.Id);
        }
        var version = payment.Version;

        Assert.Throws<PaymentOperationNotAllowedException>(() =>
            payment.StartOperation(PaymentOperationType.Charge, payment.Amount));
        Assert.Equal(version, payment.Version);
        Assert.Single(payment.Operations);
    }

    [Theory]
    [InlineData(PaymentOperationType.Capture)]
    [InlineData(PaymentOperationType.Void)]
    public void StartOperation_WhenAmountIsLessThanAuthorization_Rejects(PaymentOperationType type)
    {
        var payment = CreateAuthorizedPayment();
        var version = payment.Version;

        Assert.Throws<PaymentOperationNotAllowedException>(() => payment.StartOperation(type, Amount(50)));
        Assert.Equal(version, payment.Version);
        Assert.Equal(payment.Amount, payment.Balance.Authorized);
        Assert.Single(payment.Operations);
    }

    [Fact]
    public void StartOperation_WhenCurrencyDiffers_RejectsWithoutAddingOperation()
    {
        var payment = CreatePayment();
        var amount = new Money(100, new CurrencyCode("USD"));

        Assert.Throws<PaymentOperationNotAllowedException>(() =>
            payment.StartOperation(PaymentOperationType.Charge, amount));
        Assert.Empty(payment.Operations);
        Assert.Equal(0L, payment.Version);
    }

    [Theory]
    [InlineData(PaymentOperationType.Authorize)]
    [InlineData(PaymentOperationType.Charge)]
    public void StartOperation_WhenInitialProcessingHasFailed_Rejects(PaymentOperationType type)
    {
        var (payment, operation, attempt) = StartCharge();
        payment.RecordAttemptFailure(operation.Id, attempt.Id, "declined", Now);
        payment.FailOperation(operation.Id, Now);
        var version = payment.Version;

        Assert.Throws<PaymentOperationNotAllowedException>(() => payment.StartOperation(type, payment.Amount));
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(Amount(0), payment.Balance.Captured);
        Assert.Equal(version, payment.Version);
    }

    [Theory]
    [InlineData(PaymentOperationType.Capture)]
    [InlineData(PaymentOperationType.Void)]
    [InlineData(PaymentOperationType.Refund)]
    public void FollowUpOperation_UsesOriginalTransactionAndRejectsAnotherProvider(PaymentOperationType type)
    {
        var payment = type == PaymentOperationType.Refund ? CreateCapturedPayment() : CreateAuthorizedPayment();
        var originalAttempt = Assert.Single(Assert.Single(payment.Operations).Attempts);
        var operation = payment.StartOperation(type, payment.Amount);
        var version = payment.Version;

        Assert.Equal(originalAttempt.ProviderTransactionId, operation.OriginalProviderTransactionId);
        Assert.Equal(originalAttempt.ProviderAccountId, payment.GetOriginalProviderAccountId(operation.Id));
        Assert.Throws<InvalidOperationException>(() => payment.SetOperationRoute(
            operation.Id, 1, null, [NextProviderId], null, "Invalid follow-up route"));
        Assert.Null(operation.Route);
        Assert.Equal(version, payment.Version);
    }
}
