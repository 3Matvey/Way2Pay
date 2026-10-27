using Way2Pay.AppHost.Infrastructure.Vault;
using Scalar.Aspire;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql", port: 14333)
    .WithImageTag("2025-latest")
    .WithDataVolume("way2pay-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var paymentsDatabase = sql.AddDatabase("payments-db", "Way2Pay.Payments");
var cardVaultDatabase = sql.AddDatabase("cardvault-db", "Way2Pay.CardVault");

var vault = builder.AddVault();

var paymentsApi = builder.AddProject<Projects.Way2Pay_Payments_Api>("way2pay-payments-api")
    .WithReference(paymentsDatabase)
    .WaitFor(paymentsDatabase);

var cardVault = builder.AddProject<Projects.Way2Pay_CardVault>("way2pay-cardvault")
    .WithReference(cardVaultDatabase)
    .WaitFor(cardVaultDatabase)
    .WithEnvironment("Vault__Address", vault.GetEndpoint("http"))
    .WaitFor(vault);

builder.AddProject<Projects.Way2Pay_Gateway>("way2pay-gateway")
    .WithReference(paymentsApi)
    .WithReference(cardVault)
    .WaitFor(paymentsApi)
    .WaitFor(cardVault);

var scalar = builder.AddScalarApiReference(options =>
{
    options.PreferHttpsEndpoint();

    if (builder.Environment.IsDevelopment())
        options.AllowSelfSignedCertificates();
});

scalar
    .WithApiReference(paymentsApi, configureOptions: options =>
    {
        options.AddDocument("public-v1", "Payments");
    })
    .WithApiReference(cardVault, configureOptions: options =>
    {
        options.AddDocument("public-v1", "Cards");
    });

builder.Build().Run();
