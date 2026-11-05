namespace Way2Pay.Gateway.Authentication;

internal static class MerchantApiKeyAuthentication
{
    public const string Scheme = "MerchantApiKey";
    public const string Policy = "MerchantApiKey";
    public const string MerchantIdClaim = "merchant_id";
    public const string MerchantIdHeader = "X-Way2Pay-Merchant-Id";
}
