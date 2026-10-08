namespace Way2Pay.Payments.Domain.Payments.Operations;

public enum PaymentOperationStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Unknown
}
