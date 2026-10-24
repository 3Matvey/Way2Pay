using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Way2Pay.CardVault.Persistence;

internal static class DependencyInjection
{
    /// <summary>Registers SQL Server persistence and audit timestamps without applying migrations.</summary>
    internal static IServiceCollection AddCardVaultPersistence(
        this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<CardVaultDbContext>((provider, options) =>
            options.UseSqlServer(connectionString)
                .AddInterceptors(provider.GetRequiredService<AuditInterceptor>()));
        return services;
    }
}
