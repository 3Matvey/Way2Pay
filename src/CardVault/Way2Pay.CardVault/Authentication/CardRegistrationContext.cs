using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.CardVault.Authentication;

/// <summary>The trusted scope of an authorized card-registration request.</summary>
/// <remarks>
/// Authorization places this object in HttpContext.Features only after validating the registration
/// permission, its expiration and merchant binding. It must not be constructed from browser-supplied
/// session, merchant or customer identifiers. Values come from the verified session record.
/// This object describes the verified scope; it is not a token validator.
/// </remarks>
internal sealed record CardRegistrationContext
{
    public Guid SessionId { get; }
    public Guid MerchantId { get; }
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

    public CardRegistrationContext(Guid sessionId, Guid merchantId, string? externalCustomerId = null)
    {
        SessionId = Guard.RequiredId(sessionId);
        MerchantId = Guard.RequiredId(merchantId);
        ExternalCustomerId = externalCustomerId;
    }
}
