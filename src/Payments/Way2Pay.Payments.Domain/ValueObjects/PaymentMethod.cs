namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>An immutable reference to a payment credential source managed by CardVault.</summary>
/// <remarks>
/// Contains neither card credentials nor the provider token itself.
/// Application checks source ownership, availability and provider-account compatibility.
/// </remarks>
public sealed record PaymentMethod
{
    public PaymentMethodType Type { get; private init; }
    public PaymentMethodSourceKind SourceKind { get; private init; }

    /// <summary>The internal card token ID or provider-token reference ID, depending on SourceKind.</summary>
    /// <remarks>Knowing this identifier does not authorize access to the credentials.</remarks>
    public Guid SourceId { get; private init; }

    private PaymentMethod(PaymentMethodSourceKind sourceKind, Guid sourceId)
    {
        Type = PaymentMethodType.Card;
        SourceKind = sourceKind;
        SourceId = Guard.RequiredId(sourceId);
    }

    /// <summary>References a stored card whose credentials are encrypted by CardVault.</summary>
    public static PaymentMethod FromVaultCard(Guid cardTokenId) =>
        new(PaymentMethodSourceKind.VaultCard, cardTokenId);

    /// <summary>References a provider token stored by CardVault and scoped to its provider account.</summary>
    public static PaymentMethod FromProviderToken(Guid providerTokenReferenceId) =>
        new(PaymentMethodSourceKind.ProviderToken, providerTokenReferenceId);
}
