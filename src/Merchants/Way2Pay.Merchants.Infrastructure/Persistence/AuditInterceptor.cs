using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Way2Pay.Common.DomainAbstractions;

namespace Way2Pay.Merchants.Infrastructure.Persistence;

/// <summary>Sets audit timestamps for tracked auditable entities when changes are saved.</summary>
internal sealed class AuditInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context is null)
            return;
        var now = timeProvider.GetUtcNow();
        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(entity => entity.CreatedAt).CurrentValue = now;
            if (entry.State == EntityState.Modified)
                entry.Property(entity => entity.UpdatedAt).CurrentValue = now;
        }
    }
}
