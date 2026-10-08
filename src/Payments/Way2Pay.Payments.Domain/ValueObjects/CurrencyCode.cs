using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.ValueObjects;

public sealed record CurrencyCode
{
    public string Value { get; }

    public CurrencyCode(string value)
    {
        Guard.Required(value);
        if (value.Length != 3 || value.Any(c => c is < 'A' or > 'Z'))
            throw new ArgumentException("Currency must be a three-letter uppercase code.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value;
}
