using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Way2Pay.CardVault.Persistence;

namespace Way2Pay.CardVault.IntegrationTests;

/// <summary>Owns an isolated SQL Server container and applies the real CardVault migrations once per test class.</summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    internal CardVaultDbContext CreateContext()
    {
        var connection = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = "Way2Pay.CardVault.IntegrationTests",
            MultipleActiveResultSets = false
        };
        var options = new DbContextOptionsBuilder<CardVaultDbContext>().UseSqlServer(connection.ConnectionString)
            .AddInterceptors(new AuditInterceptor(TimeProvider.System)).Options;
        return new CardVaultDbContext(options);
    }
}
