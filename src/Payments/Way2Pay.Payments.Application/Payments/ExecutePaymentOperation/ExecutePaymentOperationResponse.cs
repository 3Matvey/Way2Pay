using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Attempts;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Application.Payments.ExecutePaymentOperation;

/// <summary>The current payment, operation and latest attempt states, including unresolved outcomes.</summary>
public sealed record ExecutePaymentOperationResponse(
    Guid PaymentId, Guid OperationId, PaymentStatus PaymentStatus, PaymentOperationStatus OperationStatus,
    Guid? AttemptId, PaymentAttemptStatus? AttemptStatus);
