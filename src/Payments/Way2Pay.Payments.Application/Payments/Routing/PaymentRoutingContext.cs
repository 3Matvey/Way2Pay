using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>The operation context and prefiltered accounts used for a routing decision.</summary>
/// <param name="MerchantId">The merchant owning all eligible accounts.</param>
/// <param name="PaymentId">The payment being processed.</param>
/// <param name="OperationId">The Authorize or Charge operation being routed.</param>
/// <param name="OperationType">Authorize or Charge; follow-up operations bypass the engine.</param>
/// <param name="Amount">The positive operation amount and currency.</param>
/// <param name="PaymentMethodType">The kind of payment method used by the payment.</param>
/// <param name="ConfigurationVersion">The version of the merchant configuration used to filter candidates.</param>
/// <param name="EligibleProviderAccounts">Active merchant accounts supporting the operation, currency and method binding.</param>
/// <remarks>Application prepares the candidates. Their order does not define routing priority.</remarks>
public sealed record PaymentRoutingContext(
    Guid MerchantId,
    Guid PaymentId,
    Guid OperationId,
    PaymentOperationType OperationType,
    Money Amount,
    PaymentMethodType PaymentMethodType,
    long ConfigurationVersion,
    IReadOnlyList<Guid> EligibleProviderAccounts)
{
    public IReadOnlyList<Guid> EligibleProviderAccounts { get; } = RoutingAccounts.Copy(EligibleProviderAccounts);
}
