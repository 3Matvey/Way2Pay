namespace Way2Pay.Routing.Contracts;

/// <summary>A three-letter uppercase currency code used in published routing conditions.</summary>
public sealed record RoutingCurrencyCode
{
    public string Value { get; }

    public RoutingCurrencyCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length != 3 || value.Any(character => character is < 'A' or > 'Z'))
            throw new ArgumentException("Currency must be a three-letter uppercase code.", nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
