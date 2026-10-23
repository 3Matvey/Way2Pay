namespace Way2Pay.CardVault.Vault;

/// <summary>Connection settings and the Transit key used to encrypt PAN.</summary>
public sealed class VaultOptions
{
    public string Address { get; init; } = string.Empty;

    /// <summary>A restricted application token supplied through secrets configuration, never a root token.</summary>
    public string Token { get; init; } = string.Empty;

    public string TransitMountPoint { get; init; } = "transit";
    public string PanKeyName { get; init; } = "card-pan";
}
