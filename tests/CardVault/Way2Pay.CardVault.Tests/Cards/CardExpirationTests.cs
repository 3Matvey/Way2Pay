using Way2Pay.CardVault.Cards;

namespace Way2Pay.CardVault.Tests.Cards;

public sealed class CardExpirationTests
{
    [Theory]
    [InlineData(2026, 10, 31, false)]
    [InlineData(2026, 11, 1, false)]
    [InlineData(2026, 11, 30, false)]
    [InlineData(2026, 12, 1, true)]
    [InlineData(2027, 1, 1, true)]
    public void IsExpiredOn_CardRemainsValidThroughExpirationMonth(int year, int month, int day, bool expired)
    {
        var expiration = new CardExpiration(11, 2026);

        Assert.Equal(expired, expiration.IsExpiredOn(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2026, 12, 31, false)]
    [InlineData(2027, 1, 1, true)]
    public void IsExpiredOn_DecemberExpirationHandlesYearBoundary(int year, int month, int day, bool expired)
    {
        var expiration = new CardExpiration(12, 2026);

        Assert.Equal(expired, expiration.IsExpiredOn(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(2028, 2, 29, false)]
    [InlineData(2028, 3, 1, true)]
    public void IsExpiredOn_FebruaryExpirationHandlesLeapDay(int year, int month, int day, bool expired)
    {
        var expiration = new CardExpiration(2, 2028);

        Assert.Equal(expired, expiration.IsExpiredOn(new DateOnly(year, month, day)));
    }

    [Theory]
    [InlineData(0, 2027)]
    [InlineData(13, 2027)]
    [InlineData(1, 0)]
    [InlineData(1, 10000)]
    public void Constructor_WhenCalendarValuesAreInvalid_Rejects(int month, int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CardExpiration(month, year));
    }
}
