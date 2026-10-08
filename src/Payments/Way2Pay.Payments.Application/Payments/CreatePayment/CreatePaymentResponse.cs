using Way2Pay.Payments.Domain.Payments;

namespace Way2Pay.Payments.Application.Payments.CreatePayment;

public sealed record CreatePaymentResponse(Guid PaymentId, PaymentStatus Status);
