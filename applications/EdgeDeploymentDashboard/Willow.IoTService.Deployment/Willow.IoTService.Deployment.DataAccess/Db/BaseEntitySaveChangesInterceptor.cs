using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Willow.IoTService.Deployment.DataAccess.Entities;
using Willow.IoTService.Deployment.DataAccess.PortService;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class BaseEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IUserInfoService _userInfoService;

    public BaseEntitySaveChangesInterceptor(IUserInfoService userInfoService)
    {
        _userInfoService = userInfoService;
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

        return base.SavingChangesAsync(eventData,
                                       result,
                                       cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                var now = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = _userInfoService.GetUserName();
                entry.Entity.CreatedOn = now;
                entry.Entity.UpdatedBy = _userInfoService.GetUserName();
                entry.Entity.UpdatedOn = now;
            }

            if (entry.State is EntityState.Modified || HasChangedOwnedEntities(entry))
            {
                entry.Entity.UpdatedBy = _userInfoService.GetUserName();
                entry.Entity.UpdatedOn = DateTimeOffset.UtcNow;
            }
        }
    }

    private static bool HasChangedOwnedEntities(EntityEntry entry)
    {
        return entry.References.Any(r => r.TargetEntry != null
                                         && r.TargetEntry.Metadata.IsOwned()
                                         && r.TargetEntry.State is EntityState.Added or EntityState.Modified);
    }
}
