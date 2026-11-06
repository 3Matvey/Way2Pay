namespace Way2Pay.Payments.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount
    { 
        get;
        private init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "The amount cannot be negative.");
            if (decimal.Round(value, 4) != value)
                throw new ArgumentOutOfRangeException(nameof(value), "Amount must have at most four significant decimal places.");
            field = value;
        }
    }
    public CurrencyCode Currency { get; }

    public Money(decimal amount, CurrencyCode currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(CurrencyCode currency) => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (Currency != other.Currency)
            throw new InvalidOperationException("Money values must have the same currency.");
    }
}
