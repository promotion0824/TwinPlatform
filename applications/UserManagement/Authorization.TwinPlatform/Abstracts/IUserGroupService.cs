using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface to manager User Groups
/// </summary>
public interface IUserGroupService
{

    /// <summary>
    /// Get all junction table records for User and Group.
    /// </summary>
    /// <typeparam name="T">Type of Response Model.</typeparam>
    /// <returns>List of records.</returns>
    public Task<List<T>> GetAll<T>();

    /// <summary>
    ///  Add User Group entity to the database
    /// </summary>
    /// <param name="model">UserGroupModel to add</param>
    /// <returns>Task that can be awaited</returns>
    Task AddAsync(GroupUserModel model);

	/// <summary>
	/// Removes User Group entity from the database
	/// </summary>
	/// <param name="model">UserGroupModel to remove</param>
	/// <returns>Task that can be awaited</returns>
	Task RemoveAsync(GroupUserModel model);

    /// <summary>
    /// Import user-group records from file data model
    /// </summary>
    /// <param name="userRecordToImport">GroupUser Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    Task<IEnumerable<GroupUserFileModel>> ImportAsync(IEnumerable<GroupUserFileModel> groupUserRecords);
}
