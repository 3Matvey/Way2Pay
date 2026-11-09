using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Cards.RegisterCard;
using Way2Pay.CardVault.Vault;

namespace Way2Pay.CardVault.IntegrationTests;

internal static class RegistrationTestData
{
    public const string Ciphertext = "vault:v1:test-ciphertext";

    public static RegisterCardRequest CreateRequest(string pan = "4111111111111111") =>
        new() { Pan = pan, ExpirationMonth = 12, ExpirationYear = 2027 };

    public static CardRegistrationContext CreateContext(CardRegistrationSession session) =>
        new(session.Id, session.MerchantId, session.ExternalCustomerId);

    public static async Task<CardRegistrationSession> SeedSessionAsync(SqlServerFixture database, bool revoked = false)
    {
        var now = new TestClock().GetUtcNow();
        var session = new CardRegistrationSession(Guid.NewGuid(), Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            now, now.AddMinutes(15), "customer-1");
        if (revoked)
            session.Revoke();
        await using var context = database.CreateContext();
        context.CardRegistrationSessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public static async Task AssertPendingWithoutCardsAsync(SqlServerFixture database, CardRegistrationSession session)
    {
        await using var context = database.CreateContext();
        var saved = await context.CardRegistrationSessions.SingleAsync(candidate => candidate.Id == session.Id);
        Assert.Equal(CardRegistrationSessionStatus.Pending, saved.Status);
        Assert.Null(saved.StoredCardId);
        Assert.Equal(0L, saved.Version);
        Assert.False(await context.StoredCards.AnyAsync(card => card.MerchantId == session.MerchantId));
    }
}

internal sealed class TestClock : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => UtcNow;
}

internal sealed class TestCardDataProtector : ICardDataProtector
{
    private int _calls;
    public int Calls => Volatile.Read(ref _calls);
    public Func<CancellationToken, Task<string>>? OnEncrypt { get; init; }

    public Task<string> EncryptPanAsync(ReadOnlyMemory<byte> pan, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _calls);
        return OnEncrypt is null ? Task.FromResult(RegistrationTestData.Ciphertext) : OnEncrypt(cancellationToken);
    }
}
