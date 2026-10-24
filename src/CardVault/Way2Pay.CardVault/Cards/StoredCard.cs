using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.CardVault.Cards;

/// <summary>A merchant-owned card identified externally by its inherited Id as the CardTokenId.</summary>
/// <remarks>Stores encrypted PAN and display metadata only. Plaintext PAN and CVV are not part of this entity.</remarks>
public sealed class StoredCard : AuditableEntity
{
    public Guid MerchantId { get; private init; }

    /// <summary>An optional customer reference in the merchant's system, scoped to MerchantId.</summary>
    public string? ExternalCustomerId
    {
        get;
        private init
        {
            if (value is not null && (string.IsNullOrWhiteSpace(value) || value.Length > 450))
                throw new ArgumentException("External customer identifier must be non-blank and at most 450 characters.", nameof(value));
            field = value;
        }
    }

    /// <summary>The encrypted PAN returned by Vault Transit, never the plaintext card number.</summary>
    public string PanCiphertext { get; private init; } = string.Empty;

    public string LastFour
    {
        get;
        private init
        {
            Guard.Required(value);
            if (value.Length != 4 || value.Any(character => character is < '0' or > '9'))
                throw new ArgumentException("LastFour must contain exactly four ASCII digits.", nameof(value));

            field = value;
        }
    } = string.Empty;
    public CardExpiration Expiration { get; private init; } = null!;
    public CardStatus Status { get; private set; }

    private StoredCard() { }

    /// <summary>Creates an active card from encrypted credentials and validated display metadata.</summary>
    /// <remarks>
    /// The registration handler must obtain ciphertext from Transit and derive LastFour from the same PAN.
    /// This entity cannot verify that the ciphertext and display metadata describe the same card.
    /// </remarks>
    public StoredCard(
        Guid merchantId, string panCiphertext, string lastFour,
        CardExpiration expiration, string? externalCustomerId = null)
    {
        MerchantId = Guard.RequiredId(merchantId);
        PanCiphertext = Guard.Required(panCiphertext);
        ArgumentNullException.ThrowIfNull(expiration);
        LastFour = lastFour;
        Expiration = expiration;
        ExternalCustomerId = externalCustomerId;
        Status = CardStatus.Active;
    }

    /// <summary>Prevents further use without deleting the stored credentials.</summary>
    public void Block() => Status = CardStatus.Blocked;

    /// <summary>Removes the manual block; expiration and access permissions still apply.</summary>
    public void Unblock() => Status = CardStatus.Active;
}
