using Microsoft.Extensions.DependencyInjection;
using Way2Pay.Merchants.Application.ApiKeys;
using Way2Pay.Merchants.Infrastructure.Persistence;
using Way2Pay.Merchants.Infrastructure.Security;

namespace Way2Pay.Merchants.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence and the stateless API key secret provider.</summary>
    public static IServiceCollection AddMerchantsInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddMerchantsPersistenceInfrastructure(connectionString);
        services.AddSingleton<IApiKeySecretProvider, ApiKeySecretProvider>();

        return services;
    }
}
