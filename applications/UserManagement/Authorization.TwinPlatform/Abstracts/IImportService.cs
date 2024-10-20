using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Service contract for managing authorization imports
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Update Application Option.
    /// </summary>
    /// <param name="applicationModel">Application Model.</param>
    /// <param name="applicationOption">Application Options.</param>
    /// <returns>Awaitable task.</returns>
    Task UpdateApplication(ApplicationModel applicationModel, ApplicationOption applicationOption);

    /// <summary>
    /// Method to import permissions
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createPermissions">List of create permissions</param>
    /// <returns>Completed task</returns>
    public Task ImportPermissionsAsync(ApplicationModel application, List<CreatePermissionModel> createPermissions);

    /// <summary>
    /// Method to import roles
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createRoleModels">List of create roles instances</param>
    /// <returns>Completed task</returns>
    public Task ImportRolesAsync(ApplicationModel application, List<CreateRoleModel> createRoleModels);

    /// <summary>
    /// Method to update permissions within roles
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createRoleModels">List of create roles instances</param>
    /// <returns>Completed task</returns>
    public Task UpdateRolePermissions(ApplicationModel application, List<CreateRoleModel> createRoleModels);

    /// <summary>
    /// Method to import groups
    /// </summary>
	/// <param name="application">Application Model.</param>
    /// <param name="createGroupModels">List of create group model instances</param>
    /// <returns>Awaitable task</returns>
    public Task ImportGroups(ApplicationModel application, List<CreateGroupModel> createGroupModels);


    /// <summary>
    /// Import group assignment from configuration.
    /// </summary>
	/// <param name="application">Application Model.</param>
    /// <param name="createGroupAssignmentModels">List of create group assignment models.</param>
    /// <returns>Awaitable task.</returns>
    public Task ImportGroupAssignments(ApplicationModel application, List<CreateGroupAssignmentModel> createGroupAssignmentModels);

}
