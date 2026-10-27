using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Application.Merchants;

public interface IMerchantRepository
{
    /// <summary>Loads a merchant by identifier, or returns null when it does not exist.</summary>
    Task<Merchant?> GetByIdAsync(Guid merchantId, CancellationToken cancellationToken = default);

    /// <summary>Stages the merchant without saving; persistence is performed by the unit of work.</summary>
    void Add(Merchant merchant);
}
