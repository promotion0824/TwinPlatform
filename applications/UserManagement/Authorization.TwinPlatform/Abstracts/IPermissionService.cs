using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface for managing Permissions Entity
/// </summary>
public interface IPermissionService: IBatchRequestEntityService<Permission, PermissionModel>
{
    /// <summary>
    /// Get all permissions from database as a queryable
    /// </summary>
    /// <typeparam name="T">Type of response model</typeparam>
    /// <returns>IQueryable of type <typeparamref name="T"/>.</returns>
    public IQueryable<T> GetAll<T>() where T : IPermission;

    /// <summary>
    /// Method to get the all the permissions from database
    /// </summary>
    /// <param name="searchPropertyModel">Search Properties</param>
    /// <returns>List of all the Permissions</returns>
    Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IPermission;

	/// <summary>
	/// Set Id parameter for Permission model retrieved from database
	/// </summary>
	/// <param name="models">Collection of Permission Model to set Id</param>
	/// <returns>Task that can be awaited</returns>
	//Task SetIdsAsync(IEnumerable<PermissionModel> models);

	/// <summary>
	/// Method to Import Permissions in to the database
	/// </summary>
	/// <param name="importModel">ImportPermissionModel with list of permission to import</param>
	/// <returns>Task that can be awaited</returns>
	//Task ImportAsync(ImportPermissionModel importModel);

	/// <summary>
	/// Method to Add Permission record to database
	/// </summary>
	/// <param name="model">Permission Model Payload</param>
	/// <returns>Id of the Inserted Record</returns>
	Task<Guid> AddAsync(PermissionModel model);

	/// <summary>
	/// Method to retrieve Permission By Id
	/// </summary>
	/// <param name="Id">Id of the record to retrieve</param>
	/// <returns>Permission Model</returns>
	Task<PermissionModel?> GetById(Guid Id);

	/// <summary>
	/// Method to Update Permission Entity
	/// </summary>
	/// <param name="model">Model of the Permission entity</param>
	/// <returns>Updated permission model</returns>
	Task<PermissionModel> UpdateAsync(PermissionModel model);

	/// <summary>
	/// Method to Delete Permission
	/// </summary>
	/// <param name="Id">Id of the Permission to delete</param>
	/// <returns>Delete task</returns>
	Task DeleteAsync(Guid Id);

    /// <summary>
    /// Import permission records from file data model
    /// </summary>
    /// <param name="permissionRecordToImport">Permission Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    public Task<IEnumerable<PermissionFileModel>> ImportAsync(IEnumerable<PermissionFileModel> permissionRecordToImport);
}
