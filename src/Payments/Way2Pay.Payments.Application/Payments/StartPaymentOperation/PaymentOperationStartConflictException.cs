namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

/// <summary>Another operation start with the same merchant/key pair has already committed.</summary>
public sealed class PaymentOperationStartConflictException()
    : Exception("An operation start with the same merchant and idempotency key already exists.");
