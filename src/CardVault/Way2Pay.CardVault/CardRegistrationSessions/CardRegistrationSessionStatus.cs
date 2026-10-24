namespace Way2Pay.CardVault.CardRegistrationSessions;

/// <summary>The registration outcome; expiration is evaluated separately using ExpiresAt.</summary>
public enum CardRegistrationSessionStatus
{
    Pending = 0,
    Completed = 1,
    Revoked = 2
}
