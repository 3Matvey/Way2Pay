namespace Way2Pay.Merchants.Application.ApiKeys.IssueApiKey;

public static class IssueApiKeyErrors
{
    public static Error InvalidMerchantId() => Error.BadRequest(
        "Merchants.IssueApiKey.InvalidMerchantId", "A merchant identifier is required.");

    public static Error InvalidName() => Error.BadRequest(
        "Merchants.IssueApiKey.InvalidName", "API key name must be non-blank and at most 200 characters after trimming.");

    public static Error InvalidExpiration() => Error.BadRequest(
        "Merchants.IssueApiKey.InvalidExpiration", "Expiration must be after the issuance time.");

    public static Error MerchantNotFound() => Error.NotFound(
        "Merchants.IssueApiKey.MerchantNotFound", "The merchant was not found.");
}
