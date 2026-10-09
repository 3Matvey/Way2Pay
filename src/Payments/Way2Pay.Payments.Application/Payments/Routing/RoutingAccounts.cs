namespace Way2Pay.Payments.Application.Payments.Routing;

/// <summary>Copies account collections without changing their route order.</summary>
internal static class RoutingAccounts
{
    public static IReadOnlyList<Guid> Copy(IEnumerable<Guid> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);
        var snapshot = accountIds.ToArray();
        if (snapshot.Any(id => id == Guid.Empty) || snapshot.Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Account identifiers must be non-empty and unique.", nameof(accountIds));
        return Array.AsReadOnly(snapshot);
    }
}
