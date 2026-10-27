namespace Way2Pay.Merchants.Application.ApiKeys;

/// <summary>Generates API-key secrets and verifies them without exposing cryptographic implementation details.</summary>
public interface IApiKeySecretProvider
{
    /// <summary>Generates a Base64Url secret from 32 cryptographically random bytes and its storage hash.</summary>
    ApiKeySecret Generate();

    /// <summary>Checks the exact secret against its stored hash using a timing-safe comparison.</summary>
    bool Verify(string secret, string secretHash);
}
