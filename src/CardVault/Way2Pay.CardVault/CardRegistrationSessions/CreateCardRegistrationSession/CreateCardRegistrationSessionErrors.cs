using Way2Pay.Common.Results;

namespace Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

public static class CreateCardRegistrationSessionErrors
{
    public static Error InvalidMerchantId() =>
        Error.BadRequest("CardVault.CardRegistrationSessions.InvalidMerchantId", "A merchant identifier is required.");

    public static Error InvalidExternalCustomerId() =>
        Error.BadRequest("CardVault.CardRegistrationSessions.InvalidExternalCustomerId", "The external customer identifier must be non-blank and at most 450 characters when supplied.");
}
