namespace Way2Pay.CardVault.CardRegistrationSessions.CreateCardRegistrationSession;

/// <summary>Returns the persisted registration reference and its secret bearer token.</summary>
/// <remarks>Contains a credential and must not be logged, traced or cached as an HTTP response.</remarks>
public sealed class CreateCardRegistrationSessionResponse(Guid sessionId, string registrationToken, DateTimeOffset expiresAt)
{
    public Guid SessionId { get; } = sessionId;
    public string RegistrationToken { get; } = registrationToken;
    public DateTimeOffset ExpiresAt { get; } = expiresAt;
}
