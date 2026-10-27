namespace Way2Pay.Merchants.Application.Common;

public interface IUnitOfWork
{
    /// <summary>Atomically saves all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
