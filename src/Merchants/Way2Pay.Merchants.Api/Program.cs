using Way2Pay.Merchants.Application;
using Way2Pay.Merchants.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddMerchantsApplication();

var connectionString = builder.Configuration.GetConnectionString("merchants-db")
    ?? throw new InvalidOperationException("The 'merchants-db' connection string is required.");
builder.Services.AddMerchantsInfrastructure(connectionString);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
