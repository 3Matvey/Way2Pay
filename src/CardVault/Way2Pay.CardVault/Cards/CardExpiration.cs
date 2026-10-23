namespace Way2Pay.CardVault.Cards;

/// <summary>A card's expiration month and four-digit calendar year.</summary>
public sealed record CardExpiration
{
    public int Month { get; }
    public int Year { get; }

    public CardExpiration(int month, int year)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        if (year is < 1 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        Month = month;
        Year = year;
    }

    /// <summary>Returns true after the expiration month; the card is valid through that entire month.</summary>
    /// <remarks>The caller supplies the business date; this value object does not read the system clock.</remarks>
    public bool IsExpiredOn(DateOnly date) =>
        date.Year > Year || (date.Year == Year && date.Month > Month);
}
