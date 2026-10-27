using Microsoft.AspNetCore.Authentication;

namespace Way2Pay.CardVault.Authentication;

internal static class DependencyInjection
{
    internal static IServiceCollection AddCardRegistrationAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, CardRegistrationAuthenticationHandler>(
                CardRegistrationAuthentication.Scheme, _ => { });
        services.AddAuthorizationBuilder()
            .AddPolicy(CardRegistrationAuthentication.Policy, policy => policy
                .AddAuthenticationSchemes(CardRegistrationAuthentication.Scheme)
                .RequireAuthenticatedUser());
        return services;
    }
}
