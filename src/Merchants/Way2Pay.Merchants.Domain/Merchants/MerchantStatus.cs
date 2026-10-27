namespace Way2Pay.Merchants.Domain.Merchants;

/// <summary>Controls the merchant's external API access, independently of individual API-key revocation.</summary>
public enum MerchantStatus
{
    Active = 0,
    Suspended = 1
}
