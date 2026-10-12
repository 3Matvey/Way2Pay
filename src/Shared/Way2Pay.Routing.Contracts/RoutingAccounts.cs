namespace Way2Pay.Routing.Contracts;

/// <summary>Copies account identifiers without changing routing priority.</summary>
internal static class RoutingAccounts
{
    internal static IReadOnlyList<Guid> Copy(IEnumerable<Guid> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);
        var snapshot = accountIds.ToArray();
        if (snapshot.Any(id => id == Guid.Empty) || snapshot.Distinct().Count() != snapshot.Length)
            throw new ArgumentException("Account identifiers must be non-empty and unique.", nameof(accountIds));
        return Array.AsReadOnly(snapshot);
    }
}
