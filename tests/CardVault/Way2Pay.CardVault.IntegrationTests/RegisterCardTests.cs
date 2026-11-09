using Microsoft.EntityFrameworkCore;
using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Cards.RegisterCard;
using static Way2Pay.CardVault.IntegrationTests.RegistrationTestData;

namespace Way2Pay.CardVault.IntegrationTests;

public sealed class RegisterCardTests(SqlServerFixture database) : IClassFixture<SqlServerFixture>
{
    [Fact]
    public async Task Register_PersistsCardAndSessionCompletion()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector();
        await using var context = database.CreateContext();
        var result = await new RegisterCardHandler(context, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));

        Assert.True(result.IsSuccess);
        await using var verification = database.CreateContext();
        var card = await verification.StoredCards.SingleAsync(card => card.MerchantId == session.MerchantId);
        var savedSession = await verification.CardRegistrationSessions.SingleAsync(candidate => candidate.Id == session.Id);
        Assert.Equal(result.Value.CardTokenId, card.Id);
        Assert.Equal(Ciphertext, card.PanCiphertext);
        Assert.Equal("1111", card.LastFour);
        Assert.Equal(session.ExternalCustomerId, card.ExternalCustomerId);
        Assert.Equal(12, card.Expiration.Month);
        Assert.Equal(2027, card.Expiration.Year);
        Assert.NotEqual(default, card.CreatedAt);
        Assert.Equal(CardRegistrationSessionStatus.Completed, savedSession.Status);
        Assert.Equal(card.Id, savedSession.StoredCardId);
        Assert.Equal(1L, savedSession.Version);
        Assert.Equal(1, protector.Calls);
    }

    [Fact]
    public async Task Register_WhenRepeated_ReturnsSavedResultWithoutEncryptingAgain()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector();
        await using var firstContext = database.CreateContext();
        var first = await new RegisterCardHandler(firstContext, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));
        await using var replayContext = database.CreateContext();
        var replay = await new RegisterCardHandler(replayContext, protector, new TestClock())
            .ExecuteAsync(CreateRequest("invalid-replayed-pan"), CreateContext(session));

        Assert.True(first.IsSuccess);
        Assert.True(replay.IsSuccess);
        Assert.Equal(first.Value, replay.Value);
        Assert.Equal(1, protector.Calls);
        Assert.Equal(1, await replayContext.StoredCards.CountAsync(card => card.MerchantId == session.MerchantId));
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("revoked")]
    [InlineData("merchant")]
    [InlineData("customer")]
    public async Task Register_WhenSessionIsNotAuthorized_DoesNotEncryptOrCreateCard(string reason)
    {
        var session = await SeedSessionAsync(database, revoked: reason == "revoked");
        var clock = new TestClock();
        if (reason == "expired")
            clock.UtcNow = session.ExpiresAt;
        var scope = reason switch
        {
            "merchant" => new CardRegistrationContext(session.Id, Guid.NewGuid(), session.ExternalCustomerId),
            "customer" => new CardRegistrationContext(session.Id, session.MerchantId, "another-customer"),
            _ => CreateContext(session)
        };
        var protector = new TestCardDataProtector();
        await using var context = database.CreateContext();
        var result = await new RegisterCardHandler(context, protector, clock).ExecuteAsync(CreateRequest(), scope);

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidSession", result.Error!.Code);
        Assert.Equal(0, protector.Calls);
        Assert.False(await context.StoredCards.AnyAsync(card => card.MerchantId == session.MerchantId));
    }

    [Fact]
    public async Task Register_WhenPanIsInvalid_LeavesSessionPendingWithoutCallingVault()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector();
        await using var context = database.CreateContext();
        var result = await new RegisterCardHandler(context, protector, new TestClock())
            .ExecuteAsync(CreateRequest("4111111111111112"), CreateContext(session));

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidPan", result.Error!.Code);
        Assert.Equal(0, protector.Calls);
        await AssertPendingWithoutCardsAsync(database, session);
    }

    [Fact]
    public async Task Register_WhenVaultFails_LeavesSessionPendingWithoutCard()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector { OnEncrypt = _ => Task.FromException<string>(new HttpRequestException()) };
        await using var context = database.CreateContext();
        var handler = new RegisterCardHandler(context, protector, new TestClock());

        await Assert.ThrowsAsync<HttpRequestException>(() => handler.ExecuteAsync(CreateRequest(), CreateContext(session)));
        Assert.Equal(1, protector.Calls);
        await AssertPendingWithoutCardsAsync(database, session);
    }

    [Fact]
    public async Task Register_WhenSessionExpiresDuringEncryption_DoesNotSaveCardOrCompleteSession()
    {
        var session = await SeedSessionAsync(database);
        var clock = new TestClock();
        var protector = new TestCardDataProtector { OnEncrypt = _ =>
        {
            clock.UtcNow = session.ExpiresAt;
            return Task.FromResult(Ciphertext);
        } };
        await using var context = database.CreateContext();
        var result = await new RegisterCardHandler(context, protector, clock)
            .ExecuteAsync(CreateRequest(), CreateContext(session));

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidSession", result.Error!.Code);
        Assert.Equal(1, protector.Calls);
        await AssertPendingWithoutCardsAsync(database, session);
    }

    [Fact]
    public async Task Register_WhenCompletedSessionExpires_RejectsReplay()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector();
        await using var firstContext = database.CreateContext();
        var first = await new RegisterCardHandler(firstContext, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));
        Assert.True(first.IsSuccess);
        await using var replayContext = database.CreateContext();
        var clock = new TestClock { UtcNow = session.ExpiresAt };
        var replay = await new RegisterCardHandler(replayContext, protector, clock)
            .ExecuteAsync(CreateRequest(), CreateContext(session));

        Assert.False(replay.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidSession", replay.Error!.Code);
        Assert.Equal(1, protector.Calls);
        Assert.Equal(1, await replayContext.StoredCards.CountAsync(card => card.MerchantId == session.MerchantId));
    }

    [Fact]
    public async Task Register_WhenRevokedDuringEncryption_RollsBackCardAndPreservesRevocation()
    {
        var session = await SeedSessionAsync(database);
        var protector = new TestCardDataProtector { OnEncrypt = async _ =>
        {
            await using var concurrentContext = database.CreateContext();
            var current = await concurrentContext.CardRegistrationSessions.SingleAsync(candidate => candidate.Id == session.Id);
            current.Revoke();
            await concurrentContext.SaveChangesAsync();
            return Ciphertext;
        } };
        await using var context = database.CreateContext();
        var result = await new RegisterCardHandler(context, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidSession", result.Error!.Code);
        await using var verification = database.CreateContext();
        var saved = await verification.CardRegistrationSessions.SingleAsync(candidate => candidate.Id == session.Id);
        Assert.Equal(CardRegistrationSessionStatus.Revoked, saved.Status);
        Assert.Null(saved.StoredCardId);
        Assert.False(await verification.StoredCards.AnyAsync(card => card.MerchantId == session.MerchantId));
    }

    [Fact]
    public async Task Register_WhenDatabaseRejectsCompletion_RollsBackInsertedCard()
    {
        var session = await SeedSessionAsync(database);
        await using var context = database.CreateContext();
        // A test-only constraint rejects completion after the card insert required by the foreign key.
        var sql = $"ALTER TABLE [CardRegistrationSessions] ADD CONSTRAINT [CK_Test_{session.Id:N}] "
            + $"CHECK ([Id] <> '{session.Id:D}' OR [Version] = 0)";
        await context.Database.ExecuteSqlRawAsync(sql);
        var protector = new TestCardDataProtector();
        var handler = new RegisterCardHandler(context, protector, new TestClock());

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.ExecuteAsync(CreateRequest(), CreateContext(session)));
        Assert.Equal(1, protector.Calls);
        await AssertPendingWithoutCardsAsync(database, session);
    }

    [Fact]
    public async Task Register_WhenRequestsCompete_BothReturnSingleCommittedCard()
    {
        var session = await SeedSessionAsync(database);
        var bothEncrypting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var arrivals = 0;
        var protector = new TestCardDataProtector { OnEncrypt = async cancellationToken =>
        {
            if (Interlocked.Increment(ref arrivals) == 2)
                bothEncrypting.TrySetResult();
            await bothEncrypting.Task.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);
            return Ciphertext;
        } };
        await using var firstContext = database.CreateContext();
        await using var secondContext = database.CreateContext();
        var first = new RegisterCardHandler(firstContext, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));
        var second = new RegisterCardHandler(secondContext, protector, new TestClock())
            .ExecuteAsync(CreateRequest(), CreateContext(session));
        var results = await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(30));

        Assert.All(results, result => Assert.True(result.IsSuccess));
        Assert.Equal(results[0].Value, results[1].Value);
        Assert.Equal(2, protector.Calls);
        await using var verification = database.CreateContext();
        var card = await verification.StoredCards.SingleAsync(card => card.MerchantId == session.MerchantId);
        var savedSession = await verification.CardRegistrationSessions.SingleAsync(candidate => candidate.Id == session.Id);
        Assert.Equal(results[0].Value.CardTokenId, card.Id);
        Assert.Equal(card.Id, savedSession.StoredCardId);
        Assert.Equal(CardRegistrationSessionStatus.Completed, savedSession.Status);
        Assert.Equal(1L, savedSession.Version);
    }
}
