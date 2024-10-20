using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface for managing RoleAssignment Entity
/// </summary>
public interface IRoleAssignmentService: IBatchRequestEntityService<RoleAssignment, UserRoleAssignmentModel>
{
    /// <summary>
    /// Retrieve user role assignment record by Id.
    /// </summary>
    /// <param name="userRoleAssignmentId">Id of the record.</param>
    /// <returns>Instance of User Role Assignment Model.</returns>
    Task<UserRoleAssignmentModel?> GetAssignmentByIdAsync(Guid userRoleAssignmentId);

	/// <summary>
	/// Method to retrieve list of all user assignments
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns>List of T</returns>
	Task<List<T>> GetAssignmentsAsync<T>() where T : IUserRoleAssignment;

	/// <summary>
	/// Method to get the list of Assignments for a User
	/// </summary>
	/// <param name="userId">User Id of the User record</param>
	/// <returns>List of Role Assignment Model</returns>
	Task<List<UserRoleAssignmentModel>> GetAssignmentsByUserAsync(Guid userId);

	/// <summary>
	/// Adds Role Assignment Model to the database
	/// </summary>
	/// <param name="model">RoleAssignment Model to add</param>
	/// <returns>Task that can be awaited</returns>
	Task<Guid> AddAsync(UserRoleAssignmentModel model);

	/// <summary>
	/// Updates Role Assignment Model in the database
	/// </summary>
	/// <param name="model">RoleAssignment Model to update</param>
	/// <returns>Task</returns>
	Task UpdateAsync(UserRoleAssignmentModel model);

	/// <summary>
	/// Removes Role Assignment Model from the database
	/// </summary>
	/// <param name="IdToRemove">Id of the RoleAssignment Model to remove</param>
	/// <returns>Task that can be awaited</returns>
	Task RemoveAsync(Guid IdToRemove);

    /// <summary>
    /// Import assignments records from file data model
    /// </summary>
    /// <param name="roleAssignmentsToImport">Role Assignments File models to import</param>
    /// <returns>List of Import Errors</returns>
    public Task<IEnumerable<UserRoleAssignmentFileModel>> ImportAsync(IEnumerable<UserRoleAssignmentFileModel> roleAssignmentsToImport);
}
