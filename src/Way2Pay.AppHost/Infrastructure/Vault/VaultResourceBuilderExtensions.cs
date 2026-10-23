namespace Way2Pay.AppHost.Infrastructure.Vault;

internal static class VaultResourceBuilderExtensions
{
    /// <summary>Adds a persistent, single-node Vault server for local development over HTTP.</summary>
    /// <remarks>
    /// Initialization, unsealing and policy provisioning are performed separately.
    /// The health check succeeds only after Vault has been initialized and unsealed.
    /// </remarks>
    internal static IResourceBuilder<ContainerResource> AddVault(this IDistributedApplicationBuilder builder)
    {
        var configurationPath = Path.Combine(builder.AppHostDirectory, "Infrastructure", "Vault");
        return builder.AddContainer("vault", "hashicorp/vault", "2.1.1")
            .WithArgs("server")
            .WithBindMount(configurationPath, "/vault/config", isReadOnly: true)
            .WithVolume("way2pay-vault-data", "/vault/file")
            .WithEnvironment("VAULT_ADDR", "http://127.0.0.1:8200")
            .WithHttpEndpoint(port: 8200, targetPort: 8200, name: "http", isProxied: false)
            .WithHttpHealthCheck("/v1/sys/health")
            .WithLifetime(ContainerLifetime.Persistent);
    }
}
