using Authorization.Common.Models;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage Role entity records
/// </summary>
public interface IRoleManager
{
    /// <summary>
    /// Get count of role entity.
    /// </summary>
    /// <returns></returns>
    public Task<int> GetCountAsync();

    /// <summary>
    /// Get Roles Batch
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO instance.</param>
    /// <returns>BatchDTO of RoleModelWithPermissions.</returns>
    public Task<BatchDto<RoleModelWithPermissions>> GetRolesAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Method to retrieve role by Id
    /// </summary>
    /// <param name="roleId">Id of the role</param>
    /// <returns>RoleModel as response</returns>
    public Task<RoleModelWithPermissions?> GetRoleByIdAsync(Guid roleId);

	/// <summary>
	/// Method to get a role entity by Name
	/// </summary>
	/// <param name="name">Name of the role entity</param>
	/// <returns>RoleModel</returns>
	Task<RoleModelWithPermissions?> GetByNameAsync(string name);

	/// <summary>
	/// Method to add role to the collection
	/// </summary>
	/// <param name="model">RoleModel defining the Role properties</param>
	/// <returns>RoleModel as response</returns>
	public Task<RoleModel> AddRoleAsync(RoleModel model);


	/// <summary>
	/// Method to Delete Role Record
	/// </summary>
	/// <param name="idToDelete">Id of the Role to Delete</param>
	/// <returns>True if success or false if not</returns>
	public Task<bool> DeleteRoleAsync(Guid idToDelete);

	/// <summary>
	/// Method to update Role Record
	/// </summary>
	/// <param name="roleToUpdate">Role Model to update</param>
	/// <returns>True if update is successful</returns>
	public Task<bool?> UpdateRoleAsync(RoleModel roleToUpdate);

	/// <summary>
	/// Method to assign a permission to a role record
	/// </summary>
	/// <param name="roleId">Id of the role</param>
	/// <param name="permissionToAssign">Permission Model to assign</param>
	/// <returns>Task</returns>
	public Task AssignPermission(Guid roleId, PermissionModel permissionToAssign);

	/// <summary>
	/// Method to remove a permission from a role record
	/// </summary>
	/// <param name="roleId">Id of the role</param>
	/// <param name="permission">Permission model to remove</param>
	/// <returns>Task</returns>
	public Task RemovePermission(Guid roleId, PermissionModel permission);
}

