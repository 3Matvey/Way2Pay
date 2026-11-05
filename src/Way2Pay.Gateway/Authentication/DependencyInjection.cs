using Microsoft.AspNetCore.Authentication;
using Way2Pay.Merchants.Contracts.Protos.Authentication.V1;

namespace Way2Pay.Gateway.Authentication;

internal static class DependencyInjection
{
    public static IServiceCollection AddMerchantApiKeyAuthentication(this IServiceCollection services)
    {
        AddMerchantsClient(services);
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, MerchantApiKeyHandler>(
            MerchantApiKeyAuthentication.Scheme, _ => { });
        services.AddAuthorizationBuilder()
            .AddPolicy(MerchantApiKeyAuthentication.Policy, policy => policy.AddAuthenticationSchemes(MerchantApiKeyAuthentication.Scheme)
                .RequireAuthenticatedUser().RequireClaim(MerchantApiKeyAuthentication.MerchantIdClaim));
        return services;
    }

    private static void AddMerchantsClient(IServiceCollection services)
    {
#pragma warning disable EXTEXP0001 // Use the gRPC deadline without inherited HTTP retries or timeouts.
        services.AddGrpcClient<MerchantAuthentication.MerchantAuthenticationClient>(options =>
        {
            options.Address = new Uri("https://way2pay-merchants-api");
        }).RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
    }
}
