using Microsoft.EntityFrameworkCore;
using Way2Pay.Merchants.Application.ApiKeys;
using Way2Pay.Merchants.Domain.ApiKeys;

namespace Way2Pay.Merchants.Infrastructure.Persistence.Repositories;

internal sealed class MerchantApiKeyRepository(MerchantsDbContext dbContext) : IMerchantApiKeyRepository
{
    public Task<MerchantApiKey?> GetByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default) =>
        dbContext.MerchantApiKeys.AsTracking().SingleOrDefaultAsync(apiKey => apiKey.Id == apiKeyId, cancellationToken);

    public void Add(MerchantApiKey apiKey) => dbContext.MerchantApiKeys.Add(apiKey);
}
