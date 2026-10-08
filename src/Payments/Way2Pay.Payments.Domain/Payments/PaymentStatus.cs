namespace Way2Pay.Payments.Domain.Payments;

/// <summary>The confirmed financial state, independent of a pending operation.</summary>
public enum PaymentStatus
{
    Created,
    Authorized,
    Captured,
    PartiallyRefunded,
    Refunded,
    Canceled,
    /// <summary>Initial processing ended without a monetary effect; no further operations are allowed.</summary>
    Failed
}
