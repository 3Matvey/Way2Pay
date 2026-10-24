using Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

namespace Way2Pay.CardVault.CardRegistrationSessions;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCardRegistrationSessions(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CardRegistrationSessionOptions>()
            .Bind(configuration.GetSection("CardRegistrationSessions"))
            .Validate(options => options.Lifetime > TimeSpan.Zero, "Registration session lifetime must be positive.")
            .ValidateOnStart();
        services.AddScoped<CreateCardRegistrationSessionHandler>();

        return services;
    }
}
