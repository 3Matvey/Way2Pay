using Scalar.AspNetCore;
using Way2Pay.Merchants.Application;
using Way2Pay.Merchants.Infrastructure;
using Way2Pay.Merchants.Api.Endpoints;
using Way2Pay.Merchants.Api.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi(MerchantAdminEndpoints.DocumentName, options =>
{
    options.ShouldInclude = description => description.GroupName == MerchantAdminEndpoints.DocumentName;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Way2Pay Merchant Administration API";
        document.Info.Version = "v1";
        document.Servers = [new() { Url = "/" }];
        return Task.CompletedTask;
    });
});
builder.Services.AddGrpc();
builder.Services.AddMerchantsApplication();

var connectionString = builder.Configuration.GetConnectionString("merchants-db")
    ?? throw new InvalidOperationException("The 'merchants-db' connection string is required.");
builder.Services.AddMerchantsInfrastructure(connectionString);

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<MerchantAuthenticationService>();
app.MapOpenApi();
app.MapScalarApiReference(options => options
    .WithTitle("Way2Pay Merchant Administration API")
    .AddDocument(MerchantAdminEndpoints.DocumentName, "Merchants Admin")
    .DisableAgent());

if (app.Environment.IsDevelopment())
{
    app.MapMerchantAdminEndpoints();
}

app.UseHttpsRedirection();

app.Run();
