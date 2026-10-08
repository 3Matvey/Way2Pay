namespace Way2Pay.Payments.Domain.ValueObjects;

/// <summary>An immutable snapshot of a payment method and its provider-account bindings.</summary>
/// <remarks>
/// Bindings identify the accounts through which the method can be used.
/// Application checks account ownership and availability.
/// </remarks>
public sealed record PaymentMethod
{
    public PaymentMethodType Type { get; }
    /// <summary>Bindings ordered by account identifier for value equality.</summary>
    /// <remarks>This order does not define routing priority. Only one binding per account is allowed.</remarks>
    public IReadOnlyList<ProviderPaymentMethodReference> ProviderReferences { get; }

    /// <summary>Creates a snapshot from a non-empty collection of bindings with unique account identifiers.</summary>
    /// <remarks>
    /// The input collection is copied so subsequent changes cannot affect the snapshot.
    /// The copy is sorted by account so identical bindings supplied in different orders
    /// compare equally through SequenceEqual and produce the same hash code.
    /// </remarks>
    public PaymentMethod(PaymentMethodType type, IEnumerable<ProviderPaymentMethodReference> providerReferences)
    {
        ArgumentNullException.ThrowIfNull(providerReferences);
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));
        var references = providerReferences.ToArray();
        EnsureValidReferences(references);
        Array.Sort(references, (left, right) => left.ProviderAccountId.CompareTo(right.ProviderAccountId));
        Type = type;
        ProviderReferences = Array.AsReadOnly(references);
    }

    /// <summary>Compares the method type and binding values rather than collection references.</summary>
    public bool Equals(PaymentMethod? other) =>
        other is not null && Type == other.Type && ProviderReferences.SequenceEqual(other.ProviderReferences);

    /// <summary>Computes a hash code from the same values used for equality.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        foreach (var reference in ProviderReferences)
            hash.Add(reference);
        return hash.ToHashCode();
    }

    private static void EnsureValidReferences(ProviderPaymentMethodReference[] providerReferences)
    {
        if (providerReferences.Length == 0 || providerReferences.Any(reference => reference is null))
            throw new ArgumentException("At least one non-null provider reference is required.", nameof(providerReferences));
        if (providerReferences.Select(reference => reference.ProviderAccountId).Distinct().Count() != providerReferences.Length)
            throw new ArgumentException("Only one reference per provider account is allowed.", nameof(providerReferences));
    }
}
