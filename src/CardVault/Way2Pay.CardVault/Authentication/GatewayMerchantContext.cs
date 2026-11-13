namespace Way2Pay.CardVault.Authentication;

/// <summary>Merchant identity forwarded by Gateway. This requires network isolation of CardVault.</summary>
internal sealed record GatewayMerchantContext(Guid MerchantId)
{
    private const string MerchantIdHeader = "X-Way2Pay-Merchant-Id";

    internal static GatewayMerchantContext? FromHeaders(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue(MerchantIdHeader, out var values) || values.Count != 1)
            return null;

        return Guid.TryParse(values[0], out var merchantId) && merchantId != Guid.Empty
            ? new GatewayMerchantContext(merchantId)
            : null;
    }
}
