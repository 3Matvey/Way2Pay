namespace Way2Pay.Merchants.Application.ApiKeys.RevokeApiKey;

public static class RevokeApiKeyErrors
{
    public static Error InvalidMerchantId() => Error.BadRequest(
        "Merchants.RevokeApiKey.InvalidMerchantId", "A merchant identifier is required.");

    public static Error InvalidApiKeyId() => Error.BadRequest(
        "Merchants.RevokeApiKey.InvalidApiKeyId", "An API key identifier is required.");

    public static Error NotFound() => Error.NotFound(
        "Merchants.RevokeApiKey.NotFound", "The API key was not found.");
}
