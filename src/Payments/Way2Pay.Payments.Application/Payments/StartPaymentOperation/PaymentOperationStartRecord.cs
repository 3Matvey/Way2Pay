using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

public sealed record PaymentOperationStartRecord(
    Guid MerchantId, string Key, Guid PaymentId,
    PaymentOperationType Type, Money Amount, Guid OperationId);
