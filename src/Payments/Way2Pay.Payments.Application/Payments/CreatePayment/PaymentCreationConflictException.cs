namespace Way2Pay.Payments.Application.Payments.CreatePayment;

/// <summary>Another creation with the same merchant/key pair has already committed.</summary>
public sealed class PaymentCreationConflictException()
    : Exception("A payment creation with the same merchant and idempotency key already exists.");

