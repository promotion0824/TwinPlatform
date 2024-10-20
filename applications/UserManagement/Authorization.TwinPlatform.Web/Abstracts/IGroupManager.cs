using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Auth;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage Group Entity Records
/// </summary>
public interface IGroupManager
{
    /// <summary>
    /// Get count of group entity.
    /// </summary>
    /// <returns></returns>
    public Task<int> GetCountAsync();

    /// <summary>
    /// Get batch of all group records
    /// </summary>
    /// <param name="filterModel">Batch Request DTO.</param>
    /// <returns>Batch DTO of Group Models.</returns>
    public Task<BatchDto<GroupModel>> GetGroupsAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Method to get group by Id
    /// </summary>
    /// <param name="id">Id of the group</param>
    /// <returns>Secured Response of Group Model</returns>
    public Task<SecuredResult<GroupModel?>> GetGroupByIdAsync(Guid id);

    /// <summary>
    /// Method to get Group Record by Name
    /// </summary>
    /// <param name="name">Name of the Group Record</param>
    /// <returns>Secured GroupModel as response</returns>
    public Task<SecuredResult<GroupModel?>> GetGroupByNameAsync(string name);

    /// <summary>
    /// Method to add a Group to the collection
    /// </summary>
    /// <param name="group">GroupModel defining the group properties</param>
    /// <returns>Created groupModel object inside secured response.</returns>
    public Task<SecuredResult<GroupModel?>> AddGroupAsync(GroupModel group);

    /// <summary>
    /// Method to Delete Group Record
    /// </summary>
    /// <param name="idToDelete">Id of the group to Delete</param>
    /// <returns>Secured result of deleted group model.</returns>
    public Task<SecuredResult<GroupModel?>> DeleteGroupAsync(Guid idToDelete);

    /// <summary>
    /// Method to update Group Record
    /// </summary>
    /// <param name="groupToUpdate">Group Model to update</param>
    /// <returns>Secured result of updated group model.</returns>
    public Task<SecuredResult<GroupModel?>> UpdateGroupAsync(GroupModel groupToUpdate);

    /// <summary>
    /// Method to assign user to group record
    /// </summary>
    /// <param name="groupId">Id of the Group</param>
    /// <param name="userModel">UserModel</param>
    /// <returns>Task</returns>
    public Task AssignUserAsync(Guid groupId,UserModel userModel);

	/// <summary>
	/// Method to remove user from group
	/// </summary>
	/// <param name="groupId">Id of the Group</param>
	/// <param name="userId">Id of the User</param>
	/// <returns>Task</returns>
	public Task RemoveUserAsync(Guid groupId,Guid userId);
}

