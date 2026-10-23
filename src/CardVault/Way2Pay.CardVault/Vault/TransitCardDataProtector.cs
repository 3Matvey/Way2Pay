using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Way2Pay.CardVault.Vault;

/// <summary>Encrypts PAN through the Vault Transit HTTP API.</summary>
/// <remarks>
/// Base64 remains plaintext data. Neither the request body nor the token may be logged.
/// The Base64 string and serializer copies cannot be reliably cleared; clearing the caller's
/// byte buffer does not erase these copies. The caller retains ownership of that buffer.
/// </remarks>
internal sealed class TransitCardDataProtector(
    HttpClient httpClient, IOptions<VaultOptions> options) : ICardDataProtector
{
    public async Task<string> EncryptPanAsync(
        ReadOnlyMemory<byte> pan, CancellationToken cancellationToken = default)
    {
        var request = new EncryptRequest { Plaintext = Convert.ToBase64String(pan.Span) };
        using var response = await httpClient.PostAsJsonAsync(GetEncryptionPath(), request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<EncryptResponse>(cancellationToken: cancellationToken);
        return GetCiphertext(body);
    }

    private string GetEncryptionPath()
    {
        var mount = options.Value.TransitMountPoint.Trim('/');
        var key = Uri.EscapeDataString(options.Value.PanKeyName);
        return $"v1/{mount}/encrypt/{key}";
    }

    private static string GetCiphertext(EncryptResponse? response)
    {
        var ciphertext = response?.Data?.Ciphertext;
        if (string.IsNullOrWhiteSpace(ciphertext))
            throw new InvalidOperationException("Vault returned an empty encryption result.");
        return ciphertext;
    }

    private sealed class EncryptRequest
    {
        [JsonPropertyName("plaintext")]
        public string Plaintext { get; init; } = string.Empty;
    }

    private sealed class EncryptResponse
    {
        public EncryptionData? Data { get; init; }
    }

    private sealed class EncryptionData
    {
        public string Ciphertext { get; init; } = string.Empty;
    }
}
