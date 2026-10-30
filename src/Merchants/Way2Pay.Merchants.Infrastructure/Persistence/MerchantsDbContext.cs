using Microsoft.EntityFrameworkCore;
using Way2Pay.Merchants.Application.Common;
using Way2Pay.Merchants.Domain.ApiKeys;
using Way2Pay.Merchants.Domain.Merchants;

namespace Way2Pay.Merchants.Infrastructure.Persistence;

internal sealed class MerchantsDbContext(DbContextOptions<MerchantsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<MerchantApiKey> MerchantApiKeys => Set<MerchantApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MerchantsDbContext).Assembly);
    }
}
