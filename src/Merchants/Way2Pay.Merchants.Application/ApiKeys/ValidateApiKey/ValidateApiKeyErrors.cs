namespace Way2Pay.Merchants.Application.ApiKeys.ValidateApiKey;

public static class ValidateApiKeyErrors
{
    public static Error InvalidApiKey() => Error.AccessUnauthorized(
        "Merchants.ValidateApiKey.InvalidApiKey", "The API key is invalid.");

    public static Error MerchantSuspended() => Error.AccessForbidden(
        "Merchants.ValidateApiKey.MerchantSuspended", "The merchant's external API access is suspended.");
}
