namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Diagnostics;

internal class BaseEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IUserInfoService userInfoService;

    public BaseEntitySaveChangesInterceptor(IUserInfoService userInfoService)
    {
        this.userInfoService = userInfoService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
                                                                          InterceptionResult<int> result,
                                                                          CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry is { State: EntityState.Added, Entity: IAuditableEntity addEntity })
            {
                addEntity.CreatedDate = now;
            }

            if (entry is { State: EntityState.Modified, Entity: IAuditableEntity modifiedEntity })
            {
                modifiedEntity.LastUpdated = DateTimeOffset.UtcNow;
            }

            if (entry is { State: EntityState.Deleted, Entity: ISoftDeleteEntity softDeleteEntity })
            {
                softDeleteEntity.IsDeleted = true;
            }

            if (entry is { Entity: IAuditableEntity auditableEntity })
            {
                auditableEntity.LastUpdated = now;
            }
        }
    }

    private static bool HasChangedOwnedEntities(EntityEntry entry) => entry.References.Any(r => r.TargetEntry != null
                                                                                             && r.TargetEntry.Metadata.IsOwned()
                                                                                             && r.TargetEntry.State is EntityState.Added or EntityState.Modified);
}
