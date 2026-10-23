namespace Way2Pay.CardVault.Vault;

/// <summary>Encrypts card data without exposing encryption keys to the registration scenario.</summary>
public interface ICardDataProtector
{
    /// <summary>Encrypts an ASCII-encoded PAN and returns the ciphertext supplied by Vault Transit.</summary>
    /// <param name="pan">The plaintext PAN buffer, owned by the caller.</param>
    /// <param name="cancellationToken">Cancels the encryption operation.</param>
    /// <remarks>
    /// The caller keeps the buffer valid and unchanged until the returned task completes, then clears
    /// its owned buffer in a finally block, including on failure or cancellation.
    /// The implementation must not retain the buffer after task completion or log its contents.
    /// Temporary plaintext copies created by the implementation require separate lifetime management.
    /// The returned ciphertext is intended for storage; it is not a card token.
    /// </remarks>
    Task<string> EncryptPanAsync(ReadOnlyMemory<byte> pan, CancellationToken cancellationToken = default);
}
