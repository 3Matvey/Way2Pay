using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Application.Routing;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Payments.Domain.ValueObjects;
using Way2Pay.Routing.Contracts;
using Way2Pay.Routing.Contracts.Conditions;

namespace Way2Pay.Payments.Application.Tests.Routing;

internal static class RoutingTestData
{
    public static readonly Guid FirstProvider = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid SecondProvider = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ThirdProvider = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public const long ConfigurationVersion = 11;
    public const long PolicyVersion = 7;

    public static PaymentRoutingContext CreateContext(
        decimal amount = 100, string currency = "BYN", IReadOnlyList<Guid>? eligibleAccounts = null,
        RoutingPaymentMethodType method = RoutingPaymentMethodType.Card) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PaymentOperationType.Charge,
            new Money(amount, new CurrencyCode(currency)), method, ConfigurationVersion,
            eligibleAccounts ?? [FirstProvider, SecondProvider, ThirdProvider]);

    public static CurrencyCondition CurrencyIs(string currency) =>
        new(SetMembershipOperator.In, [new RoutingCurrencyCode(currency)]);

    public static PaymentRoutingRule CreateRule(
        PaymentRoutingCondition condition, IEnumerable<Guid>? accounts = null) =>
        new(Guid.NewGuid(), "Test rule", condition, accounts ?? [FirstProvider]);

    public static PaymentRoutingPolicy CreatePolicy(
        IEnumerable<PaymentRoutingRule> rules, IEnumerable<Guid>? defaultAccounts = null) =>
        new(PolicyVersion, rules, defaultAccounts ?? [ThirdProvider]);

    public static PaymentRoutingDecision EvaluateCondition(
        PaymentRoutingCondition condition, PaymentRoutingContext context) =>
        new PaymentRoutingEngine().Evaluate(context, CreatePolicy([CreateRule(condition)]));
}
