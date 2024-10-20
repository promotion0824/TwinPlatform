using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface for managing Group Role Assignment Entity
/// </summary>
public interface IGroupRoleAssignmentService: IBatchRequestEntityService<GroupRoleAssignment, GroupRoleAssignmentModel>
{
    /// <summary>
    /// Get group role assignment record by Id.
    /// </summary>
    /// <param name="id">Record Id.</param>
    /// <returns>Instance of Group Role Assignment Model.</returns>
    Task<GroupRoleAssignmentModel?> GetAssignmentByIdAsync(Guid id);

    /// <summary>
    /// Method to retrieve list of all group assignments
    /// </summary>
    /// <typeparam name="T">Type of IGroupRoleAssignment</typeparam>
    /// <returns>List of T</returns>
    public Task<List<T>> GetAssignmentsAsync<T>() where T : IGroupRoleAssignment;

	/// <summary>
	/// Get GroupRoleAssignment Entity by Id
	/// </summary>
	/// <param name="groupId"></param>
	/// <returns>List of GroupRoleAssignmentModel</returns>
	Task<List<GroupRoleAssignmentModel>> GetAssignmentsByGroupAsync(Guid groupId);

	/// <summary>
	/// Add GroupRoleAssignment record to the database
	/// </summary>
	/// <param name="model">GroupRoleAssignmentModel to add</param>
	/// <returns>Task that can be awaited</returns>
	Task<Guid> AddAsync(GroupRoleAssignmentModel model);

	/// <summary>
	/// Updates Group Role Assignment Model in the database
	/// </summary>
	/// <param name="model">GroupRoleAssignment Model to update</param>
	/// <returns>Task</returns>
	Task UpdateAsync(GroupRoleAssignmentModel model);

	/// <summary>
	/// Removes GroupRoleAssignment record from database
	/// </summary>
	/// <param name="IdToRemove">Id of the GroupRoleAssignmentModel to remove</param>
	/// <returns>Task that can be awaited</returns>
	Task RemoveAsync(Guid IdToRemove);

	/// <summary>
	/// Import assignments records from file data model
	/// </summary>
	/// <param name="roleAssignmentsToImport">Group Role Assignments File models to import</param>
	/// <returns>List of Import Errors></returns>
	public Task<IEnumerable<GroupRoleAssignmentFileModel>> ImportAsync(IEnumerable<GroupRoleAssignmentFileModel> roleAssignmentsToImport);
}
