using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Operations;
using static Way2Pay.Payments.Domain.Tests.PaymentTestData;

namespace Way2Pay.Payments.Domain.Tests;

public sealed class PaymentRefundTests
{
    [Fact]
    public void Refund_WhenPartiallySuccessful_UpdatesRefundableBalance()
    {
        var payment = CreateCapturedPayment();
        CompleteOperation(payment, PaymentOperationType.Refund, Amount(30));

        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(Amount(100), payment.Balance.Captured);
        Assert.Equal(Amount(30), payment.Balance.Refunded);
        Assert.Equal(Amount(70), payment.Balance.Refundable);
    }

    [Fact]
    public void Refund_WhenRemainingAmountIsReturned_AccumulatesRefundsAndCompletesPayment()
    {
        var payment = CreateCapturedPayment();
        CompleteOperation(payment, PaymentOperationType.Refund, Amount(30));
        CompleteOperation(payment, PaymentOperationType.Refund, Amount(70));

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(Amount(100), payment.Balance.Captured);
        Assert.Equal(Amount(100), payment.Balance.Refunded);
        Assert.Equal(Amount(0), payment.Balance.Refundable);
    }

    [Fact]
    public void Refund_WhenAmountExceedsRemainingBalance_RejectsWithoutChangingPayment()
    {
        var payment = CreateCapturedPayment();
        CompleteOperation(payment, PaymentOperationType.Refund, Amount(30));
        var version = payment.Version;

        Assert.Throws<PaymentOperationNotAllowedException>(() =>
            payment.StartOperation(PaymentOperationType.Refund, Amount(71)));
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(Amount(30), payment.Balance.Refunded);
        Assert.Equal(Amount(70), payment.Balance.Refundable);
        Assert.Equal(2, payment.Operations.Count);
        Assert.Equal(version, payment.Version);
    }

    [Fact]
    public void Refund_WhenAttemptFails_PreservesCapturedPaymentAndRefundableBalance()
    {
        var payment = CreateCapturedPayment();
        var operation = StartRoutedOperation(payment, PaymentOperationType.Refund, Amount(30));
        var attempt = payment.StartAttempt(operation.Id, ProviderId);
        payment.RecordAttemptFailure(operation.Id, attempt.Id, "refund_declined", Now);
        payment.FailOperation(operation.Id, Now);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(Amount(0), payment.Balance.Refunded);
        Assert.Equal(payment.Amount, payment.Balance.Refundable);
        Assert.Equal(PaymentOperationStatus.Failed, operation.Status);
    }

    [Fact]
    public void Refund_WhenAlreadyFullyRefunded_RejectsAnotherRefund()
    {
        var payment = CreateCapturedPayment();
        CompleteOperation(payment, PaymentOperationType.Refund);
        var version = payment.Version;

        Assert.Throws<PaymentOperationNotAllowedException>(() =>
            payment.StartOperation(PaymentOperationType.Refund, Amount(1)));
        Assert.Equal(Amount(0), payment.Balance.Refundable);
        Assert.Equal(version, payment.Version);
    }
}
