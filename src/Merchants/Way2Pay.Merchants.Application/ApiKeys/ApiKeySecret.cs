namespace Way2Pay.Merchants.Application.ApiKeys;

/// <summary>A generated plaintext secret and its verifier; this object must not be logged or persisted.</summary>
public sealed class ApiKeySecret(string secret, string hash)
{
    public string Secret { get; } = secret;
    public string Hash { get; } = hash;
}
