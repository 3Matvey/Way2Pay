using Way2Pay.CardVault.Authentication;
using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Cards.RegisterCard;
using Way2Pay.CardVault.Errors;
using Way2Pay.CardVault.Persistence;
using Way2Pay.CardVault.Vault;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddVaultInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("cardvault-db")
    ?? throw new InvalidOperationException("The 'cardvault-db' connection string is required.");

builder.Services.AddCardVaultPersistence(connectionString);
builder.Services.AddCardRegistrationSessions(builder.Configuration);
builder.Services.AddCardRegistrationAuthentication();
builder.Services.AddScoped<RegisterCardHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// Route malformed request bodies through the same safe error handler in every environment.
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services.AddOpenApi("public-v1", options =>
{
    options.ShouldInclude = description => description.GroupName == "public-v1";
    options.AddOperationTransformer<CardRegistrationOpenApiTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Way2Pay Cards API";
        document.Info.Version = "v1";
        document.Servers = [new() { Url = "/" }];
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapRegisterCard();

app.Run();
