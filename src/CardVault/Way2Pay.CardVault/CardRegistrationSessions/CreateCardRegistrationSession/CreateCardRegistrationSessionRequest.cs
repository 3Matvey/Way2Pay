namespace Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

/// <summary>Requests a registration permission; the merchant is supplied by the authenticated integration context.</summary>
public sealed class CreateCardRegistrationSessionRequest
{
    /// <summary>An optional, non-blank customer reference of at most 450 characters.</summary>
    public string? ExternalCustomerId { get; init; }
}
