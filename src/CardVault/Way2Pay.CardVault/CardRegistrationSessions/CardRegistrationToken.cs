using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Way2Pay.CardVault.CardRegistrationSessions;

/// <summary>Creates opaque registration credentials and the verifier stored in the database.</summary>
internal static class CardRegistrationToken
{
    /// <summary>Generates a Base64Url token from 32 cryptographically random bytes.</summary>
    internal static string Generate() =>
        WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    /// <summary>Hashes the exact, case-sensitive token text as UTF-8 and returns uppercase SHA-256 hexadecimal.</summary>
    /// <remarks>Issuance and authentication must use the same representation, without trimming or decoding.</remarks>
    internal static string ComputeHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
