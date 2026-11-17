using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Way2Pay.Payments.Application.Common;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Infrastructure.Persistence.Repositories;

namespace Way2Pay.Payments.Infrastructure.Persistence;

public static class DependencyInjection
{
    /// <summary>Registers SQL Server persistence, auditing and a shared scoped unit of work.</summary>
    public static IServiceCollection AddPaymentsPersistenceInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<PaymentsDbContext>((provider, options) =>
            options.UseSqlServer(connectionString)
                .AddInterceptors(provider.GetRequiredService<AuditInterceptor>()));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentsDbContext>());
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentCreationStore, PaymentCreationStore>();
        return services;
    }
}
