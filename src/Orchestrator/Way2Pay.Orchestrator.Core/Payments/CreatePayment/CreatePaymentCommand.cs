namespace Way2Pay.Orchestrator.Core.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    string? ExternalPaymentId,
    decimal Amount,
    string CurrencyCode,
    string? CountryCode,
    string? Description);
