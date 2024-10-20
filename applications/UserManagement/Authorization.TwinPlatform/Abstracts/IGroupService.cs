using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Service Contract for managing Group Entity
/// </summary>
public interface IGroupService : IBatchRequestEntityService<Group, GroupModel>
{
    /// <summary>
    /// Method to get list of groups
    /// </summary>
    /// <param name="filterModel">Filter Property Model</param>
    /// <returns>List of IGroup type models.</returns>
    Task<List<T>> GetListAsync<T>(FilterPropertyModel filterModel) where T : IGroup;

    /// <summary>
    /// Method to get Group Entity by Id
    /// </summary>
    /// <param name="id">Id of the Group</param>
    /// <returns>Task that can be awaited to get GroupModel</returns>
     Task<GroupModel?> GetAsync(Guid id);

	/// <summary>
	/// Method to get Group Entity by Name
	/// </summary>
	/// <param name="name">Name of the Group Entity</param>
	/// <returns>GroupModel</returns>
	Task<GroupModel?> GetByNameAsync(string name);

	/// <summary>
	/// Adds Group Entity to the Database
	/// </summary>
	/// <param name="model">Group Model to add</param>
	/// <returns>ID of the added record</returns>
	Task<Guid> AddAsync(GroupModel model);

	/// <summary>
	/// Delete Group Entity from the database
	/// </summary>
	/// <param name="idToDelete">Id of the group to delete</param>
	/// <returns>Id of the deleted group</returns>
	Task<Guid> DeleteAsync(Guid idToDelete);

	/// <summary>
	/// Method to update group entity
	/// </summary>
	/// <param name="model">Group Model to update</param>
	/// <returns>Id of the update entity</returns>
	Task<Guid> UpdateAsync(GroupModel model);

    /// <summary>
    /// Import group records from file data model
    /// </summary>
    /// <param name="groupRecordsToImport">Group Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    Task<IEnumerable<GroupFileModel>> ImportAsync(IEnumerable<GroupFileModel> groupRecordsToImport);
}
