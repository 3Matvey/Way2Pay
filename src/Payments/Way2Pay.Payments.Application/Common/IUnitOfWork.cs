namespace Way2Pay.Payments.Application.Common.Interfaces;

public interface IUnitOfWork
{
    /// <summary>Atomically saves all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back any failed transaction and discards all pending changes.
    /// Subsequent reads must see committed data without the discarded local changes.
    /// </summary>
    Task DiscardChangesAsync(CancellationToken cancellationToken = default);
}
