using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Types;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using System.Linq.Expressions;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Base Implementation for all managers resource based auth checks.
/// </summary>
public abstract class BaseManager
{
    /// <summary>
    /// Get Group Security Filter
    /// </summary>
    /// <param name="authorizationManager">User Authorization Manager.</param>
    /// <param name="groupTypeManager">Group Type Manager.</param>
    /// <returns>Predicate Expression.</returns>
    public async Task<Expression<Func<Group, bool>>?> GetGroupSecurityFilter(IUserAuthorizationManager authorizationManager,
        IGroupTypeManager groupTypeManager,
        string requiredPermission)
    {
        var hasRequiredPermission = string.IsNullOrWhiteSpace(requiredPermission) || await authorizationManager.CheckCurrentUserHasPermission(requiredPermission);
        if (hasRequiredPermission)
        {
            return null;
        }

        var applicationGroupType = await groupTypeManager.GetGroupTypeByNameAsync(GroupTypeNames.Application.ToString());

        return x => (x.GroupTypeId == applicationGroupType!.Id);
    }

    /// <summary>
    /// Applies security filter to groups 
    /// </summary>
    /// <param name="authorizationManager">User Authorization Manager.</param>
    /// <param name="groupTypeManager">Group Type Manager.</param>
    /// <param name="groups">Array of groups to check for access.</param>
    /// <returns>Enumerable of groups that has access.</returns>
    public async Task<IEnumerable<GroupModel>> ApplySecurityFilter(IUserAuthorizationManager authorizationManager,
        IGroupTypeManager groupTypeManager,
        string requiredPermission,
        params GroupModel[] groups)
    {
        var hasRequiredPermission = string.IsNullOrWhiteSpace(requiredPermission) || await authorizationManager.CheckCurrentUserHasPermission(requiredPermission);
        var applicationGroupType = await groupTypeManager.GetGroupTypeByNameAsync(GroupTypeNames.Application.ToString());

        // apply filter
        var filteredGroups = groups.Where(x => (x.GroupTypeId == applicationGroupType!.Id || hasRequiredPermission));
        return filteredGroups;
    }

    public async Task<Expression<Func<Permission, bool>>?> GetPermissionSecurityFilter(IUserAuthorizationManager authorizationManager,
        string[] restrictedPermissions)
    {
        var isAdminUser = await authorizationManager.IsCurrentUserAnAdminAsync();

        return !isAdminUser ? w => !(restrictedPermissions.Contains(w.Name) && AppPermissions.ExtensionName == w.Application.Name)
        : null;
    }

    /// <summary>
    /// Get security filter to group assignments 
    /// </summary>
    /// <param name="authorizationManager">User Authorization Manager.</param>
    /// <param name="groupTypeManager">Group Type Manager.</param>
    /// <param name="groups">Array of group assignments to check for access.</param>
    /// <returns>Predicate for group role assignments that has access.</returns>
    public async Task<Expression<Func<GroupRoleAssignment, bool>>?> GetGroupRoleAssignmentSecurityFilter(IUserAuthorizationManager authorizationManager,
        IGroupTypeManager groupTypeManager,
        string requiredPermission)
    {
        var hasRequiredPermission = string.IsNullOrWhiteSpace(requiredPermission) || await authorizationManager.CheckCurrentUserHasPermission(requiredPermission);
        if (hasRequiredPermission)
        {
            return null;
        }

        var applicationGroupType = await groupTypeManager.GetGroupTypeByNameAsync(GroupTypeNames.Application.ToString());

        // apply filter
        return x => x.Group.GroupTypeId == applicationGroupType!.Id;
    }
}
