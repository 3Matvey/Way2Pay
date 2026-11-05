using Scalar.AspNetCore;
using Way2Pay.Gateway.Authentication;
using Way2Pay.Gateway.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMerchantApiKeyAuthentication();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(MerchantContextTransform.Configure);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.MapScalarApiReference(options => options
    .WithTitle("Way2Pay API")
    .AddDocument("payments", "Payments", "/openapi/payments.json", isDefault: true)
    .AddDocument("cards", "Cards", "/openapi/cards.json")
    .DisableAgent());

app.Run();
