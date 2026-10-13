using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Routing.Contracts.Conditions;

namespace Way2Pay.Payments.Application.Routing;

/// <summary>Evaluates typed conditions directly. Groups stop when their result is determined.</summary>
internal static class RoutingConditionEvaluator
{
    internal static bool Matches(
        PaymentRoutingCondition condition, PaymentRoutingContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return condition switch
        {
            RoutingConditionGroup group => MatchesGroup(group, context, cancellationToken),
            AmountCondition amount => MatchesAmount(amount, context),
            CurrencyCondition currency => MatchesMembership(
                currency.Currencies.Any(value => value.Value == context.Amount.Currency.Value), currency.Operator),
            PaymentMethodCondition method => MatchesMembership(
                method.PaymentMethods.Contains(context.PaymentMethodType), method.Operator),
            _ => throw new InvalidOperationException("The routing condition type is not supported.")
        };
    }

    private static bool MatchesGroup(
        RoutingConditionGroup group, PaymentRoutingContext context, CancellationToken cancellationToken) => group.Operator switch
    {
        RoutingConditionGroupOperator.All => group.Conditions.All(condition => Matches(condition, context, cancellationToken)),
        RoutingConditionGroupOperator.Any => group.Conditions.Any(condition => Matches(condition, context, cancellationToken)),
        _ => throw new InvalidOperationException("The condition group operator is not supported.")
    };

    private static bool MatchesAmount(
        AmountCondition condition, PaymentRoutingContext context)
    {
        if (condition.Currency.Value != context.Amount.Currency.Value)
            return false;

        return condition.Operator switch
        {
            AmountComparisonOperator.Equal => context.Amount.Amount == condition.Amount,
            AmountComparisonOperator.GreaterThan => context.Amount.Amount > condition.Amount,
            AmountComparisonOperator.GreaterThanOrEqual => context.Amount.Amount >= condition.Amount,
            AmountComparisonOperator.LessThan => context.Amount.Amount < condition.Amount,
            AmountComparisonOperator.LessThanOrEqual => context.Amount.Amount <= condition.Amount,
            _ => throw new InvalidOperationException("The amount comparison operator is not supported.")
        };
    }

    private static bool MatchesMembership(bool contains, SetMembershipOperator @operator) => @operator switch
    {
        SetMembershipOperator.In => contains,
        SetMembershipOperator.NotIn => !contains,
        _ => throw new InvalidOperationException("The membership operator is not supported.")
    };
}
