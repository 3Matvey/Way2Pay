using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Domain.Tests;

internal static class PaymentTestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    public static readonly Guid ProviderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid NextProviderId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static Money Amount(decimal value) => new(value, new CurrencyCode("BYN"));

    public static Payment CreatePayment() =>
        new(Guid.NewGuid(), Amount(100), PaymentMethod.FromVaultCard(Guid.NewGuid()));

    public static Payment CreateAuthorizedPayment()
    {
        var payment = CreatePayment();
        CompleteOperation(payment, PaymentOperationType.Authorize);
        return payment;
    }

    public static Payment CreateCapturedPayment()
    {
        var payment = CreatePayment();
        CompleteOperation(payment, PaymentOperationType.Charge);
        return payment;
    }

    public static PaymentOperation StartRoutedOperation(
        Payment payment, PaymentOperationType type, Money? amount = null)
    {
        var operation = payment.StartOperation(type, amount ?? payment.Amount);
        var initial = type is PaymentOperationType.Authorize or PaymentOperationType.Charge;
        payment.SetOperationRoute(operation.Id, 1, initial ? 1L : null, [ProviderId], null, "Test route");
        return operation;
    }

    public static PaymentOperation CompleteOperation(
        Payment payment, PaymentOperationType type, Money? amount = null)
    {
        var operation = StartRoutedOperation(payment, type, amount);
        var attempt = payment.StartAttempt(operation.Id, ProviderId);
        payment.RecordAttemptSuccess(operation.Id, attempt.Id, $"tx-{operation.Id:N}", Now);
        return operation;
    }

    public static (Payment Payment, PaymentOperation Operation, PaymentAttempt Attempt) StartCharge()
    {
        var payment = CreatePayment();
        var operation = payment.StartOperation(PaymentOperationType.Charge, payment.Amount);
        payment.SetOperationRoute(operation.Id, 1, 1, [ProviderId, NextProviderId], null, "Test fallback route");
        return (payment, operation, payment.StartAttempt(operation.Id, ProviderId));
    }
}
