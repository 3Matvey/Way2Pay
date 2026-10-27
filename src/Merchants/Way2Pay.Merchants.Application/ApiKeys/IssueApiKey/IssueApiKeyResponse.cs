namespace Way2Pay.Merchants.Application.ApiKeys.IssueApiKey;

/// <summary>Returns the complete API key only on issuance, after its verifier has been saved.</summary>
/// <remarks>Contains a credential: do not log, trace or cache this response. The key cannot be retrieved later.</remarks>
public sealed class IssueApiKeyResponse(Guid apiKeyId, string apiKey, DateTimeOffset? expiresAt)
{
    public Guid ApiKeyId { get; } = apiKeyId;
    public string ApiKey { get; } = apiKey;
    public DateTimeOffset? ExpiresAt { get; } = expiresAt;
}
