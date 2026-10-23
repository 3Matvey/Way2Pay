namespace Way2Pay.CardVault.Cards.RegisterCard;

/// <summary>Supplies card data for registration; merchant and customer references come from the trusted request context.</summary>
/// <remarks>
/// Contains plaintext PAN and must not be logged or included in tracing or error details.
/// JSON binding creates an immutable string that cannot be reliably cleared by the handler.
/// CVV belongs to a separate card-use session and is not accepted by this contract.
/// </remarks>
public sealed class RegisterCardRequest
{
    public string Pan { get; init; } = string.Empty;
    public int ExpirationMonth { get; init; }
    public int ExpirationYear { get; init; }
}
