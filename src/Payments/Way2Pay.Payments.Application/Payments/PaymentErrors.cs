using Way2Pay.Payments.Application.Common.Results;

namespace Way2Pay.Payments.Application.Payments;

public static class PaymentErrors
{
    public static Error InvalidMerchantId() =>
        Error.BadRequest("Payments.InvalidMerchantId", "A merchant identifier is required.");

    public static Error InvalidAmount() =>
        Error.BadRequest("Payments.InvalidAmount", "The payment amount must be positive.");

    public static Error InvalidCurrency() =>
        Error.BadRequest("Payments.InvalidCurrency", "Currency must be a three-letter uppercase code.");

    public static Error InvalidIdempotencyKey() =>
        Error.BadRequest("Payments.InvalidIdempotencyKey", "An idempotency key is required.");

    public static Error IdempotencyConflict() =>
        Error.Conflict("Payments.IdempotencyConflict", "The idempotency key was already used with different payment parameters.");

    public static Error MerchantUnavailable() =>
        Error.AccessForbidden("Payments.MerchantUnavailable", "The merchant cannot accept payments.");
}
