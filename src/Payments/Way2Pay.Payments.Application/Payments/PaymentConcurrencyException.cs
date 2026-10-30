namespace Way2Pay.Payments.Application.Payments;

/// <summary>The loaded aggregate changed concurrently; none of the pending changes were committed.</summary>
public sealed class PaymentConcurrencyException(Exception innerException)
    : Exception("The payment was changed by another command.", innerException);
