namespace Way2Pay.CardVault.Cards.RegisterCard;

/// <summary>Returns the registered card's internal reference and display metadata, without credentials.</summary>
/// <param name="CardTokenId">The stored card identifier; knowing it does not authorize use of the card.</param>
/// <param name="LastFour">The last four PAN digits, including any leading zeros.</param>
/// <param name="ExpirationMonth">The expiration month, from 1 through 12.</param>
/// <param name="ExpirationYear">The full calendar year, rather than a two-digit year.</param>
public sealed record RegisterCardResponse(
    Guid CardTokenId,
    string LastFour,
    int ExpirationMonth,
    int ExpirationYear);
