using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.CreatePayment;

public sealed record PaymentCreationRecord(Guid MerchantId, string Key, Money Amount, Guid PaymentId);
