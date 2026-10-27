using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.Merchants.Domain.Merchants;

/// <summary>A merchant whose lifecycle is independent of its API keys and integration settings.</summary>
public sealed class Merchant : AuditableEntity
{
    public string Name
    {
        get;
        private set
        {
            var name = Guard.Required(value).Trim();
            if (name.Length > 200)
                throw new ArgumentException("Merchant name must contain at most 200 characters.", nameof(value));
            field = name;
        }
    } = string.Empty;

    public MerchantStatus Status { get; private set; }

    private Merchant() { }

    public Merchant(string name)
    {
        Name = name;
        Status = MerchantStatus.Active;
    }

    public void Rename(string name) => Name = name;

    /// <summary>Suspends external API access without revoking keys or stopping internal payment processing.</summary>
    public void Suspend() => Status = MerchantStatus.Suspended;

    /// <summary>Restores external API access; each key must still pass its own validity checks.</summary>
    public void Activate() => Status = MerchantStatus.Active;
}
