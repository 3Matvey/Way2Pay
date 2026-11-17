using Way2Pay.Payments.Application.Payments;

namespace Way2Pay.Payments.Api.Authentication;

// The forwarded identity is trusted only on a network that restricts direct access to Payments.
internal sealed class GatewayMerchantAccess(IHttpContextAccessor contextAccessor) : IMerchantPaymentAccess
{
    private const string MerchantIdHeader = "X-Way2Pay-Merchant-Id";

    internal Guid? MerchantId
    {
        get
        {
            var headers = contextAccessor.HttpContext?.Request.Headers;
            if (headers is null || !headers.TryGetValue(MerchantIdHeader, out var values) || values.Count != 1)
                return null;

            return Guid.TryParse(values[0], out var id) && id != Guid.Empty ? id : null;
        }
    }

    public Task<bool> CanAcceptPaymentsAsync(Guid merchantId, CancellationToken cancellationToken = default) =>
        Task.FromResult(MerchantId == merchantId);
}
