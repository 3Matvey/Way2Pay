namespace Way2Pay.Routing.Contracts.Conditions;

/// <summary>Tests whether the operation currency belongs to or is excluded from a non-empty currency set.</summary>
public sealed record CurrencyCondition : PaymentRoutingCondition
{
    public SetMembershipOperator Operator { get; }
    public IReadOnlyList<RoutingCurrencyCode> Currencies { get; }

    public CurrencyCondition(SetMembershipOperator @operator, IEnumerable<RoutingCurrencyCode> currencies)
    {
        ArgumentNullException.ThrowIfNull(currencies);
        var snapshot = currencies.ToArray();
        if (snapshot.Length == 0 || snapshot.Any(currency => currency is null))
            throw new ArgumentException("At least one non-null currency is required.", nameof(currencies));
        Operator = @operator;
        Currencies = Array.AsReadOnly(snapshot);
    }
}
