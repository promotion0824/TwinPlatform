using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Class to manage Role entity records
/// </summary>
public class RoleManager(IRoleService roleService,
    IRolePermissionService rolePermissionService,
    ILogger<RoleManager> logger,
    IUserAuthorizationManager userAuthorization,
    IAuditLogger<RoleManager> auditLogger) : BaseManager, IRoleManager
{
    /// <summary>
    /// Get count of role entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetCountAsync()
    {
        return await roleService.GetCountAsync();
    }

    /// <summary>
    /// Method to add role to the collection
    /// </summary>
    /// <param name="model">RoleModel defining the Role properties</param>
    /// <returns>RoleModel as response</returns>
    public async Task<RoleModel> AddRoleAsync(RoleModel model)
    {
        logger.LogInformation("Creating role with name: {name}.", model.Name);
        model.Id = await roleService.AddAsync(model);
        logger.LogInformation("Role {name} created successfully with an Id: {Id}", model.Name, model.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleModel).Name, RecordAction.Create, model.Name));
        return model;
    }

    /// <summary>
    /// Method to retrieve role by Id
    /// </summary>
    /// <param name="roleId">Id of the role</param>
    /// <returns>RoleModel as response</returns>
    public async Task<RoleModelWithPermissions?> GetRoleByIdAsync(Guid roleId)
    {
        logger.LogTrace("Finding role by Id: {Id}.", roleId);
        return await roleService.GetAsync(roleId);
    }

    /// <summary>
    /// Method to get a role entity by Name
    /// </summary>
    /// <param name="name">Name of the role entity</param>
    /// <returns>RoleModel</returns>
    public Task<RoleModelWithPermissions?> GetByNameAsync(string name)
    {
        logger.LogTrace("Finding role by name: {name}.", name);
        return roleService.GetByNameAsync(name);
    }

    /// <summary>
    /// Get Roles Batch
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO instance.</param>
    /// <returns>BatchDTO of RoleModelWithPermissions.</returns>
    public Task<BatchDto<RoleModelWithPermissions>> GetRolesAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Getting all roles by batch: {batch}.", JsonSerializer.Serialize(batchRequest));
        return roleService.GetBatchAsync(batchRequest, null, true);
    }

    /// <summary>
    /// Method to Delete Role Record
    /// </summary>
    /// <param name="idToDelete">Id of the Role to Delete</param>
    /// <returns>True if success or false if not</returns>
    public async Task<bool> DeleteRoleAsync(Guid idToDelete)
    {
        var existingModel = await roleService.GetAsync(idToDelete);
        logger.LogInformation("Deleting role by Id: {Id}.", idToDelete);
        await roleService.DeleteAsync(idToDelete);
        logger.LogInformation("Role with id: {id} deleted successfully.", idToDelete);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleModel).Name, RecordAction.Delete, existingModel?.Name));
        return true;
    }

    /// <summary>
    /// Method to update Role Record
    /// </summary>
    /// <param name="roleToUpdate">Role Model to update</param>
    /// <returns>True if update is successful</returns>
    public async Task<bool?> UpdateRoleAsync(RoleModel roleToUpdate)
    {
        var oldModel = await roleService.GetAsync(roleToUpdate.Id);
        logger.LogInformation("Updating role by Id: {Id}.", roleToUpdate.Id);
        await roleService.UpdateAsync(roleToUpdate);
        logger.LogInformation("Update role by Id: {Id} completed.", roleToUpdate.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleModel).Name, RecordAction.Update, oldModel?.Name, AuditLog.Summarize(oldModel, roleToUpdate)));
        return true;
    }

    /// <summary>
    /// Method to assign a permission to a role record
    /// </summary>
    /// <param name="roleId">Id of the role</param>
    /// <param name="permission">Permission Model to assign</param>
    /// <returns>Task</returns>
    public async Task AssignPermission(Guid roleId, PermissionModel permission)
    {
        var roleModel = await roleService.GetAsync(roleId);
        logger.LogTrace("Adding permission {permissionId} to the role {roleId}.", permission.Id, roleId);
        await rolePermissionService.AddAsync(new RolePermissionModel() { RoleId = roleId, Permission = permission });
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleModel).Name, RecordAction.Assign, roleModel?.Name, permission.FullName));
    }

    /// <summary>
    /// Method to remove a permission from a role record
    /// </summary>
    /// <param name="roleId">Id of the role</param>
    /// <param name="permission">Permission model to remove</param>
    /// <returns>Task</returns>
    public async Task RemovePermission(Guid roleId, PermissionModel permission)
    {
        var roleModel = await roleService.GetAsync(roleId);
        logger.LogTrace("Removing permission {permissionId} to the role {roleId}.", permission.Id, roleId);
        await rolePermissionService.RemoveAsync(new RolePermissionModel() { RoleId = roleId, Permission = permission });
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleModel).Name, RecordAction.Remove, roleModel?.Name, permission.FullName));
    }
}
