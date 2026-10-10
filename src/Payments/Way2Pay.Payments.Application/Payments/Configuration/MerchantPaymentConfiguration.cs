using Way2Pay.Payments.Application.Payments.Routing;
using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Application.Payments.Configuration;

/// <summary>An immutable merchant configuration snapshot consumed by payment execution.</summary>
/// <remarks>
/// This is a transfer object, not a Payments persistence entity. Account ownership and policy
/// references are validated by Merchant Service when publishing the configuration.
/// </remarks>
public sealed record MerchantPaymentConfiguration
{
    public Guid MerchantId { get; }
    /// <summary>Changes when the merchant account configuration or its active routing policy changes.</summary>
    public long Version { get; }
    public IReadOnlyList<ProviderAccountConfiguration> ProviderAccounts { get; }
    public PaymentRoutingPolicy RoutingPolicy { get; }

    public MerchantPaymentConfiguration(
        Guid merchantId, long version, IEnumerable<ProviderAccountConfiguration> providerAccounts,
        PaymentRoutingPolicy routingPolicy)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentNullException.ThrowIfNull(routingPolicy);
        MerchantId = Guard.RequiredId(merchantId);
        Version = version;
        RoutingPolicy = routingPolicy;
        var accounts = providerAccounts.ToArray();
        if (accounts.Any(account => account is null) || accounts.Select(account => account.ProviderAccountId).Distinct().Count() != accounts.Length)
            throw new ArgumentException("Accounts must be non-null and have unique identifiers.", nameof(providerAccounts));
        ProviderAccounts = Array.AsReadOnly(accounts);
    }
}
