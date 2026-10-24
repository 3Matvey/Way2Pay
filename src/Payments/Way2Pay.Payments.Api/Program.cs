using Way2Pay.Payments.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

app.UseHttpsRedirection();

app.Run();
