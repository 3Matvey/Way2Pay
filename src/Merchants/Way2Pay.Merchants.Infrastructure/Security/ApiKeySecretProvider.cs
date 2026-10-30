using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Way2Pay.Merchants.Application.ApiKeys;

namespace Way2Pay.Merchants.Infrastructure.Security;

/// <summary>Generates 256-bit random secrets and stores SHA-256 verifiers of their exact Base64Url text.</summary>
internal sealed class ApiKeySecretProvider : IApiKeySecretProvider
{
    public ApiKeySecret Generate()
    {
        var secret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        return new ApiKeySecret(secret, Convert.ToHexString(HashSecret(secret)));
    }

    /// <remarks>Malformed hexadecimal storage data throws; it is a technical failure, not a credential mismatch.</remarks>
    public bool Verify(string secret, string secretHash)
    {
        var expectedHash = Convert.FromHexString(secretHash);
        var actualHash = HashSecret(secret);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static byte[] HashSecret(string secret) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(secret));
}
