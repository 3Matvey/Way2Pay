namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>An expected reason for not producing a provider route.</summary>
public enum PaymentRoutingFailureReason
{
    /// <summary>Application supplied no eligible provider accounts.</summary>
    NoEligibleAccounts,

    /// <summary>The selected rule or default route contains no eligible accounts.</summary>
    NoCompatibleAccounts
}
