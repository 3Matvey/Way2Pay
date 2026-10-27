namespace Way2Pay.Merchants.Application.ApiKeys.ValidateApiKey;

/// <summary>Contains an external credential; do not log or trace this request.</summary>
public sealed class ValidateApiKeyRequest(string apiKey)
{
    public string ApiKey { get; } = apiKey;
}
