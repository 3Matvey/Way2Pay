namespace Way2Pay.CardVault.CardRegistrationSessions;

/// <summary>Server-controlled registration lifetime, configured in the CardRegistrationSessions section.</summary>
public sealed class CardRegistrationSessionOptions
{
    public TimeSpan Lifetime { get; init; } = TimeSpan.FromMinutes(15);
}
