using Way2Pay.AppHost.Infrastructure.Vault;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql", port: 14333)
    .WithImageTag("2025-latest")
    .WithDataVolume("way2pay-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var paymentsDatabase = sql.AddDatabase("payments-db", "Way2Pay.Payments");

var vault = builder.AddVault();

builder.AddProject<Projects.Way2Pay_Payments_Api>("way2pay-payments-api")
    .WithReference(paymentsDatabase)
    .WaitFor(paymentsDatabase);

builder.AddProject<Projects.Way2Pay_CardVault>("way2pay-cardvault")
    .WithEnvironment("Vault__Address", vault.GetEndpoint("http"))
    .WaitFor(vault);

builder.Build().Run();
