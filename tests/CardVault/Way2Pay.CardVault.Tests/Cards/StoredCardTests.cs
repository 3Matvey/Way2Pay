using Way2Pay.CardVault.Cards;
using static Way2Pay.CardVault.Tests.CardVaultTestData;

namespace Way2Pay.CardVault.Tests.Cards;

public sealed class StoredCardTests
{
    [Fact]
    public void BlockAndUnblock_PreserveCardTokenOwnershipAndEncryptedData()
    {
        var card = CreateCard(Guid.NewGuid(), "customer-1");
        var tokenId = card.Id;
        card.Block();
        Assert.Equal(CardStatus.Blocked, card.Status);
        card.Unblock();

        Assert.Equal(CardStatus.Active, card.Status);
        Assert.Equal(tokenId, card.Id);
        Assert.Equal(Ciphertext, card.PanCiphertext);
        Assert.Equal("customer-1", card.ExternalCustomerId);
    }

    [Fact]
    public void Unblock_DoesNotExtendCardExpiration()
    {
        var expiration = new CardExpiration(10, 2026);
        var card = new StoredCard(Guid.NewGuid(), Ciphertext, "1111", expiration);
        card.Block();
        card.Unblock();

        Assert.Equal(CardStatus.Active, card.Status);
        Assert.True(card.Expiration.IsExpiredOn(new DateOnly(2026, 11, 1)));
        Assert.Equal(expiration, card.Expiration);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12a4")]
    [InlineData("１２３４")]
    [InlineData(" 123")]
    public void Constructor_WhenLastFourAreNotFourAsciiDigits_Rejects(string lastFour)
    {
        Assert.Throws<ArgumentException>(() => new StoredCard(
            Guid.NewGuid(), Ciphertext, lastFour, new CardExpiration(12, 2027)));
    }

    [Fact]
    public void Constructor_PreservesLeadingZerosInLastFour()
    {
        var card = new StoredCard(Guid.NewGuid(), Ciphertext, "0001", new CardExpiration(12, 2027));

        Assert.Equal("0001", card.LastFour);
    }

    [Theory]
    [InlineData(450, true)]
    [InlineData(451, false)]
    public void Constructor_EnforcesExternalCustomerIdLength(int length, bool accepted)
    {
        var customerId = new string('a', length);
        if (accepted)
            Assert.Equal(customerId, CreateCard(Guid.NewGuid(), customerId).ExternalCustomerId);
        else
            Assert.Throws<ArgumentException>(() => CreateCard(Guid.NewGuid(), customerId));
    }
}
