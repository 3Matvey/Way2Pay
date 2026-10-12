namespace Way2Pay.Routing.Contracts.Conditions;

/// <summary>A node in an immutable routing condition tree.</summary>
/// <remarks>Publication validates supported operators, tree depth and node count before activating a policy.</remarks>
public abstract record PaymentRoutingCondition
{
    private protected PaymentRoutingCondition()
    {
    }
}
