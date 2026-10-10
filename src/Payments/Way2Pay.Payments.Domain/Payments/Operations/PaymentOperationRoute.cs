using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.Payments.Operations;

/// <summary>An immutable routing snapshot saved before an operation's first provider attempt.</summary>
public sealed class PaymentOperationRoute
{
    public long ConfigurationVersion { get; }
    /// <summary>Null when the account is inherited from the original successful operation.</summary>
    public long? PolicyVersion { get; }
    /// <summary>Null for the default route or a route inherited from the original operation.</summary>
    public Guid? MatchedRuleId { get; }
    public string Explanation { get; }
    /// <summary>Provider accounts in routing priority order. Positions start at one.</summary>
    /// <remarks>An empty route records a decision with no available provider accounts.</remarks>
    public IReadOnlyList<PaymentOperationRouteStep> Steps { get; }

    internal PaymentOperationRoute(
        Guid operationId, long configurationVersion, long? policyVersion,
        IEnumerable<Guid> providerAccountIds, Guid? matchedRuleId, string explanation)
    {
        ValidateVersions(configurationVersion, policyVersion, matchedRuleId);
        ConfigurationVersion = configurationVersion;
        PolicyVersion = policyVersion;
        MatchedRuleId = matchedRuleId;
        Explanation = Guard.Required(explanation);
        Steps = CreateSteps(operationId, providerAccountIds);
    }

    private static void ValidateVersions(long configurationVersion, long? policyVersion, Guid? matchedRuleId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(configurationVersion);
        if (policyVersion is not null)
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(policyVersion.Value);
        if (matchedRuleId is not null)
            Guard.RequiredId(matchedRuleId.Value);
        if (policyVersion is null && matchedRuleId is not null)
            throw new ArgumentException("A matched rule requires a routing policy version.", nameof(matchedRuleId));
    }

    private static IReadOnlyList<PaymentOperationRouteStep> CreateSteps(
        Guid operationId, IEnumerable<Guid> providerAccountIds)
    {
        ArgumentNullException.ThrowIfNull(providerAccountIds);
        var accounts = providerAccountIds.ToArray();
        if (accounts.Distinct().Count() != accounts.Length)
            throw new ArgumentException("A provider account cannot occur twice in a route.", nameof(providerAccountIds));
        var steps = accounts.Select((accountId, index) =>
            new PaymentOperationRouteStep(operationId, index + 1, accountId)).ToArray();
        return Array.AsReadOnly(steps);
    }
}
