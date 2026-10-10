namespace Way2Pay.Payments.Application.Payments.Configuration;

/// <summary>Provides a merchant configuration snapshot without exposing its storage or transport.</summary>
public interface IMerchantPaymentConfigurationProvider
{
    /// <summary>Returns null only when the merchant does not exist.</summary>
    /// <remarks>
    /// Infrastructure may fetch the snapshot from Merchant Service or a cache.
    /// Service unavailability and retrieval failures must not be represented as a missing merchant.
    /// All returned data must belong to one configuration version; an empty account list is valid.
    /// </remarks>
    Task<MerchantPaymentConfiguration?> GetAsync(
        Guid merchantId, CancellationToken cancellationToken = default);
}
