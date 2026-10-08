using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.CreatePayment;

public sealed record CreatePaymentRequest(
    Guid MerchantId, decimal Amount, string Currency, string IdempotencyKey, PaymentMethod PaymentMethod);
