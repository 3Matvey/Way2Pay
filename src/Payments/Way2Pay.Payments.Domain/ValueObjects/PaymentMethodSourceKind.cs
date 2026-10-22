namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>Identifies how CardVault supplies credentials for a payment method.</summary>
public enum PaymentMethodSourceKind
{
    /// <summary>A stored card identified by an internal CardVault token.</summary>
    VaultCard,

    /// <summary>A protected reference to a token issued for a specific provider account.</summary>
    ProviderToken
}
