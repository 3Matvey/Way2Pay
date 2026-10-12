using Way2Pay.Routing.Contracts.Conditions;

namespace Way2Pay.Routing.Contracts;

/// <summary>A named rule with a condition and an ordered route of provider accounts.</summary>
/// <remarks>An empty account list explicitly excludes routing for matching operations.</remarks>
public sealed record PaymentRoutingRule
{
    public Guid Id { get; }
    public string Name { get; }
    public PaymentRoutingCondition Condition { get; }
    public IReadOnlyList<Guid> ProviderAccountIds { get; }

    public PaymentRoutingRule(
        Guid id, string name, PaymentRoutingCondition condition, IEnumerable<Guid> providerAccountIds)
    {
        ArgumentNullException.ThrowIfNull(condition);
        if (id == Guid.Empty)
            throw new ArgumentException("A rule identifier is required.", nameof(id));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
        Condition = condition;
        ProviderAccountIds = RoutingAccounts.Copy(providerAccountIds);
    }
}
