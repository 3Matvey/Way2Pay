namespace Way2Pay.Routing.Contracts.Conditions;

/// <summary>A non-empty group combining child predicates and nested groups.</summary>
public sealed record RoutingConditionGroup : PaymentRoutingCondition
{
    public RoutingConditionGroupOperator Operator { get; }
    public IReadOnlyList<PaymentRoutingCondition> Conditions { get; }

    public RoutingConditionGroup(RoutingConditionGroupOperator @operator, IEnumerable<PaymentRoutingCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        var snapshot = conditions.ToArray();
        if (snapshot.Length == 0 || snapshot.Any(condition => condition is null))
            throw new ArgumentException("A group must contain at least one non-null condition.", nameof(conditions));
        Operator = @operator;
        Conditions = Array.AsReadOnly(snapshot);
    }
}
