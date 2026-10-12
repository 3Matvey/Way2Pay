namespace Way2Pay.Routing.Contracts.Conditions;

public enum RoutingConditionGroupOperator
{
    /// <summary>Every child condition must match (AND).</summary>
    All,
    /// <summary>At least one child condition must match (OR).</summary>
    Any
}
