using Way2Pay.Routing.Contracts;
using Way2Pay.Routing.Contracts.Conditions;
using static Way2Pay.Payments.Application.Tests.Routing.RoutingTestData;

namespace Way2Pay.Payments.Application.Tests.Routing;

public sealed class RoutingConditionTests
{
    [Theory]
    [InlineData(AmountComparisonOperator.Equal, 99, false)]
    [InlineData(AmountComparisonOperator.Equal, 100, true)]
    [InlineData(AmountComparisonOperator.Equal, 101, false)]
    [InlineData(AmountComparisonOperator.GreaterThan, 99, false)]
    [InlineData(AmountComparisonOperator.GreaterThan, 100, false)]
    [InlineData(AmountComparisonOperator.GreaterThan, 101, true)]
    [InlineData(AmountComparisonOperator.GreaterThanOrEqual, 99, false)]
    [InlineData(AmountComparisonOperator.GreaterThanOrEqual, 100, true)]
    [InlineData(AmountComparisonOperator.GreaterThanOrEqual, 101, true)]
    [InlineData(AmountComparisonOperator.LessThan, 99, true)]
    [InlineData(AmountComparisonOperator.LessThan, 100, false)]
    [InlineData(AmountComparisonOperator.LessThan, 101, false)]
    [InlineData(AmountComparisonOperator.LessThanOrEqual, 99, true)]
    [InlineData(AmountComparisonOperator.LessThanOrEqual, 100, true)]
    [InlineData(AmountComparisonOperator.LessThanOrEqual, 101, false)]
    public void Evaluate_AmountComparison_RespectsThresholdBoundary(
        AmountComparisonOperator comparison, int amount, bool matches)
    {
        var condition = new AmountCondition(new RoutingCurrencyCode("BYN"), comparison, 100);
        var decision = EvaluateCondition(condition, CreateContext(amount));

        Assert.True(decision.IsRoutable);
        Assert.Equal(matches, decision.MatchedRuleId.HasValue);
    }

    [Fact]
    public void Evaluate_AmountConditionWithDifferentCurrency_DoesNotMatch()
    {
        var condition = new AmountCondition(new RoutingCurrencyCode("USD"), AmountComparisonOperator.Equal, 100);
        var decision = EvaluateCondition(condition, CreateContext(100, "BYN"));

        Assert.Null(decision.MatchedRuleId);
        Assert.Equal(new[] { ThirdProvider }, decision.ProviderAccountIds);
    }

    [Theory]
    [InlineData(SetMembershipOperator.In, "BYN", true)]
    [InlineData(SetMembershipOperator.In, "EUR", false)]
    [InlineData(SetMembershipOperator.NotIn, "BYN", false)]
    [InlineData(SetMembershipOperator.NotIn, "EUR", true)]
    public void Evaluate_CurrencyMembership_RespectsInAndNotIn(
        SetMembershipOperator membership, string currency, bool matches)
    {
        var condition = new CurrencyCondition(membership, [new RoutingCurrencyCode("USD"), new RoutingCurrencyCode("BYN")]);
        var decision = EvaluateCondition(condition, CreateContext(currency: currency));

        Assert.Equal(matches, decision.MatchedRuleId.HasValue);
    }

    [Theory]
    [InlineData(SetMembershipOperator.In, RoutingPaymentMethodType.Card, true)]
    [InlineData(SetMembershipOperator.In, RoutingPaymentMethodType.BankAccount, false)]
    [InlineData(SetMembershipOperator.NotIn, RoutingPaymentMethodType.Card, false)]
    [InlineData(SetMembershipOperator.NotIn, RoutingPaymentMethodType.BankAccount, true)]
    public void Evaluate_PaymentMethodMembership_RespectsInAndNotIn(
        SetMembershipOperator membership, RoutingPaymentMethodType method, bool matches)
    {
        var condition = new PaymentMethodCondition(membership, [RoutingPaymentMethodType.Card]);
        var decision = EvaluateCondition(condition, CreateContext(method: method));

        Assert.Equal(matches, decision.MatchedRuleId.HasValue);
    }

    [Theory]
    [InlineData(RoutingConditionGroupOperator.All, RoutingPaymentMethodType.Card, "BYN", true)]
    [InlineData(RoutingConditionGroupOperator.All, RoutingPaymentMethodType.Card, "USD", false)]
    [InlineData(RoutingConditionGroupOperator.All, RoutingPaymentMethodType.BankAccount, "BYN", false)]
    [InlineData(RoutingConditionGroupOperator.All, RoutingPaymentMethodType.BankAccount, "USD", false)]
    [InlineData(RoutingConditionGroupOperator.Any, RoutingPaymentMethodType.Card, "BYN", true)]
    [InlineData(RoutingConditionGroupOperator.Any, RoutingPaymentMethodType.Card, "USD", true)]
    [InlineData(RoutingConditionGroupOperator.Any, RoutingPaymentMethodType.BankAccount, "BYN", true)]
    [InlineData(RoutingConditionGroupOperator.Any, RoutingPaymentMethodType.BankAccount, "USD", false)]
    public void Evaluate_Group_CombinesChildrenAccordingToOperator(
        RoutingConditionGroupOperator groupOperator, RoutingPaymentMethodType method, string currency, bool matches)
    {
        var condition = new RoutingConditionGroup(groupOperator,
            [CurrencyIs("BYN"), new PaymentMethodCondition(SetMembershipOperator.In, [RoutingPaymentMethodType.Card])]);
        var decision = EvaluateCondition(condition, CreateContext(currency: currency, method: method));

        Assert.Equal(matches, decision.MatchedRuleId.HasValue);
    }

    [Theory]
    [InlineData(100, "BYN", true)]
    [InlineData(100, "USD", true)]
    [InlineData(50, "BYN", false)]
    [InlineData(100, "EUR", false)]
    public void Evaluate_NestedGroups_RequireAmountAndEitherCurrency(int amount, string currency, bool matches)
    {
        var currencies = new RoutingConditionGroup(RoutingConditionGroupOperator.Any, [CurrencyIs("BYN"), CurrencyIs("USD")]);
        var condition = new RoutingConditionGroup(RoutingConditionGroupOperator.All,
            [new AmountCondition(new RoutingCurrencyCode(currency), AmountComparisonOperator.GreaterThanOrEqual, 100), currencies]);
        var decision = EvaluateCondition(condition, CreateContext(amount, currency));

        Assert.Equal(matches, decision.MatchedRuleId.HasValue);
    }
}
