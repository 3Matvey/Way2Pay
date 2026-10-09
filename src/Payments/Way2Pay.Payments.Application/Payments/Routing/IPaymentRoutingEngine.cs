namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>Evaluates published routing rules without network or storage access.</summary>
/// <remarks>
/// Applies only to Authorize and Charge. Follow-up operations use their original provider account.
/// Rules are evaluated in collection order; all specified conditions must match.
/// The first matching rule supplies the route. The default route applies only when no rule matches.
/// Account IDs are intersected with eligible accounts while preserving the policy order.
/// An empty result is an unroutable decision; a matching rule does not fall back to the default route.
/// </remarks>
public interface IPaymentRoutingEngine
{
    PaymentRoutingDecision Evaluate(PaymentRoutingContext context, PaymentRoutingPolicy policy);
}
