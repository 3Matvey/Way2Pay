using Microsoft.EntityFrameworkCore;
using Way2Pay.Merchants.Application.Merchants;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Infrastructure.Persistence.Repositories;

internal sealed class MerchantRepository(MerchantsDbContext dbContext) : IMerchantRepository
{
    public Task<Merchant?> GetByIdAsync(Guid merchantId, CancellationToken cancellationToken = default) =>
        dbContext.Merchants.AsTracking().SingleOrDefaultAsync(merchant => merchant.Id == merchantId, cancellationToken);

    public void Add(Merchant merchant) => dbContext.Merchants.Add(merchant);
}
