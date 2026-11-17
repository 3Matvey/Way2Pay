using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Way2Pay.Payments.Infrastructure.Persistence;

namespace Way2Pay.Payments.IntegrationTests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();
    private ServiceProvider? _services;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _services = CreateServices();
        await using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<PaymentsDbContext>().Database.MigrateAsync();
    }

    internal AsyncServiceScope CreateScope() =>
        (_services ?? throw new InvalidOperationException("SQL Server has not been initialized.")).CreateAsyncScope();

    public async Task DisposeAsync()
    {
        try
        {
            if (_services is not null)
                await _services.DisposeAsync();
        }
        finally
        {
            await _container.DisposeAsync();
        }
    }

    private ServiceProvider CreateServices()
    {
        var connection = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "Way2Pay.Payments.IntegrationTests",
            MultipleActiveResultSets = false
        };
        return new ServiceCollection().AddPaymentsPersistenceInfrastructure(connection.ConnectionString)
            .BuildServiceProvider(validateScopes: true);
    }
}
