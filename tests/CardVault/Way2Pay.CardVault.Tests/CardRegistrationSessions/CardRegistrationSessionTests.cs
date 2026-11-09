using Way2Pay.CardVault.CardRegistrationSessions;
using static Way2Pay.CardVault.Tests.CardVaultTestData;

namespace Way2Pay.CardVault.Tests.CardRegistrationSessions;

public sealed class CardRegistrationSessionTests
{
    [Fact]
    public void Complete_WhenPending_BindsCardAndAdvancesVersion()
    {
        var session = CreateSession("customer-1");
        var card = CreateCard(session.MerchantId, session.ExternalCustomerId);
        session.Complete(card, Now);

        Assert.Equal(CardRegistrationSessionStatus.Completed, session.Status);
        Assert.Equal(card.Id, session.StoredCardId);
        Assert.Equal(1L, session.Version);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void IsExpiredAt_UsesExactExpirationBoundary(int offsetTicks, bool expired)
    {
        var session = CreateSession();

        Assert.Equal(expired, session.IsExpiredAt(session.ExpiresAt.AddTicks(offsetTicks)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Complete_WhenExpired_RejectsWithoutChangingSession(int offsetTicks)
    {
        var session = CreateSession();
        var card = CreateCard(session.MerchantId);

        Assert.Throws<InvalidOperationException>(() => session.Complete(card, session.ExpiresAt.AddTicks(offsetTicks)));
        Assert.Equal(CardRegistrationSessionStatus.Pending, session.Status);
        Assert.Null(session.StoredCardId);
        Assert.Equal(0L, session.Version);
    }

    [Fact]
    public void Complete_WhenCardBelongsToAnotherMerchant_Rejects()
    {
        var session = CreateSession("customer-1");
        var card = CreateCard(Guid.NewGuid(), session.ExternalCustomerId);

        Assert.Throws<InvalidOperationException>(() => session.Complete(card, Now));
        Assert.Equal(CardRegistrationSessionStatus.Pending, session.Status);
        Assert.Null(session.StoredCardId);
        Assert.Equal(0L, session.Version);
    }

    [Theory]
    [InlineData(null, "customer-1")]
    [InlineData("customer-1", null)]
    [InlineData("customer-1", "customer-2")]
    [InlineData("Customer", "customer")]
    public void Complete_WhenCustomerDoesNotMatchExactly_Rejects(string? sessionCustomer, string? cardCustomer)
    {
        var session = CreateSession(sessionCustomer);
        var card = CreateCard(session.MerchantId, cardCustomer);

        Assert.Throws<InvalidOperationException>(() => session.Complete(card, Now));
        Assert.Null(session.StoredCardId);
        Assert.Equal(CardRegistrationSessionStatus.Pending, session.Status);
        Assert.Equal(0L, session.Version);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Complete_WhenAlreadyCompleted_RejectsAndPreservesFirstCard(bool useAnotherCard)
    {
        var session = CreateSession();
        var firstCard = CreateCard(session.MerchantId);
        session.Complete(firstCard, Now);
        var repeatedCard = useAnotherCard ? CreateCard(session.MerchantId) : firstCard;

        Assert.Throws<InvalidOperationException>(() => session.Complete(repeatedCard, Now));
        Assert.Equal(firstCard.Id, session.StoredCardId);
        Assert.Equal(CardRegistrationSessionStatus.Completed, session.Status);
        Assert.Equal(1L, session.Version);
    }

    [Fact]
    public void Revoke_WhenRepeated_DoesNotAdvanceVersionAgain()
    {
        var session = CreateSession();
        session.Revoke();
        session.Revoke();

        Assert.Equal(CardRegistrationSessionStatus.Revoked, session.Status);
        Assert.Null(session.StoredCardId);
        Assert.Equal(1L, session.Version);
    }

    [Fact]
    public void Complete_WhenRevoked_RejectsWithoutRestoringSession()
    {
        var session = CreateSession();
        session.Revoke();

        Assert.Throws<InvalidOperationException>(() => session.Complete(CreateCard(session.MerchantId), Now));
        Assert.Equal(CardRegistrationSessionStatus.Revoked, session.Status);
        Assert.Null(session.StoredCardId);
        Assert.Equal(1L, session.Version);
    }

    [Fact]
    public void Revoke_WhenCompleted_RejectsAndPreservesResult()
    {
        var session = CreateSession();
        var card = CreateCard(session.MerchantId);
        session.Complete(card, Now);

        Assert.Throws<InvalidOperationException>(() => session.Revoke());
        Assert.Equal(CardRegistrationSessionStatus.Completed, session.Status);
        Assert.Equal(card.Id, session.StoredCardId);
        Assert.Equal(1L, session.Version);
    }

    [Fact]
    public void Constructor_NormalizesLowercaseHashWithoutChangingItsValue()
    {
        var session = new CardRegistrationSession(Guid.NewGuid(), TokenHash.ToLowerInvariant(), Now, Now.AddMinutes(15));

        Assert.Equal(TokenHash, session.TokenHash);
        Assert.Equal(CardRegistrationSessionStatus.Pending, session.Status);
        Assert.Equal(0L, session.Version);
    }

    [Theory]
    [InlineData(63, 'A')]
    [InlineData(65, 'A')]
    [InlineData(64, 'G')]
    public void Constructor_WhenTokenHashIsNotSha256Hex_Rejects(int length, char character)
    {
        Assert.Throws<ArgumentException>(() => new CardRegistrationSession(
            Guid.NewGuid(), new string(character, length), Now, Now.AddMinutes(15)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WhenLifetimeIsNotPositive_Rejects(int lifetimeTicks)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CardRegistrationSession(
            Guid.NewGuid(), TokenHash, Now, Now.AddTicks(lifetimeTicks)));
    }

    [Theory]
    [InlineData(450, true)]
    [InlineData(451, false)]
    public void Constructor_EnforcesExternalCustomerIdLength(int length, bool accepted)
    {
        var customerId = new string('a', length);
        if (accepted)
            Assert.Equal(customerId, CreateSession(customerId).ExternalCustomerId);
        else
            Assert.Throws<ArgumentException>(() => CreateSession(customerId));
    }
}
