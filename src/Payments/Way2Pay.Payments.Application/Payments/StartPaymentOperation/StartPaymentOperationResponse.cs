using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

public sealed record StartPaymentOperationResponse(
    Guid PaymentId, Guid OperationId, PaymentOperationStatus Status);
