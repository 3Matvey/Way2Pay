using Way2Pay.Payments.Domain.Common;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Domain.Payments;

/// <summary>Confirmed amounts for one payment. Authorized is the remaining authorization.</summary>
public sealed class PaymentBalance
{
    public Money Authorized { get; private set; }
    public Money Captured { get; private set; }
    public Money Refunded { get; private set; }
    public Money Refundable => Captured.Subtract(Refunded);

    public PaymentBalance(CurrencyCode currency)
    {
        Authorized = Money.Zero(currency);
        Captured = Money.Zero(currency);
        Refunded = Money.Zero(currency);
    }

    internal void Authorize(Money amount)
    {
        EnsureValidAmount(amount);
        Authorized = amount;
    }

    internal void Capture(Money amount)
    {
        EnsureValidAmount(amount);
        if (amount != Authorized)
            throw new InvalidOperationException("Capture must match the full authorized amount.");

        Authorized = Money.Zero(Authorized.Currency);
        Captured = amount;
    }

    internal void Charge(Money amount)
    {
        EnsureValidAmount(amount);
        Captured = amount;
    }

    internal void Void()
    {
        Authorized = Money.Zero(Authorized.Currency);
    }

    internal void Refund(Money amount)
    {
        EnsureValidAmount(amount);
        if (!CanRefund(amount))
            throw new InvalidOperationException("Refund amount exceeds the refundable amount.");

        Refunded = Refunded.Add(amount);
    }

    internal bool CanRefund(Money? amount) =>
        amount is not null
        && amount.Currency == Captured.Currency
        && amount.Amount > 0
        && amount.Amount <= Refundable.Amount;

    private void EnsureValidAmount(Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Guard.Positive(amount.Amount);
        if (amount.Currency != Authorized.Currency)
            throw new InvalidOperationException("The amount must use the payment currency.");
    }
}
