namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>The kind of payment method, independent of the provider or connection account.</summary>
public enum PaymentMethodType
{
    Card,
    BankAccount
}
