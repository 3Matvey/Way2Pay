using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Application.Payments.StartPaymentOperation;

/// <param name="MerchantId">Must come from the authenticated merchant context.</param>
public sealed record StartPaymentOperationRequest(
    Guid MerchantId, Guid PaymentId, PaymentOperationType Type,
    decimal Amount, string Currency, string IdempotencyKey);
