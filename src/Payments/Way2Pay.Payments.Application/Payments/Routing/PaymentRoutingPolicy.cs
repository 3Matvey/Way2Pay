namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>A published policy snapshot with ordered rules and a default account route.</summary>
/// <remarks>
/// Rule order determines precedence. The default route is used only if no rule matches.
/// An empty default route means there is no fallback. Publishing a new version creates a new snapshot.
/// Rule conditions and account ownership are validated during publication.
/// </remarks>
public sealed record PaymentRoutingPolicy
{
    public long Version { get; }
    public IReadOnlyList<PaymentRoutingRule> Rules { get; }
    public IReadOnlyList<Guid> DefaultProviderAccountIds { get; }

    public PaymentRoutingPolicy(
        long version, IEnumerable<PaymentRoutingRule> rules, IEnumerable<Guid> defaultProviderAccountIds)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version, nameof(version));
        ArgumentNullException.ThrowIfNull(rules);
        var snapshot = rules.ToArray();
        if (snapshot.Any(rule => rule is null) || snapshot.Select(rule => rule.Id).Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Rules must be non-null and have unique identifiers.", nameof(rules));
        Version = version;
        Rules = Array.AsReadOnly(snapshot);
        DefaultProviderAccountIds = RoutingAccounts.Copy(defaultProviderAccountIds);
    }
}
