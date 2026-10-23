using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.CardVault.Authentication;

/// <summary>The trusted scope of an authorized card-registration request.</summary>
/// <remarks>
/// Authorization places this object in HttpContext.Features only after validating the registration
/// permission, its expiration and checkout binding. It must not be constructed from browser-supplied
/// merchant or customer identifiers. This object describes the verified scope; it is not a token validator.
/// </remarks>
internal sealed record CardRegistrationContext
{
    public Guid MerchantId { get; }
    public Guid CheckoutId { get; }
    public string? ExternalCustomerId { get; }

    public CardRegistrationContext(Guid merchantId, Guid checkoutId, string? externalCustomerId = null)
    {
        MerchantId = Guard.RequiredId(merchantId);
        CheckoutId = Guard.RequiredId(checkoutId);
        ExternalCustomerId = externalCustomerId is null ? null : Guard.Required(externalCustomerId);
    }
}
