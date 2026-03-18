using ContactsManager.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ContactsManager.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var utcNow = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAtUtc = utcNow;
                }

                entry.Entity.LastModifiedUtc = utcNow;
            }
        }
    }
}
