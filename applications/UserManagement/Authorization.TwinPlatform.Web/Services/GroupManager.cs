using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Persistence.Types;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Class to manage Group Entity Records
/// </summary>
public class GroupManager(IGroupService groupService,
    IUserGroupService userGroupService,
    IUserService userService,
    ILogger<GroupManager> logger,
    IGroupTypeManager groupTypeManager,
    IUserAuthorizationManager authorizationManager,
    IAuditLogger<GroupManager> auditLogger) : BaseManager, IGroupManager
{
    /// <summary>
    /// Get count of group entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetCountAsync()
    {
        var groupSecurityFilter = await GetGroupSecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanViewAdGroup);
        return await groupService.GetCountAsync(groupSecurityFilter);
    }

    /// <summary>
    /// Method to add a Group to the collection
    /// </summary>
    /// <param name="group">GroupModel defining the group properties</param>
    /// <returns>Created groupModel object inside secured response.</returns>
    public async Task<SecuredResult<GroupModel?>> AddGroupAsync(GroupModel group)
    {
        logger.LogInformation("Creating group with name: {name}.", group.Name);

        // Apply Security Filter
        var filteredResults = await ApplySecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanManageAdGroup, group);

        // If filtered record returned nothing, return as failed Authorization.
        if (!filteredResults.Any())
        {
            return new SecuredResult<GroupModel?>(null, failedAuthorization: true);
        }

        group.Id = await groupService.AddAsync(group);
        logger.LogInformation("Group {name} created successfully with an Id: {Id}", group.Name, group.Id);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(typeof(GroupModel).Name, RecordAction.Create, group.Name));

        return new SecuredResult<GroupModel?>(group);
    }

    /// <summary>
    /// Method to get group by Id
    /// </summary>
    /// <param name="id">Id of the group</param>
    /// <returns>Secured Response of Group Model</returns>
    public async Task<SecuredResult<GroupModel?>> GetGroupByIdAsync(Guid id)
    {
        logger.LogTrace("Finding group by Id: {Id}.", id);
        var originalResponse = await groupService.GetAsync(id);

        // Check if the record exist; else return null for not found
        if (originalResponse is null)
        {
            return new SecuredResult<GroupModel?>(originalResponse);
        }

        var securedResponse = await ApplySecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanViewAdGroup, originalResponse);

        if(securedResponse is null)
        {
            return new SecuredResult<GroupModel?>(null,failedAuthorization:true);
        }
        return new SecuredResult<GroupModel?>(securedResponse.FirstOrDefault());
    }

    /// <summary>
    /// Method to get Group Record by Name
    /// </summary>
    /// <param name="name">Name of the Group Record</param>
    /// <returns>Secured GroupModel as response</returns>
    public async Task<SecuredResult<GroupModel?>> GetGroupByNameAsync(string name)
    {
        logger.LogTrace("Finding group by name: {name}.", name);
        var originalResponse = await groupService.GetByNameAsync(name);
        if (originalResponse is null)
            return new SecuredResult<GroupModel?>(originalResponse);

        var securedResponse = await ApplySecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanViewAdGroup, originalResponse);

        if (securedResponse is null)
        {
            return new SecuredResult<GroupModel?>(null, failedAuthorization: true);
        }
        return new SecuredResult<GroupModel?>(securedResponse.FirstOrDefault());
    }

    /// <summary>
    /// Get batch of all group records
    /// </summary>
    /// <param name="filterModel">Batch Request DTO.</param>
    /// <returns>Batch DTO of Group Models.</returns>
    public async Task<BatchDto<GroupModel>> GetGroupsAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Getting all groups by filter: {filter}.", JsonSerializer.Serialize(batchRequest));

        var securityFilter = await GetGroupSecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanViewAdGroup);
        var results = await groupService.GetBatchAsync(batchRequest, securityFilter, includeTotalCount: true);
        return results;
    }

    /// <summary>
    /// Method to Delete Group Record
    /// </summary>
    /// <param name="idToDelete">Id of the group to Delete</param>
    /// <returns>Secured result of deleted group model.</returns>
    public async Task<SecuredResult<GroupModel?>> DeleteGroupAsync(Guid idToDelete)
    {
        logger.LogInformation("Deleting group by Id: {Id}.", idToDelete);

        var groupToDelete = await groupService.GetAsync(idToDelete);

        // if not found return null.
        if (groupToDelete is null)
            return new SecuredResult<GroupModel?>(null);

        var filteredGroupToDelete = await ApplySecurityFilter(authorizationManager,groupTypeManager, AppPermissions.CanManageAdGroup, groupToDelete);
        if (!filteredGroupToDelete.Any())
            return new SecuredResult<GroupModel?>(null, failedAuthorization: true);

        await groupService.DeleteAsync(idToDelete);
        logger.LogInformation("Group with id: {id} deleted successfully.", idToDelete);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(typeof(GroupModel).Name, RecordAction.Delete, groupToDelete.Name));

        return new SecuredResult<GroupModel?>(groupToDelete);
    }

    /// <summary>
    /// Method to update Group Record
    /// </summary>
    /// <param name="groupToUpdate">Group Model to update</param>
    /// <returns>Secured result of updated group model.</returns>
    public async Task<SecuredResult<GroupModel?>> UpdateGroupAsync(GroupModel groupToUpdate)
    {
        logger.LogInformation("Updating group by Id: {Id}.", groupToUpdate.Id);

        var groupFound = await groupService.GetAsync(groupToUpdate.Id);
        if (groupFound is null)
            return new SecuredResult<GroupModel?>(null);

        var filteredGroupToUpdate = await ApplySecurityFilter(authorizationManager, groupTypeManager, AppPermissions.CanManageAdGroup, groupToUpdate);
        if (!filteredGroupToUpdate.Any())
            return new SecuredResult<GroupModel?>(null, failedAuthorization: true);

        await groupService.UpdateAsync(groupToUpdate);
        logger.LogInformation("Update group by Id: {Id} completed.", groupToUpdate.Id);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(typeof(GroupModel).Name, RecordAction.Update, groupFound.Name, AuditLog.Summarize(groupFound, groupToUpdate)));

        return new SecuredResult<GroupModel?>(groupToUpdate);
    }

    /// <summary>
    /// Method to assign user to group record
    /// </summary>
    /// <param name="groupId">Id of the Group</param>
    /// <param name="userModel">UserModel</param>
    /// <returns>Task</returns>
    public async Task AssignUserAsync(Guid groupId, UserModel userModel)
    {
        logger.LogInformation("Assigning user {userId} to the group {groupId}.", userModel.Id, groupId);
        // Get existing User
        var exUser = await userService.GetAsync(userModel.Id);

        // User can be added only to Group of Type: Application
        var applicationGroupType = await groupTypeManager.GetGroupTypeByNameAsync(GroupTypeNames.Application.ToString());
        var targetGroup = await groupService.GetAsync(groupId);
        if (targetGroup?.GroupTypeId != applicationGroupType?.Id)
        {
            logger.LogError("Group: {targetGroup.Name} is not allowed for user assignment.", targetGroup?.Name);
            throw new NotSupportedException($"Group: {targetGroup?.Name} is not allowed for user assignment.");
        }

        await userGroupService.AddAsync(new GroupUserModel() { GroupId = groupId, UserId = userModel.Id });
        logger.LogInformation("User {userId} assigned to the group {groupId}.", userModel.Id, groupId);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(typeof(GroupModel).Name, RecordAction.Assign, targetGroup?.Name, exUser?.FullName));

    }

    /// <summary>
    /// Method to remove user from group
    /// </summary>
    /// <param name="groupId">Id of the Group</param>
    /// <param name="userId">Id of the User</param>
    /// <returns>Task</returns>
    public async Task RemoveUserAsync(Guid groupId, Guid userId)
    {
        logger.LogInformation("Removing user {userId} from the group {groupId}.", userId, groupId);
        // Get existing User
        var exUser = await userService.GetAsync(userId);

        // User can be removed only from Group of Type: Application
        var applicationGroupType = await groupTypeManager.GetGroupTypeByNameAsync(GroupTypeNames.Application.ToString());
        var targetGroup = await groupService.GetAsync(groupId);
        if (targetGroup?.GroupTypeId != applicationGroupType?.Id)
        {
            logger.LogError("Group: {targetGroup.Name} is not allowed for user assignment.", targetGroup?.Name);
            throw new NotSupportedException($"Group: {targetGroup?.Name} is not allowed for user assignment.");
        }

        await userGroupService.RemoveAsync(new GroupUserModel() { UserId = userId, GroupId = groupId });
        logger.LogInformation("User {userId} removed from the group {groupId}.", userId, groupId);
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(typeof(GroupModel).Name, RecordAction.Remove, targetGroup?.Name, exUser?.FullName));
    }
}
