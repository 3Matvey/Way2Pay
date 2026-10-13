using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Routing.Contracts;

namespace Way2Pay.Payments.Application.Routing;

/// <summary>Evaluates condition trees and chooses the first matching rule's eligible provider route.</summary>
/// <remarks>Rules are evaluated in policy order until the first match. Unsupported conditions stop evaluation.</remarks>
public sealed class PaymentRoutingEngine : IPaymentRoutingEngine
{
    public PaymentRoutingDecision Evaluate(
        PaymentRoutingContext context, PaymentRoutingPolicy policy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(policy);
        cancellationToken.ThrowIfCancellationRequested();
        if (context.OperationType is not (PaymentOperationType.Authorize or PaymentOperationType.Charge))
            throw new ArgumentException("Only Authorize and Charge operations use routing policies.", nameof(context));
        if (context.EligibleProviderAccounts.Count == 0)
            return PaymentRoutingDecision.Unroutable(context.ConfigurationVersion, policy.Version,
                PaymentRoutingFailureReason.NoEligibleAccounts, null, "No eligible provider accounts were supplied.");

        var matchedRule = FindMatchedRule(context, policy, cancellationToken);
        return CreateDecision(context, policy, matchedRule);
    }

    private static PaymentRoutingRule? FindMatchedRule(
        PaymentRoutingContext context, PaymentRoutingPolicy policy, CancellationToken cancellationToken)
    {
        foreach (var rule in policy.Rules)
        {
            if (RoutingConditionEvaluator.Matches(rule.Condition, context, cancellationToken))
                return rule;
        }
        return null;
    }

    /// <summary>Filters the chosen route by eligible accounts and records the result with its explanation.</summary>
    /// <remarks>
    /// Uses the matched rule's account list; the default route is used only when no rule matched.
    /// An empty matched-rule route does not trigger fallback. Filtering preserves route priority,
    /// regardless of the order of eligible accounts: route A, B, C with eligible B, C becomes B, C.
    /// The hash set is used only for membership checks; it does not determine the output order.
    /// If no accounts remain, returns NoCompatibleAccounts without trying another rule.
    /// Both outcomes retain the configuration and policy versions, the matched rule ID (if any),
    /// and an explanation identifying the selected route and any excluded accounts.
    /// </remarks>
    private static PaymentRoutingDecision CreateDecision(
        PaymentRoutingContext context, PaymentRoutingPolicy policy, PaymentRoutingRule? matchedRule)
    {
        var route = matchedRule?.ProviderAccountIds ?? policy.DefaultProviderAccountIds;
        var eligible = context.EligibleProviderAccounts.ToHashSet();
        var selected = route.Where(eligible.Contains).ToArray();
        var explanation = ExplainRoute(matchedRule, selected, route.Where(accountId => !eligible.Contains(accountId)));
        if (selected.Length == 0)
            return PaymentRoutingDecision.Unroutable(context.ConfigurationVersion, policy.Version,
                PaymentRoutingFailureReason.NoCompatibleAccounts, matchedRule?.Id, explanation);

        return PaymentRoutingDecision.Routed(context.ConfigurationVersion, policy.Version, selected, matchedRule?.Id, explanation);
    }

    private static string ExplainRoute(PaymentRoutingRule? matchedRule, IReadOnlyList<Guid> selected, IEnumerable<Guid> excluded)
    {
        var source = matchedRule is null ? "No rule matched; using the default route."
            : $"Matched rule '{matchedRule.Name}' ({matchedRule.Id}).";
        var outcome = selected.Count == 0 ? "No compatible accounts remain in the selected route."
            : $"Selected route: {string.Join(" -> ", selected)}.";
        var excludedIds = excluded.ToArray();
        return excludedIds.Length == 0 ? $"{source} {outcome}"
            : $"{source} {outcome} Excluded ineligible accounts: {string.Join(", ", excludedIds)}.";
    }
}
