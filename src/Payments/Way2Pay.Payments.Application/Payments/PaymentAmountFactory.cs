using Way2Pay.Payments.Application.Common.Results;
using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments;

internal static class PaymentAmountFactory
{
    public static Result<Money> Create(decimal amount, string currency)
    {
        if (amount <= 0)
            return PaymentErrors.InvalidAmount();
        try
        {
            return new Money(amount, new CurrencyCode(currency));
        }
        catch (ArgumentException)
        {
            return PaymentErrors.InvalidCurrency();
        }
    }
}
