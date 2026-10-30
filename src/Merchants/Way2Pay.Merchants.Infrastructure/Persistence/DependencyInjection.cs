using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Way2Pay.Merchants.Application.ApiKeys;
using Way2Pay.Merchants.Application.Common;
using Way2Pay.Merchants.Application.Merchants;
using Way2Pay.Merchants.Infrastructure.Persistence.Repositories;

namespace Way2Pay.Merchants.Infrastructure.Persistence;

internal static class DependencyInjection
{
    /// <summary>Registers SQL Server storage, repositories, auditing and a shared scoped unit of work without applying migrations.</summary>
    internal static IServiceCollection AddMerchantsPersistenceInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<MerchantsDbContext>((provider, options) => options.UseSqlServer(connectionString)
            .AddInterceptors(provider.GetRequiredService<AuditInterceptor>()));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<MerchantsDbContext>());
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        services.AddScoped<IMerchantApiKeyRepository, MerchantApiKeyRepository>();
        return services;
    }
}
