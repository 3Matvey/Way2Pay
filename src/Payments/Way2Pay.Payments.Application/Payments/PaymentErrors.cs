using Way2Pay.Payments.Application.Common.Results;

namespace Way2Pay.Payments.Application.Payments;

public static class PaymentErrors
{
    public static Error InvalidMerchantId() =>
        Error.BadRequest("Payments.InvalidMerchantId", "A merchant identifier is required.");

    public static Error InvalidPaymentMethod() =>
        Error.BadRequest("Payments.InvalidPaymentMethod", "A payment method is required.");

    public static Error InvalidAmount() =>
        Error.BadRequest("Payments.InvalidAmount", "The payment amount must be positive.");

    public static Error InvalidCurrency() =>
        Error.BadRequest("Payments.InvalidCurrency", "Currency must be a three-letter uppercase code.");

    public static Error InvalidIdempotencyKey() =>
        Error.BadRequest("Payments.InvalidIdempotencyKey", "An idempotency key is required.");

    public static Error IdempotencyConflict() =>
        Error.Conflict("Payments.IdempotencyConflict", "The idempotency key was already used with different payment parameters.");

    public static Error InvalidPaymentId() =>
        Error.BadRequest("Payments.InvalidPaymentId", "A payment identifier is required.");

    public static Error InvalidOperationType() =>
        Error.BadRequest("Payments.InvalidOperationType", "The payment operation type is not supported.");

    public static Error NotFound() =>
        Error.NotFound("Payments.NotFound", "The payment was not found.");

    public static Error OperationNotAllowed() =>
        Error.Conflict("Payments.OperationNotAllowed", "The operation or amount is not allowed for the current payment state.");

    public static Error ConcurrencyConflict() =>
        Error.Conflict("Payments.ConcurrencyConflict", "The payment was changed by another command. Retry the request.");

    public static Error MerchantUnavailable() =>
        Error.AccessForbidden("Payments.MerchantUnavailable", "The merchant cannot accept payments.");
}
