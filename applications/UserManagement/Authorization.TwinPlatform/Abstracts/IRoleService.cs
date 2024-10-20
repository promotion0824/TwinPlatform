using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Contract for Role Entity Management
/// </summary>
public interface IRoleService : IBatchRequestEntityService<Role, RoleModelWithPermissions>
{
	/// <summary>
	/// Method to retrieve list of role entity
	/// </summary>
	/// <param name="filterPropertyModel">Filter Property Model</param>
    /// <param name="includePermissions">Include Permissions</param>
	/// <returns>List of Role Model</returns>
	Task<List<RoleModelWithPermissions>> GetListAsync(FilterPropertyModel filterPropertyModel, bool includePermissions);

	/// <summary>
	/// Method to get a role entity by Id
	/// </summary>
	/// <param name="id">Id of the role entity</param>
	/// <returns>RoleModel</returns>
	Task<RoleModelWithPermissions?> GetAsync(Guid id);

	/// <summary>
	/// Method to get a role entity by Name
	/// </summary>
	/// <param name="name">Name of the role entity</param>
	/// <returns>RoleModel</returns>
	Task<RoleModelWithPermissions?> GetByNameAsync(string name);

	/// <summary>
	/// Method to add a role entity
	/// </summary>
	/// <param name="model">Role model to add</param>
	/// <returns>Id of the inserted role entity</returns>
	Task<Guid> AddAsync(RoleModel model);

	/// <summary>
	/// Method to update role entity
	/// </summary>
	/// <param name="model">RoleModel to update</param>
	/// <returns>Id of the updated Role Entity</returns>
	Task<Guid> UpdateAsync(RoleModel model);

	/// <summary>
	/// Method to Delete Role
	/// </summary>
	/// <param name="Id">Id of the Role to delete</param>
	/// <returns>Delete task</returns>
	Task DeleteAsync(Guid Id);

}
