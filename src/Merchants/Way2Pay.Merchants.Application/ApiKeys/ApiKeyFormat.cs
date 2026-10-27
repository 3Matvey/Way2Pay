namespace Way2Pay.Merchants.Application.ApiKeys;

/// <summary>Defines the shared wire format used to issue and parse merchant API keys.</summary>
internal static class ApiKeyFormat
{
    private const string Prefix = "w2p_";
    private const int KeyIdLength = 32;
    private const int SecretLength = 43;

    internal static string Create(Guid apiKeyId, string secret) => $"{Prefix}{apiKeyId:N}.{secret}";

    /// <summary>Parses the exact key text without trimming, decoding or changing the secret's case.</summary>
    internal static bool TryParse(string? value, out Guid apiKeyId, out string secret)
    {
        apiKeyId = Guid.Empty;
        secret = string.Empty;
        var separatorIndex = Prefix.Length + KeyIdLength;
        if (value is null || value.Length != separatorIndex + 1 + SecretLength
            || !value.StartsWith(Prefix, StringComparison.Ordinal) || value[separatorIndex] != '.')
            return false;

        if (!Guid.TryParseExact(value.AsSpan(Prefix.Length, KeyIdLength), "N", out apiKeyId) || apiKeyId == Guid.Empty)
            return false;

        secret = value[(separatorIndex + 1)..];
        return secret.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_');
    }
}
