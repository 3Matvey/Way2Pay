using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Persistence;
using Way2Pay.CardVault.Vault;
using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

internal sealed class RegisterCardHandler(
    CardVaultDbContext dbContext, ICardDataProtector dataProtector, TimeProvider timeProvider)
{
    /// <summary>Registers one card per authorized session or returns the previously saved result.</summary>
    /// <remarks>
    /// Saves the card and session completion atomically. A concurrent completion is resolved by reading
    /// its saved result, never by retrying registration. Expired or revoked sessions cannot replay a result.
    /// Encryption and unexpected persistence failures propagate without completing the session.
    /// </remarks>
    public async Task<Result<RegisterCardResponse>> ExecuteAsync(
        RegisterCardRequest request, CardRegistrationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var session = await LoadSessionAsync(context, cancellationToken);
        if (session is null || !IsSessionUsable(session))
            return RegisterCardErrors.InvalidSession();
        if (session.Status == CardRegistrationSessionStatus.Completed)
            return await ReadCompletedResultAsync(session, cancellationToken);
        return await RegisterAsync(request, session, context, cancellationToken);
    }

    private Task<CardRegistrationSession?> LoadSessionAsync(
        CardRegistrationContext context, CancellationToken cancellationToken) =>
        dbContext.CardRegistrationSessions.SingleOrDefaultAsync(session =>
            session.Id == context.SessionId && session.MerchantId == context.MerchantId
            && session.ExternalCustomerId == context.ExternalCustomerId, cancellationToken);

    private bool IsSessionUsable(CardRegistrationSession session) =>
        !session.IsExpiredAt(timeProvider.GetUtcNow())
        && session.Status is CardRegistrationSessionStatus.Pending or CardRegistrationSessionStatus.Completed;

    private async Task<Result<RegisterCardResponse>> RegisterAsync(
        RegisterCardRequest request, CardRegistrationSession session,
        CardRegistrationContext context, CancellationToken cancellationToken)
    {
        var validation = RegisterCardValidator.Validate(request, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime));
        if (!validation.IsSuccess)
            return validation.Error;
        var ciphertext = await EncryptPanAsync(request.Pan, cancellationToken);
        var card = new StoredCard(session.MerchantId, ciphertext, request.Pan[^4..],
            new CardExpiration(request.ExpirationMonth, request.ExpirationYear), session.ExternalCustomerId);
        return await SaveCardAsync(card, session, context, cancellationToken);
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
        StoredCard card, CardRegistrationSession session,
        CardRegistrationContext context, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        if (session.IsExpiredAt(now))
            return RegisterCardErrors.InvalidSession();
        session.Complete(card, now);
        dbContext.StoredCards.Add(card);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new RegisterCardResponse(card.Id, card.LastFour, card.Expiration.Month, card.Expiration.Year);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await ResolveConcurrentRegistrationAsync(card, session, context, cancellationToken);
        }
    }

    /// <summary>Discards the rolled-back registration and reads the winner without repeating encryption or saving.</summary>
    private async Task<Result<RegisterCardResponse>> ResolveConcurrentRegistrationAsync(
        StoredCard card, CardRegistrationSession session,
        CardRegistrationContext context, CancellationToken cancellationToken)
    {
        dbContext.Entry(card).State = EntityState.Detached;
        dbContext.Entry(session).State = EntityState.Detached;
        var current = await LoadSessionAsync(context, cancellationToken);
        if (current is null || !IsSessionUsable(current))
            return RegisterCardErrors.InvalidSession();
        if (current.Status != CardRegistrationSessionStatus.Completed)
            return RegisterCardErrors.RegistrationConflict();
        return await ReadCompletedResultAsync(current, cancellationToken);
    }

    private async Task<Result<RegisterCardResponse>> ReadCompletedResultAsync(
        CardRegistrationSession session, CancellationToken cancellationToken)
    {
        var result = await dbContext.StoredCards.AsNoTracking()
            .Where(card => card.Id == session.StoredCardId && card.MerchantId == session.MerchantId
                && card.ExternalCustomerId == session.ExternalCustomerId)
            .Select(card => new RegisterCardResponse(
                card.Id, card.LastFour, card.Expiration.Month, card.Expiration.Year))
            .SingleOrDefaultAsync(cancellationToken);
        if (session.IsExpiredAt(timeProvider.GetUtcNow()))
            return RegisterCardErrors.InvalidSession();
        return result is null ? RegisterCardErrors.RegistrationResultUnavailable() : result;
    }
}
