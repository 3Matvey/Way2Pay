using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.MapReverseProxy();
app.MapScalarApiReference(options => options
    .WithTitle("Way2Pay API")
    .AddDocument("payments", "Payments", "/openapi/payments.json", isDefault: true)
    .AddDocument("cards", "Cards", "/openapi/cards.json")
    .DisableAgent());

app.Run();
