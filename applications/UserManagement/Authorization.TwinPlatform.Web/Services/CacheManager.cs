using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Common.Abstracts;
using SDK = Authorization.TwinPlatform.Common.Model;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Cache Manager.
/// </summary>
public class CacheManager(IUserAuthorizationService userAuthorizationService, ILogger<CacheManager> logger) : IRecordChangeListener
{
    /// <summary>
    /// Notify the record changes.
    /// </summary>
    /// <param name="targetRecord">Type of entity that was changed.</param>
    /// <param name="recordAction">Kind of action on the entity.</param>
    /// <returns>Awaitable task.</returns>
    public async Task Notify(object targetRecord, RecordAction recordAction)
    {
        try
        {
            if ((targetRecord is RoleAssignmentModel &&
                (recordAction == RecordAction.Create || recordAction == RecordAction.Update || recordAction == RecordAction.Delete))
                ||
                (targetRecord is GroupUserModel && (recordAction == RecordAction.Assign || recordAction == RecordAction.Remove))
                ||
                (targetRecord is GroupModel && (recordAction == RecordAction.Delete))
                ||
                (targetRecord is RolePermissionModel && (recordAction == RecordAction.Assign || recordAction == RecordAction.Remove))
                ||
                (targetRecord is RoleModel && (recordAction == RecordAction.Delete))
                )
            {
                await userAuthorizationService.InvalidateCache([SDK.CacheStoreType.PermissionByUMAssignment, SDK.CacheStoreType.PermissionByADGroup]);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling authorization api to invalidate the cache.");
        }
    }
}
