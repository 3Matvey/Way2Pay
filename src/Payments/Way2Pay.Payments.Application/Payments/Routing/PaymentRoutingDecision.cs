using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>A route or explained refusal, tagged with the configuration and policy versions used.</summary>
/// <remarks>Execution persists this decision for the operation before starting its first attempt.</remarks>
public sealed record PaymentRoutingDecision
{
    public long ConfigurationVersion { get; }
    public long PolicyVersion { get; }
    public IReadOnlyList<Guid> ProviderAccountIds { get; }

    /// <summary>The first matching rule; null means the default route or no eligible candidates.</summary>
    public Guid? MatchedRuleId { get; }

    /// <summary>Human-readable reasons for the rule match, account ordering or refusal.</summary>
    public string Explanation { get; }
    public PaymentRoutingFailureReason? FailureReason { get; }
    public bool IsRoutable => FailureReason is null;

    private PaymentRoutingDecision(
        long configurationVersion, long policyVersion, IReadOnlyList<Guid> providerAccountIds,
        Guid? matchedRuleId, string explanation, PaymentRoutingFailureReason? failureReason)
    {
        if (configurationVersion <= 0 || policyVersion <= 0)
            throw new ArgumentException("Configuration and policy versions must be positive.");
        if (matchedRuleId == Guid.Empty)
            throw new ArgumentException("A matched rule identifier cannot be empty.", nameof(matchedRuleId));
        ConfigurationVersion = configurationVersion;
        PolicyVersion = policyVersion;
        ProviderAccountIds = providerAccountIds;
        MatchedRuleId = matchedRuleId;
        Explanation = Guard.Required(explanation);
        FailureReason = failureReason;
    }

    public static PaymentRoutingDecision Routed(
        long configurationVersion, long policyVersion, IEnumerable<Guid> providerAccountIds,
        Guid? matchedRuleId, string explanation)
    {
        var accounts = RoutingAccounts.Copy(providerAccountIds);
        if (accounts.Count == 0)
            throw new ArgumentException("A successful route must contain an account.", nameof(providerAccountIds));
        return new(configurationVersion, policyVersion, accounts, matchedRuleId, explanation, null);
    }

    public static PaymentRoutingDecision Unroutable(
        long configurationVersion, long policyVersion, PaymentRoutingFailureReason reason,
        Guid? matchedRuleId, string explanation)
    {
        return new(configurationVersion, policyVersion, [], matchedRuleId, explanation, reason);
    }
}
