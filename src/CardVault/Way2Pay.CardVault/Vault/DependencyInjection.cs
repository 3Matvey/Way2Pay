using Microsoft.Extensions.Options;

namespace Way2Pay.CardVault.Vault;

internal static class DependencyInjection
{
    internal static IServiceCollection AddVaultInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<VaultOptions>()
            .Bind(configuration.GetSection("Vault"))
            .Validate(HasValidSettings, "Vault settings are invalid.")
            .ValidateOnStart();
#pragma warning disable EXTEXP0001 // disable inherited resilience for the Vault client
        services.AddHttpClient<ICardDataProtector, TransitCardDataProtector>(ConfigureHttpClient)
            .RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

        return services;
    }

    private static bool HasValidSettings(VaultOptions options) =>
        Uri.TryCreate(options.Address, UriKind.Absolute, out var address)
        && address.Scheme is "http" or "https"
        && !string.IsNullOrWhiteSpace(options.Token)
        && !string.IsNullOrWhiteSpace(options.TransitMountPoint)
        && !string.IsNullOrWhiteSpace(options.PanKeyName);

    private static void ConfigureHttpClient(IServiceProvider provider, HttpClient client)
    {
        var options = provider.GetRequiredService<IOptions<VaultOptions>>().Value;
        client.BaseAddress = new Uri(options.Address.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(10);
        client.DefaultRequestHeaders.Add("X-Vault-Token", options.Token);
    }
}
