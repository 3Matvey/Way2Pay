namespace Way2Pay.Orchestrator.Core.Payments;

public sealed class Payment
{
    private Payment(
        Guid id,
        string? externalPaymentId,
        decimal amount,
        string currencyCode,
        string? countryCode,
        string? description,
        DateTime createdAt)
    {
        Id = id;
        ExternalPaymentId = externalPaymentId;
        Amount = amount;
        CurrencyCode = currencyCode;
        CountryCode = countryCode;
        Description = description;
        Status = PaymentStatus.Created;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public string? ExternalPaymentId { get; }
    public decimal Amount { get; }
    public string CurrencyCode { get; }
    public string? CountryCode { get; }
    public string? Description { get; }
    public PaymentStatus Status { get; }
    public DateTime CreatedAt { get; }

    public static Payment Create(
        string? externalPaymentId,
        decimal amount,
        string currencyCode,
        string? countryCode,
        string? description,
        DateTime createdAt)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Payment amount must be positive.");

        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must contain three characters.", nameof(currencyCode));

        if (countryCode is not null && (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2))
            throw new ArgumentException("Country code must contain two characters.", nameof(countryCode));

        if (externalPaymentId is not null && (string.IsNullOrWhiteSpace(externalPaymentId) || externalPaymentId.Length > 255))
            throw new ArgumentException("External payment ID must contain at most 255 characters.", nameof(externalPaymentId));

        if (createdAt.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Creation time must be UTC.", nameof(createdAt));

        return new Payment(
            Guid.NewGuid(),
            externalPaymentId,
            amount,
            currencyCode.ToUpperInvariant(),
            countryCode?.ToUpperInvariant(),
            description,
            createdAt);
    }
}
