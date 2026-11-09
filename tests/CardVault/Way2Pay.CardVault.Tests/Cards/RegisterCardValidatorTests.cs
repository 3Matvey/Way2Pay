using Way2Pay.CardVault.Cards.RegisterCard;

namespace Way2Pay.CardVault.Tests.Cards;

public sealed class RegisterCardValidatorTests
{
    private static readonly DateOnly CurrentDate = new(2026, 11, 30);

    private static RegisterCardRequest CreateRequest(string? pan = "4111111111111111", int month = 11, int year = 2026) =>
        new() { Pan = pan!, ExpirationMonth = month, ExpirationYear = year };

    // Synthetic/test PANs only. Both odd and even lengths exercise the Luhn doubling parity.
    [Theory]
    [InlineData("424242424242")]
    [InlineData("4111111111111111")]
    [InlineData("5555555555554444")]
    [InlineData("378282246310005")]
    [InlineData("4000000000000000006")]
    public void Validate_WhenPanHasValidFormatAndChecksum_Accepts(string pan)
    {
        var result = RegisterCardValidator.Validate(CreateRequest(pan), CurrentDate);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("                ")]
    [InlineData("42424242424")]
    [InlineData("42424242424242424242")]
    [InlineData("4111111111111112")]
    [InlineData("4111 1111 1111 1111")]
    [InlineData("4111-1111-1111-1111")]
    [InlineData(" 4111111111111111")]
    [InlineData("4111111111111111 ")]
    [InlineData("４１１１１１１１１１１１１１１１")]
    public void Validate_WhenPanIsInvalid_ReturnsPanError(string? pan)
    {
        var result = RegisterCardValidator.Validate(CreateRequest(pan), CurrentDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidPan", result.Error!.Code);
    }

    [Theory]
    [InlineData(0, 2027)]
    [InlineData(13, 2027)]
    [InlineData(11, 0)]
    [InlineData(11, 10000)]
    public void Validate_WhenExpirationIsMalformed_ReturnsExpirationError(int month, int year)
    {
        var result = RegisterCardValidator.Validate(CreateRequest(month: month, year: year), CurrentDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.InvalidExpiration", result.Error!.Code);
    }

    [Fact]
    public void Validate_WhenCardExpired_ReturnsExpiredError()
    {
        var result = RegisterCardValidator.Validate(CreateRequest(month: 10), CurrentDate);

        Assert.False(result.IsSuccess);
        Assert.Equal("CardVault.RegisterCard.CardExpired", result.Error!.Code);
    }

    [Fact]
    public void Validate_WhenExpirationMonthEnds_RemainsValidUntilNextMonth()
    {
        var request = CreateRequest();

        Assert.True(RegisterCardValidator.Validate(request, CurrentDate).IsSuccess);
        Assert.Equal("CardVault.RegisterCard.CardExpired",
            RegisterCardValidator.Validate(request, CurrentDate.AddDays(1)).Error!.Code);
    }

    [Fact]
    public void Validate_WhenPanIsInvalid_DoesNotIncludeSubmittedDataInError()
    {
        const string invalidPan = "4111111111111112";
        var result = RegisterCardValidator.Validate(CreateRequest(invalidPan), CurrentDate);

        Assert.False(result.IsSuccess);
        Assert.DoesNotContain(invalidPan, result.Error!.Description);
        Assert.DoesNotContain(invalidPan, result.Error.Code);
    }
}
