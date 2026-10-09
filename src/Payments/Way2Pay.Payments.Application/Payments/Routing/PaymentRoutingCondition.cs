using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>Conditions combined with AND; a null field imposes no restriction.</summary>
/// <param name="Currency">The required currency. Must be specified whenever an amount bound is present.</param>
/// <param name="MinimumAmount">An inclusive lower bound expressed in Currency.</param>
/// <param name="MaximumAmount">An inclusive upper bound expressed in Currency.</param>
/// <param name="PaymentMethodType">The required kind of payment method.</param>
/// <remarks>
/// Publication must validate non-negative bounds, MinimumAmount not exceeding MaximumAmount,
/// and valid payment-method enum values. With every field null, the condition matches any context.
/// </remarks>
public sealed record PaymentRoutingCondition(
    CurrencyCode? Currency = null,
    decimal? MinimumAmount = null,
    decimal? MaximumAmount = null,
    PaymentMethodType? PaymentMethodType = null);
