namespace Way2Pay.Orchestrator.Core.Payments.CreatePayment;

public sealed record CreatePaymentResult(Guid PaymentId, PaymentStatus Status);
