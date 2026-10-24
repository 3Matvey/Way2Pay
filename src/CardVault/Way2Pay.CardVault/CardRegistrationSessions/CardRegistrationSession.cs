using Way2Pay.CardVault.Cards;
using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.CardVault.CardRegistrationSessions;

/// <summary>Authorizes registration of one card for a merchant without requiring a payment or checkout.</summary>
/// <remarks>Stores a token verifier and registration result, never the raw token, PAN or CVV.</remarks>
public sealed class CardRegistrationSession : Entity
{
    public Guid MerchantId { get; private init; }
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

    /// <summary>The uppercase hexadecimal SHA-256 hash of a cryptographically random registration token.</summary>
    public string TokenHash 
    { 
        get; 
        private init
        {
            Guard.Required(value);
            if (value.Length != 64 || value.Any(character => !char.IsAsciiHexDigit(character)))
                throw new ArgumentException("Token hash must contain exactly 64 hexadecimal characters.", nameof(value));

            field = value.ToUpperInvariant();
        } 
    } = string.Empty;

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset ExpiresAt { get; private init; }
    public CardRegistrationSessionStatus Status { get; private set; }

    /// <summary>The completed registration result; repeated requests must reuse it instead of creating another card.</summary>
    public Guid? StoredCardId { get; private set; }

    /// <summary>An application-managed concurrency token incremented by successful state transitions.</summary>
    public long Version { get; private set; }

    private CardRegistrationSession() { }

    /// <summary>Creates a pending registration with a caller-supplied clock value and token hash.</summary>
    public CardRegistrationSession(
        Guid merchantId, string tokenHash, DateTimeOffset createdAt,
        DateTimeOffset expiresAt, string? externalCustomerId = null)
    {
        if (expiresAt <= createdAt)
            throw new ArgumentOutOfRangeException(nameof(expiresAt), "Expiration must be after creation.");

        MerchantId = Guard.RequiredId(merchantId);
        TokenHash = tokenHash;
        ExternalCustomerId = externalCustomerId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        Status = CardRegistrationSessionStatus.Pending;
    }

    public bool IsExpiredAt(DateTimeOffset now) => now >= ExpiresAt;

    /// <summary>Records the first successful registration for the session's merchant and customer.</summary>
    /// <remarks>
    /// The handler must save the card and this transition in one transaction and handle concurrency conflicts.
    /// Completed-session replay is handled by returning the saved result, not by calling Complete again.
    /// </remarks>
    public void Complete(StoredCard card, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(card);
        EnsurePending(now);
        if (card.MerchantId != MerchantId || card.ExternalCustomerId != ExternalCustomerId)
            throw new InvalidOperationException("The card does not belong to the session's merchant and customer.");
        StoredCardId = card.Id;
        Status = CardRegistrationSessionStatus.Completed;
        Version++;
    }

    /// <summary>Revokes an unused registration; repeated revocation does not change the version.</summary>
    public void Revoke()
    {
        if (Status == CardRegistrationSessionStatus.Completed)
            throw new InvalidOperationException("A completed registration cannot be revoked.");
        if (Status == CardRegistrationSessionStatus.Revoked)
            return;
        Status = CardRegistrationSessionStatus.Revoked;
        Version++;
    }

    private void EnsurePending(DateTimeOffset now)
    {
        if (Status != CardRegistrationSessionStatus.Pending || IsExpiredAt(now))
            throw new InvalidOperationException("The registration session is not pending or has expired.");
    }
}
