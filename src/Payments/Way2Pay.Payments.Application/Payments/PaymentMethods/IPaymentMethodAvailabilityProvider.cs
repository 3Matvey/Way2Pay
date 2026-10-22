using Way2Pay.Payments.Domain.ValueObjects;

namespace Way2Pay.Payments.Application.Payments.PaymentMethods;

/// <summary>Obtains non-secret credential-source availability from CardVault.</summary>
public interface IPaymentMethodAvailabilityProvider
{
    /// <summary>Returns null when the source does not exist or does not belong to the merchant.</summary>
    /// <remarks>
    /// The source identifier is not an access grant. Retrieval failures must propagate rather than
    /// appear as a missing source. Connector and CardVault recheck access before releasing credentials.
    /// </remarks>
    Task<PaymentMethodAvailability?> GetAsync(
        Guid merchantId, PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
}
