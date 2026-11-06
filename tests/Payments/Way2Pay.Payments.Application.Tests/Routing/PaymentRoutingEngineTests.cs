using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Application.Routing;
using Way2Pay.Payments.Domain.Payments.Operations;
using Way2Pay.Routing.Contracts;
using static Way2Pay.Payments.Application.Tests.Routing.RoutingTestData;

namespace Way2Pay.Payments.Application.Tests.Routing;

public sealed class PaymentRoutingEngineTests
{
    [Fact]
    public void Evaluate_WhenMultipleRulesMatch_SelectsFirstMatchInPolicyOrder()
    {
        var nonmatching = CreateRule(CurrencyIs("USD"));
        var firstMatch = CreateRule(CurrencyIs("BYN"), [SecondProvider]);
        var laterMatch = CreateRule(CurrencyIs("BYN"), [FirstProvider]);
        var decision = new PaymentRoutingEngine().Evaluate(CreateContext(), CreatePolicy([nonmatching, firstMatch, laterMatch]));

        Assert.True(decision.IsRoutable);
        Assert.Equal(firstMatch.Id, decision.MatchedRuleId);
        Assert.Equal(new[] { SecondProvider }, decision.ProviderAccountIds);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Evaluate_WhenNoRuleMatches_UsesDefaultRoute(bool emptyPolicy)
    {
        PaymentRoutingRule[] rules = emptyPolicy ? [] : [CreateRule(CurrencyIs("USD"))];
        var decision = new PaymentRoutingEngine().Evaluate(CreateContext(), CreatePolicy(rules, [SecondProvider]));

        Assert.True(decision.IsRoutable);
        Assert.Null(decision.MatchedRuleId);
        Assert.Equal(new[] { SecondProvider }, decision.ProviderAccountIds);
        Assert.Equal(ConfigurationVersion, decision.ConfigurationVersion);
        Assert.Equal(PolicyVersion, decision.PolicyVersion);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Evaluate_WhenFilteringAccounts_PreservesChosenRouteOrder(bool useRule)
    {
        Guid[] route = [FirstProvider, SecondProvider, ThirdProvider];
        var rule = CreateRule(CurrencyIs("BYN"), route);
        var context = CreateContext(eligibleAccounts: [ThirdProvider, SecondProvider]);
        var policy = CreatePolicy(useRule ? [rule] : [], route);
        var decision = new PaymentRoutingEngine().Evaluate(context, policy);

        Assert.True(decision.IsRoutable);
        Assert.Equal(new[] { SecondProvider, ThirdProvider }, decision.ProviderAccountIds);
        Assert.Equal(useRule ? rule.Id : (Guid?)null, decision.MatchedRuleId);
        Assert.Contains(FirstProvider.ToString(), decision.Explanation);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Evaluate_WhenMatchedRouteHasNoCompatibleAccounts_DoesNotTryLaterRuleOrDefault(bool emptyRoute)
    {
        var matched = CreateRule(CurrencyIs("BYN"), emptyRoute ? [] : [FirstProvider]);
        var later = CreateRule(CurrencyIs("BYN"), [SecondProvider]);
        var policy = CreatePolicy([matched, later], [ThirdProvider]);
        var decision = new PaymentRoutingEngine().Evaluate(
            CreateContext(eligibleAccounts: [SecondProvider, ThirdProvider]), policy);

        Assert.False(decision.IsRoutable);
        Assert.Equal(PaymentRoutingFailureReason.NoCompatibleAccounts, decision.FailureReason);
        Assert.Equal(matched.Id, decision.MatchedRuleId);
        Assert.Empty(decision.ProviderAccountIds);
    }

    [Fact]
    public void Evaluate_WhenNoAccountsAreEligible_ReturnsNoEligibleAccounts()
    {
        var policy = CreatePolicy([CreateRule(CurrencyIs("BYN"))]);
        var decision = new PaymentRoutingEngine().Evaluate(CreateContext(eligibleAccounts: []), policy);

        Assert.False(decision.IsRoutable);
        Assert.Equal(PaymentRoutingFailureReason.NoEligibleAccounts, decision.FailureReason);
        Assert.Null(decision.MatchedRuleId);
        Assert.Empty(decision.ProviderAccountIds);
        Assert.Equal(ConfigurationVersion, decision.ConfigurationVersion);
        Assert.Equal(PolicyVersion, decision.PolicyVersion);
    }

    [Fact]
    public void Evaluate_WhenDefaultRouteIsEmpty_ReturnsNoCompatibleAccounts()
    {
        var decision = new PaymentRoutingEngine().Evaluate(CreateContext(), CreatePolicy([], []));

        Assert.False(decision.IsRoutable);
        Assert.Equal(PaymentRoutingFailureReason.NoCompatibleAccounts, decision.FailureReason);
        Assert.Null(decision.MatchedRuleId);
        Assert.Empty(decision.ProviderAccountIds);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Evaluate_ForMatchedRule_PreservesSnapshotMetadataForBothOutcomes(bool routable)
    {
        var rule = CreateRule(CurrencyIs("BYN"), routable ? [FirstProvider] : []);
        var decision = new PaymentRoutingEngine().Evaluate(CreateContext(), CreatePolicy([rule]));

        Assert.Equal(routable, decision.IsRoutable);
        Assert.Equal(ConfigurationVersion, decision.ConfigurationVersion);
        Assert.Equal(PolicyVersion, decision.PolicyVersion);
        Assert.Equal(rule.Id, decision.MatchedRuleId);
        Assert.False(string.IsNullOrWhiteSpace(decision.Explanation));
    }

    [Fact]
    public void Evaluate_WhenCancelled_ThrowsBeforeRouting()
    {
        var context = CreateContext();
        var policy = CreatePolicy([CreateRule(CurrencyIs("BYN"))]);
        var cancellationToken = new CancellationToken(canceled: true);

        Assert.Throws<OperationCanceledException>(() =>
            new PaymentRoutingEngine().Evaluate(context, policy, cancellationToken));
    }

    [Theory]
    [InlineData(PaymentOperationType.Capture)]
    [InlineData(PaymentOperationType.Void)]
    [InlineData(PaymentOperationType.Refund)]
    public void Evaluate_ForFollowUpOperation_RejectsPolicyRouting(PaymentOperationType type)
    {
        var context = CreateContext() with { OperationType = type };
        var policy = CreatePolicy([CreateRule(CurrencyIs("BYN"))]);

        Assert.Throws<ArgumentException>(() => new PaymentRoutingEngine().Evaluate(context, policy));
    }
}
