namespace Way2Pay.Payments.Domain.Payments;

/// <summary>The requested operation violates the payment's business rules.</summary>
public sealed class PaymentOperationNotAllowedException(string message) : InvalidOperationException(message);
