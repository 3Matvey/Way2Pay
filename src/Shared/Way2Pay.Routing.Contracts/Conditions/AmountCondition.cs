namespace Way2Pay.Routing.Contracts.Conditions;

/// <summary>Matches the specified currency and compares the operation amount with a non-negative threshold.</summary>
/// <remarks>A different currency makes the condition false; currency conversion is never performed.</remarks>
public sealed record AmountCondition : PaymentRoutingCondition
{
    public RoutingCurrencyCode Currency { get; }
    public AmountComparisonOperator Operator { get; }
    public decimal Amount { get; }

    public AmountCondition(RoutingCurrencyCode currency, AmountComparisonOperator @operator, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        Currency = currency;
        Operator = @operator;
        Amount = amount;
    }
}
