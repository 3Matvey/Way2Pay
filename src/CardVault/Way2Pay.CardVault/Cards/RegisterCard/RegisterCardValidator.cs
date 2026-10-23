using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.Cards.RegisterCard;

public static class RegisterCardValidator
{
    /// <summary>Returns the first validation error without including submitted card data.</summary>
    /// <remarks>
    /// Expects an unformatted PAN. A valid format and checksum do not establish that a card exists or can be charged.
    /// The caller supplies the business date; the expiration month remains valid through its last day.
    /// </remarks>
    public static Result Validate(RegisterCardRequest request, DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!HasValidPan(request.Pan.AsSpan()))
            return RegisterCardErrors.InvalidPan();
        var expiration = ValidateExpiration(request.ExpirationMonth, request.ExpirationYear, currentDate);
        if (!expiration.IsSuccess)
            return expiration;
        return Result.Success();
    }

    private static bool HasValidPan(ReadOnlySpan<char> pan)
    {
        if (pan.Length is < 12 or > 19)
            return false;
        foreach (char character in pan)
            if (character is < '0' or > '9')
                return false;
        return HasValidChecksum(pan);
    }

    /// <summary>
    /// Validates a PAN using the Luhn checksum algorithm.
    /// </summary>
    private static bool HasValidChecksum(ReadOnlySpan<char> pan)
    {
        int sum = 0;
        for (int index = 0; index < pan.Length; index++)
        {
            int digit = pan[index] - '0';
            sum += (pan.Length - index) % 2 == 0 ? SumDoubledDigit(digit) : digit;
        }
        return sum % 10 == 0;

        static int SumDoubledDigit(int digit)
        {
            int doubled = digit * 2;
            return doubled > 9 ? doubled - 9 : doubled;
        }
    }

    private static Result ValidateExpiration(int month, int year, DateOnly currentDate)
    {
        if (month is < 1 or > 12 || year is < 1 or > 9999)
            return RegisterCardErrors.InvalidExpiration();
        var expiration = new CardExpiration(month, year);
        if (expiration.IsExpiredOn(currentDate))
            return RegisterCardErrors.CardExpired();
        return Result.Success();
    }
}
