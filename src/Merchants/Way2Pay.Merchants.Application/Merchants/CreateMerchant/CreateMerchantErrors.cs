namespace Way2Pay.Merchants.Application.Merchants.CreateMerchant;

public static class CreateMerchantErrors
{
    public static Error InvalidName() => Error.BadRequest(
        "Merchants.CreateMerchant.InvalidName", "Merchant name must be non-blank and at most 200 characters after trimming.");
}
