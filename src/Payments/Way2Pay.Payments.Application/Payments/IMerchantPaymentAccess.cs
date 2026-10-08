namespace Way2Pay.Payments.Application.Payments;

public interface IMerchantPaymentAccess
{
    /// <summary>Returns false for a missing merchant or one prohibited from accepting payments.</summary>
    Task<bool> CanAcceptPaymentsAsync(Guid merchantId, CancellationToken cancellationToken = default);
}
