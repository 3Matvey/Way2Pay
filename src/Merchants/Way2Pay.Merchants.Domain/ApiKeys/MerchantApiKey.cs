using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.Merchants.Domain.ApiKeys;

/// <summary>A separately managed merchant credential containing a secret verifier, never the original API key.</summary>
/// <remarks>Secret generation, hashing and verification belong outside this entity.</remarks>
public sealed class MerchantApiKey : Entity
{
    public Guid MerchantId { get; private init; }

    public string Name
    {
        get;
        private init
        {
            var name = Guard.Required(value).Trim();
            if (name.Length > 200)
                throw new ArgumentException("API key name must contain at most 200 characters.", nameof(value));
            field = name;
        }
    } = string.Empty;

    public string SecretHash { get; private init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? ExpiresAt { get; private init; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private MerchantApiKey() { }

    public MerchantApiKey(Guid merchantId, string name, string secretHash,
        DateTimeOffset createdAt, DateTimeOffset? expiresAt = null)
    {
        if (expiresAt.HasValue && expiresAt.Value <= createdAt)
            throw new ArgumentOutOfRangeException(nameof(expiresAt), "Expiration must be after creation.");
        MerchantId = Guard.RequiredId(merchantId);
        Name = name;
        SecretHash = Guard.Required(secretHash);
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>Checks the current revocation state and validity period; merchant status is checked separately.</summary>
    public bool IsValidAt(DateTimeOffset now) =>
        RevokedAt is null && now >= CreatedAt && (!ExpiresAt.HasValue || now < ExpiresAt.Value);

    /// <summary>Irreversibly revokes this key; repeated revocation preserves the original timestamp.</summary>
    public void Revoke(DateTimeOffset revokedAt)
    {
        if (RevokedAt.HasValue)
            return;

        if (revokedAt < CreatedAt)
            throw new ArgumentOutOfRangeException(nameof(revokedAt), "Revocation cannot precede creation.");

        RevokedAt = revokedAt;
    }
}
