using Way2Pay.Payments.Application.Payments.Configuration;
using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Domain.Payments;
using Way2Pay.Payments.Domain.Payments.Operations;

namespace Way2Pay.Payments.Application.Payments.ExecutePaymentOperation;

/// <summary>Prepares routing inputs and maps decisions to the aggregate without storage or provider calls.</summary>
internal static class PaymentExecutionRouting
{
    internal static bool IsInitial(PaymentOperation operation) =>
        operation.Type is PaymentOperationType.Authorize or PaymentOperationType.Charge;

    internal static Guid[] GetEligibleAccounts(
        Payment payment, PaymentOperation operation, MerchantPaymentConfiguration configuration)
    {
        return configuration.ProviderAccounts
            .Where(account => IsAccountEligible(account, payment, operation))
            .Select(account => account.ProviderAccountId)
            .ToArray();
    }

    private static bool IsAccountEligible(
        ProviderAccountConfiguration account, Payment payment, PaymentOperation operation)
    {
        if (!account.IsActive || !SupportsOperation(account, payment, operation))
            return false;
        if (!IsInitial(operation))
            return true;
        return payment.PaymentMethod.ProviderReferences.Any(
            reference => reference.ProviderAccountId == account.ProviderAccountId);
    }

    private static bool SupportsOperation(
        ProviderAccountConfiguration account, Payment payment, PaymentOperation operation) =>
        account.SupportedOperations.Contains(operation.Type)
        && account.SupportedCurrencies.Contains(operation.Amount.Currency)
        && account.SupportedPaymentMethods.Contains(payment.PaymentMethod.Type);

    internal static void SetRoute(
        Payment payment, PaymentOperation operation, MerchantPaymentConfiguration configuration,
        IReadOnlyList<Guid> eligibleAccounts, IPaymentRoutingEngine routingEngine)
    {
        if (IsInitial(operation))
            SetInitialRoute(payment, operation, configuration, eligibleAccounts, routingEngine);
        else
            SetOriginalAccountRoute(payment, operation, configuration.Version);
    }

    private static void SetOriginalAccountRoute(Payment payment, PaymentOperation operation, long configurationVersion)
    {
        var originalAccountId = payment.GetOriginalProviderAccountId(operation.Id);
        payment.SetOperationRoute(operation.Id, configurationVersion, null,
            [originalAccountId], null, "Use the account of the original successful operation.");
    }

    private static void SetInitialRoute(
        Payment payment, PaymentOperation operation, MerchantPaymentConfiguration configuration,
        IReadOnlyList<Guid> eligibleAccounts, IPaymentRoutingEngine routingEngine)
    {
        var context = new PaymentRoutingContext(payment.MerchantId, payment.Id, operation.Id,
            operation.Type, operation.Amount, payment.PaymentMethod.Type, configuration.Version, eligibleAccounts);
        var decision = routingEngine.Evaluate(context, configuration.RoutingPolicy);
        EnsureDecisionMatchesConfiguration(decision, configuration, eligibleAccounts);
        payment.SetOperationRoute(operation.Id, decision.ConfigurationVersion, decision.PolicyVersion,
            decision.ProviderAccountIds, decision.MatchedRuleId, decision.Explanation);
    }

    private static void EnsureDecisionMatchesConfiguration(
        PaymentRoutingDecision decision, MerchantPaymentConfiguration configuration, IReadOnlyList<Guid> eligibleAccounts)
    {
        if (decision.ConfigurationVersion != configuration.Version
            || decision.PolicyVersion != configuration.RoutingPolicy.Version)
            throw new InvalidOperationException("The routing decision uses different configuration or policy versions.");
        if (decision.ProviderAccountIds.Any(accountId => !eligibleAccounts.Contains(accountId)))
            throw new InvalidOperationException("The routing decision contains an ineligible provider account.");
    }

    /// <summary>Returns steps after the last attempted position; previously skipped steps are not reconsidered.</summary>
    internal static PaymentOperationRouteStep[] GetRemainingSteps(PaymentOperation operation)
    {
        var route = operation.Route ?? throw new InvalidOperationException("The operation route has not been assigned.");
        var lastAttempt = operation.Attempts.LastOrDefault();
        var lastPosition = lastAttempt is null ? 0
            : route.Steps.Single(step => step.Id == lastAttempt.RouteStepId).Position;
        return route.Steps
            .Where(step => step.Position > lastPosition)
            .OrderBy(step => step.Position)
            .ToArray();
    }
}
