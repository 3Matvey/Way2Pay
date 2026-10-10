using Way2Pay.Payments.Domain.Common;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.Configuration;

/// <summary>A merchant provider account's availability and payment capabilities, without credentials.</summary>
/// <remarks>
/// Capabilities are explicit allowlists; an empty list permits no values of that kind.
/// Collection order does not determine routing priority. Payment-method bindings are checked separately.
/// </remarks>
public sealed record ProviderAccountConfiguration
{
    public Guid ProviderAccountId { get; }
    /// <summary>A stable provider code identifying the gateway adapter, not an account or a secret.</summary>
    public string ProviderCode { get; }
    public bool IsActive { get; }
    public IReadOnlyList<PaymentOperationType> SupportedOperations { get; }
    public IReadOnlyList<CurrencyCode> SupportedCurrencies { get; }
    public IReadOnlyList<PaymentMethodType> SupportedPaymentMethods { get; }

    public ProviderAccountConfiguration(
        Guid providerAccountId, string providerCode, bool isActive,
        IEnumerable<PaymentOperationType> supportedOperations, IEnumerable<CurrencyCode> supportedCurrencies,
        IEnumerable<PaymentMethodType> supportedPaymentMethods)
    {
        ProviderAccountId = Guard.RequiredId(providerAccountId);
        ProviderCode = Guard.Required(providerCode);
        IsActive = isActive;
        SupportedOperations = Array.AsReadOnly(supportedOperations.Distinct().ToArray());
        SupportedCurrencies = Array.AsReadOnly(supportedCurrencies.Distinct().ToArray());
        SupportedPaymentMethods = Array.AsReadOnly(supportedPaymentMethods.Distinct().ToArray());
        if (SupportedCurrencies.Any(value => value is null))
            throw new ArgumentException("Supported currencies cannot contain null values.", nameof(supportedCurrencies));
    }
}
