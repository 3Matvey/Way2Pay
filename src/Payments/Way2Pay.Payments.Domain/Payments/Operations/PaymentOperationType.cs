namespace Way2Pay.Payments.Domain.Payments.Operations;

public enum PaymentOperationType
{
    Authorize,
    Charge,
    Capture,
    Void,
    Refund
}
