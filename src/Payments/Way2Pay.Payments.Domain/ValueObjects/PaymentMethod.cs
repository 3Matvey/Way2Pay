namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>An immutable snapshot of a payment method and its provider-account bindings.</summary>
/// <remarks>
/// Bindings identify the accounts through which the method can be used.
/// Application checks account ownership and availability.
/// </remarks>
public sealed record PaymentMethod
{
    private readonly List<ProviderPaymentMethodReference> _providerReferences = [];

    public PaymentMethodType Type { get; private init; }
    /// <summary>The provider-account bindings of this payment method.</summary>
    /// <remarks>Collection order affects neither value equality nor routing priority. Only one binding per account is allowed.</remarks>
    public IReadOnlyList<ProviderPaymentMethodReference> ProviderReferences => _providerReferences.AsReadOnly();

    /// <summary>Used by persistence to restore the method and populate its backing collection.</summary>
    private PaymentMethod()
    {
    }

    /// <summary>Creates a snapshot from a non-empty collection of bindings with unique account identifiers.</summary>
    /// <remarks>
    /// The input collection is copied so subsequent changes cannot affect the snapshot.
    /// Equality and hashing order bindings by account identifier, independently of their insertion or loading order.
    /// </remarks>
    public PaymentMethod(PaymentMethodType type, IEnumerable<ProviderPaymentMethodReference> providerReferences)
    {
        ArgumentNullException.ThrowIfNull(providerReferences);
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));
        var references = providerReferences.ToArray();
        EnsureValidReferences(references);
        Type = type;
        _providerReferences.AddRange(references);
    }

    /// <summary>Compares the method type and binding values rather than collection references.</summary>
    public bool Equals(PaymentMethod? other) =>
        other is not null && Type == other.Type && OrderedReferences().SequenceEqual(other.OrderedReferences());

    /// <summary>Computes a hash code from the same values used for equality.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        foreach (var reference in OrderedReferences())
            hash.Add(reference);
        return hash.ToHashCode();
    }

    /// <summary>Uses the same canonical order for equality and hashing regardless of persistence collection order.</summary>
    private IOrderedEnumerable<ProviderPaymentMethodReference> OrderedReferences() =>
        _providerReferences.OrderBy(reference => reference.ProviderAccountId);

    private static void EnsureValidReferences(ProviderPaymentMethodReference[] providerReferences)
    {
        if (providerReferences.Length == 0 || providerReferences.Any(reference => reference is null))
            throw new ArgumentException("At least one non-null provider reference is required.", nameof(providerReferences));
        if (providerReferences.Select(reference => reference.ProviderAccountId).Distinct().Count() != providerReferences.Length)
            throw new ArgumentException("Only one reference per provider account is allowed.", nameof(providerReferences));
    }
}
