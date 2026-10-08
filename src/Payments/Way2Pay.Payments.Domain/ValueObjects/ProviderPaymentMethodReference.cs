using Way2Pay.Payments.Domain.Common;

namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>A stored payment-method reference scoped to a specific provider account.</summary>
/// <remarks>
/// A reference for one account is not automatically usable through another.
/// The payment-method preparation flow supplies any additional account bindings.
/// </remarks>
public sealed record ProviderPaymentMethodReference
{
    /// <summary>The provider account through which this reference can be used.</summary>
    public Guid ProviderAccountId { get; }

    /// <summary>An opaque value whose format is interpreted by the provider adapter.</summary>
    /// <remarks>Identifies a payment method rather than a processed transaction.</remarks>
    public string Reference { get; }

    public ProviderPaymentMethodReference(Guid providerAccountId, string reference)
    {
        ProviderAccountId = Guard.RequiredId(providerAccountId);
        Reference = Guard.Required(reference);
    }
}
