using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Application.Payments.Routing;

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
        Id = Guard.RequiredId(id);
        Name = Guard.Required(name);
        Condition = condition;
        ProviderAccountIds = RoutingAccounts.Copy(providerAccountIds);
    }
}
