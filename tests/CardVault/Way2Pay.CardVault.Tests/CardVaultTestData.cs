using Way2Pay.CardVault.CardRegistrationSessions;
using Way2Pay.CardVault.Cards;

namespace Way2Pay.CardVault.Tests;

internal static class CardVaultTestData
{
    public static readonly DateTimeOffset Now = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
    public const string TokenHash = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";
    public const string Ciphertext = "vault:v1:test-ciphertext";

    public static CardRegistrationSession CreateSession(string? customerId = null) =>
        new(Guid.NewGuid(), TokenHash, Now, Now.AddMinutes(15), customerId);

    public static StoredCard CreateCard(Guid merchantId, string? customerId = null) =>
        new(merchantId, Ciphertext, "1111", new CardExpiration(12, 2027), customerId);
}
