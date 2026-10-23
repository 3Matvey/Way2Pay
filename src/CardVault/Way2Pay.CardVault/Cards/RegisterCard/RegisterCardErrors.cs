using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

public static class RegisterCardErrors
{
    public static Error InvalidPan() =>
        Error.BadRequest("CardVault.RegisterCard.InvalidPan", "PAN must contain 12 to 19 ASCII digits and pass the Luhn check.");

    public static Error InvalidExpiration() =>
        Error.BadRequest("CardVault.RegisterCard.InvalidExpiration", "Expiration month must be 1 to 12 and year must be 1 to 9999.");

    public static Error CardExpired() =>
        Error.BadRequest("CardVault.RegisterCard.CardExpired", "The card has expired.");
}
