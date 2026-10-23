using System.Security.Cryptography;
using System.Text;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.Persistence;
using Way2Pay.CardVault.Vault;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

internal sealed class RegisterCardHandler(
    CardVaultDbContext dbContext, ICardDataProtector dataProtector, TimeProvider timeProvider)
{
    /// <summary>Validates, encrypts and stores a card within an already authorized registration scope.</summary>
    /// <remarks>
    /// Uses the current UTC date for expiration validation. Encryption and persistence failures propagate;
    /// a successful response is returned only after saving. No plaintext credentials are persisted.
    /// </remarks>
    public async Task<Result<RegisterCardResponse>> ExecuteAsync(
        RegisterCardRequest request, CardRegistrationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var validation = RegisterCardValidator.Validate(request, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        if (!validation.IsSuccess)
            return validation.Error;
        var ciphertext = await EncryptPanAsync(request.Pan, cancellationToken);
        return await SaveCardAsync(request, context, ciphertext, cancellationToken);
    }

    private async Task<string> EncryptPanAsync(string pan, CancellationToken cancellationToken)
    {
        var buffer = Encoding.ASCII.GetBytes(pan);
        try
        {
            return await dataProtector.EncryptPanAsync(buffer, cancellationToken);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(buffer);
        }
    }

    private async Task<Result<RegisterCardResponse>> SaveCardAsync(
        RegisterCardRequest request, CardRegistrationContext context, string ciphertext, CancellationToken cancellationToken)
    {
        var expiration = new CardExpiration(request.ExpirationMonth, request.ExpirationYear);
        var card = new StoredCard(context.MerchantId, ciphertext, request.Pan[^4..], expiration, context.ExternalCustomerId);
        dbContext.StoredCards.Add(card);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new RegisterCardResponse(card.Id, card.LastFour, expiration.Month, expiration.Year);
    }
}
