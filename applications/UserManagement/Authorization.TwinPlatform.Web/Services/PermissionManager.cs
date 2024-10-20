using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using System.Linq.Expressions;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Class to manage Permission Entity Records
/// </summary>
public class PermissionManager(IPermissionService permissionService,
    IPermissionAggregatorService permissionAggregatorService,
    IAuthorizationGraphService authGraphService,
    ILogger<PermissionManager> logger,
    IUserAuthorizationManager userAuthorization,
    IAuditLogger<PermissionManager> auditLogger) : BaseManager, IPermissionManager
{
    readonly string[] RestrictedPermissions = [AppPermissions.CanManageAdGroup];

    /// <summary>
    /// Get count of permission entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetCountAsync()
    {
        var permissionSecurityFilter = await GetPermissionSecurityFilter(userAuthorization, RestrictedPermissions);
        return await permissionService.GetCountAsync(permissionSecurityFilter);
    }

    /// <summary>
    /// Method to add permission to the collection
    /// </summary>
    /// <param name="permission">PermissionModel to insert</param>
    /// <returns>PermissionModel as response</returns>
    public async Task<PermissionModel> AddPermissionAsync(PermissionModel permission)
    {
        logger.LogInformation("Request received to create permission {permissionName}", permission.Name);
        permission.Id = await permissionService.AddAsync(permission);
        logger.LogInformation("Created permission {permissionName} with Id: {Id}", permission.Name, permission.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(PermissionModel).Name, RecordAction.Create, permission.FullName));
        return permission;
    }

    /// <summary>
    /// Method to retrieve a Permission by Id
    /// </summary>
    /// <param name="permissionId">Id of the permission to retrieve</param>
    /// <returns>PermissionModel as response</returns>
    public Task<PermissionModel?> GetPermissionByIdAsync(Guid permissionId)
    {
        logger.LogTrace("Finding permission by Id: {Id}", permissionId);
        return permissionService.GetById(permissionId);
    }

    /// <summary>
    /// Get Permission Batch
    /// </summary>
    /// <param name="batchRequest">BatchRequest DTO.</param>
    /// <returns>BatchDto Of PermissionModel</returns>
    public async Task<BatchDto<PermissionModel>> GetPermissionsAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Get all permissions record with filter: {filter}", JsonSerializer.Serialize(batchRequest));
        var permissionSecurityFilter = await GetPermissionSecurityFilter(userAuthorization, RestrictedPermissions);
        return await permissionService.GetBatchAsync(batchRequest, permissionSecurityFilter, includeTotalCount: true);
    }

    /// <summary>
    /// Get batch of Permissions by Role.
    /// </summary>
    /// <param name="roleId">Role Id</param>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <param name="getOnlyNonMembers">Only Users who are not a member of the group will be returned</param>
    /// <returns>Batch DTO Of Permission Model.</returns>
    public async Task<BatchDto<PermissionModel>> GetPermissionsByRoleAsync(string roleId, BatchRequestDto batchRequest, bool getOnlyNonMembers = false)
    {
        logger.LogTrace("Getting Permissions by role:{roleId}.", roleId);

        var securityFilter = await GetPermissionSecurityFilter(userAuthorization, RestrictedPermissions);

        Expression<Func<Permission, bool>> systemFilter = getOnlyNonMembers ?
                x => x.RolePermission.All(a => a.RoleId.ToString() != roleId) :
                x => x.RolePermission.Any(a => a.RoleId.ToString() == roleId);

        var permissions = await permissionService.GetBatchAsync(batchRequest, securityFilter == null ? systemFilter : systemFilter.And(securityFilter), includeTotalCount: true);

        return permissions;
    }

    /// <summary>
    /// Method to Delete Permission Record
    /// </summary>
    /// <param name="idToDelete">Id of the Permission to Delete</param>
    /// <returns>True if success or false if not</returns>
    public async Task<bool> DeletePermissionAsync(Guid idToDelete)
    {
        var permission = await permissionService.GetById(idToDelete);
        logger.LogInformation("Deleting permission by Id: {Id}.", idToDelete);
        await permissionService.DeleteAsync(idToDelete);
        logger.LogInformation("Permission with id: {id} deleted successfully.", idToDelete);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(PermissionModel).Name, RecordAction.Delete, permission?.FullName));
        return true;
    }

    /// <summary>
    /// Method to update Permission Record
    /// </summary>
    /// <param name="permissionModel">Permission Model to update</param>
    /// <returns>True if update is successful</returns>
    public async Task<PermissionModel> UpdatePermissionAsync(PermissionModel permissionModel)
    {
        var exPermission = await permissionService.GetById(permissionModel.Id);
        logger.LogInformation("Updating permission by Id: {Id}.", permissionModel.Id);
        permissionModel = await permissionService.UpdateAsync(permissionModel);
        logger.LogInformation("Update permission by Id: {Id} completed successfully.", permissionModel.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(PermissionModel).Name, RecordAction.Update, exPermission?.FullName, AuditLog.Summarize(exPermission, permissionModel)));
        return permissionModel;
    }

    /// <summary>
    /// Get all permission assigned to a user identified by the email
    /// </summary>
    /// <param name="email">Email of the User</param>
    /// <returns>List of conditional permission model</returns>
    public async Task<IEnumerable<ConditionalPermissionModel>> GetPermissionsByUserEmail(string email)
    {
        logger.LogTrace("Finding permissions by email: {email}", email);
        var originalResponse = await permissionAggregatorService.GetUserPermissions(email);

        var securedResponse = await ApplySecurityFilter(originalResponse.ToArray());
        return securedResponse;
    }

    /// <summary>
    /// Get all permission inherited by a user based on AD Group membership
    /// </summary>
    /// <param name="email">Email of the User</param>
    /// <returns>List of conditional permission model</returns>
    public async Task<IEnumerable<ConditionalPermissionModel>> GetPermissionBasedOnADGroupMembership(string email)
    {
        logger.LogTrace("Getting all AD based user permission by email: {mail}", email);
        var response = await authGraphService.GetAllPermissionByEmail(email);
        return response;
    }

    private async Task<IEnumerable<ConditionalPermissionModel>> ApplySecurityFilter(params ConditionalPermissionModel[] permissionModels)
    {
        var isAdminUser = await userAuthorization.IsCurrentUserAnAdminAsync();
        if (!isAdminUser)
        {
            return permissionModels.Where(w => !(RestrictedPermissions.Contains(w.Permission.Name) && AppPermissions.ExtensionName == w.Permission.Application.Name));
        }
        return permissionModels.AsEnumerable();
    }
}
