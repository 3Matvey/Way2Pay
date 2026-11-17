using Way2Pay.Payments.Api.Authentication;
using Way2Pay.Payments.Api.Endpoints;
using Way2Pay.Payments.Application.Payments;
using Way2Pay.Payments.Application.Payments.CreatePayment;
using Way2Pay.Payments.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GatewayMerchantAccess>();
builder.Services.AddScoped<IMerchantPaymentAccess>(services => services.GetRequiredService<GatewayMerchantAccess>());
builder.Services.AddScoped<CreatePaymentUseCase>();

builder.Services.AddOpenApi("public-v1", options =>
{
    options.ShouldInclude = description => description.GroupName == "public-v1";
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Way2Pay Payments API";
        document.Info.Version = "v1";
        document.Servers = [new() { Url = "/" }];
        return Task.CompletedTask;
    });
});

var connectionString = builder.Configuration.GetConnectionString("payments-db")
    ?? throw new InvalidOperationException("The 'payments-db' connection string is required.");
builder.Services.AddPaymentsPersistenceInfrastructure(connectionString);

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapCreatePayment();

app.UseHttpsRedirection();

app.Run();
