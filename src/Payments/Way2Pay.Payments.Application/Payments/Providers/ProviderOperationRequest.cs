using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.Providers;

/// <summary>Describes one provider call for an already persisted attempt.</summary>
/// <param name="MerchantId">The merchant owning the payment and provider account.</param>
/// <param name="PaymentId">The payment being processed.</param>
/// <param name="OperationId">The monetary operation being executed.</param>
/// <param name="AttemptId">The stable correlation reference and basis for provider idempotency, when supported.</param>
/// <param name="ProviderAccountId">The selected provider connection; credentials are resolved separately.</param>
/// <param name="Type">The monetary action to execute.</param>
/// <param name="Amount">The positive operation amount in the payment currency.</param>
/// <param name="PaymentMethodType">The payment method kind, supplied explicitly for the provider adapter.</param>
/// <param name="OriginalProviderTransactionId">Required for Capture, Void and Refund; absent for Authorize and Charge.</param>
/// <param name="PaymentMethodReference">Required for Authorize and Charge and bound to ProviderAccountId; absent for follow-up operations.</param>
/// <remarks>
/// The execution use case constructs the request from the aggregate and selected route.
/// Reusing AttemptId does not imply that every provider supports safe retries.
/// </remarks>
public sealed record ProviderOperationRequest(
    Guid MerchantId,
    Guid PaymentId,
    Guid OperationId,
    Guid AttemptId,
    Guid ProviderAccountId,
    PaymentOperationType Type,
    Money Amount,
    PaymentMethodType PaymentMethodType,
    string? OriginalProviderTransactionId,
    ProviderPaymentMethodReference? PaymentMethodReference);
