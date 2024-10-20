using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Class to manage User entity records
/// </summary>
public class UserManager(IUserService userService,
    IAdminService adminService,
    ILogger<UserManager> logger,
    IUserAuthorizationManager userAuthorization,
    IAuditLogger<UserManager> auditLogger) : BaseManager, IUserManager
{
    /// <summary>
    /// Get count of user entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetCountAsync()
    {
        return await userService.GetCountAsync();
    }

    /// <summary>
    /// Get batch of Users.
    /// </summary>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <returns>Batch DTO Of User Model.</returns>
    public async Task<BatchDto<UserModel>> GetUsersAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Getting all users record by filter: {filter}.", JsonSerializer.Serialize(batchRequest));
        var users = await userService.GetBatchAsync(batchRequest, null, includeTotalCount: true);
        // Set Admin Users
        var adminUserEmails = await adminService.GetAdminEmails();

        foreach (var user in users.Items)
        {
            if (adminUserEmails.Contains(user.Email))
            {
                user.isAdmin = true;
            }
        }
        return users;
    }

    /// <summary>
    /// Get batch of Users by group.
    /// </summary>
    /// <param name="groupId">Group Id</param>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <param name="getOnlyNonMembers">Only Users who are not a member of the group will be returned</param>
    /// <returns>Batch DTO Of User Model.</returns>
    public async Task<BatchDto<UserModel>> GetUsersByGroupAsync(string groupId, BatchRequestDto batchRequest, bool getOnlyNonMembers = false)
    {
        logger.LogTrace("Getting users by group:{group}.",groupId);

        var users = await userService.GetBatchAsync(batchRequest,
            getOnlyNonMembers ?
                x => x.UserGroups.All(a => a.GroupId.ToString() != groupId) :
                x => x.UserGroups.Any(a => a.GroupId.ToString() == groupId),
            includeTotalCount: true);

        return users;
    }

    /// <summary>
    /// Get list of admin user by comparing the super admin emails from permission api.
    /// </summary>
    /// <returns>List of UserModel.</returns>
    public async Task<IEnumerable<UserModel>> GeAdminUsersAsync()
    {
        logger.LogTrace("Finding admin user");
        var adminUserEmails = await adminService.GetAdminEmails();
        var users = await userService.GetListAsync<UserModel>(new FilterPropertyModel());
        return users.Where(w => adminUserEmails.Contains(w.Email)).ToList();
    }

    /// <summary>
    /// Method to get user by Id
    /// </summary>
    /// <param name="Id">Id of the User Record</param>
    /// <returns>UserModel</returns>
    public Task<UserModel?> GetUserByIdAsync(Guid Id)
    {
        logger.LogTrace("Finding user record by Id: {Id}", Id);
        return userService.GetAsync(Id);
    }

    /// <summary>
    /// Method to get user by Email Id
    /// </summary>
    /// <param name="email">Id of the User Record</param>
    /// <returns>UserModel</returns>
    public Task<UserModel?> GetUserByEmailAsync(string email)
    {
        logger.LogTrace("Finding user record by email: {email}", email);
        return userService.GetByEmailAsync(email);
    }

    /// <summary>
    /// Method to Add User Record
    /// </summary>
    /// <param name="model">User Model</param>
    /// <returns>Inserted User Model</returns>
    public async Task<UserModel> AddUserAsync(UserModel model)
    {
        logger.LogInformation("Creating user with email: {email}.", model.Email);
        model = await userService.AddAsync(model);
        logger.LogInformation("User with email: {email} created successfully with an Id: {Id}", model.Email, model.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(UserModel).Name, RecordAction.Create, model.Email));
        return model;
    }

    /// <summary>
    /// Method to Update User Record
    /// </summary>
    /// <param name="model">User Model to update</param>
    /// <returns>Id of the Updated User Record</returns>
    public async Task<Guid> UpdateUserAsync(UserModel model)
    {
        var oldModel = await userService.GetByEmailAsync(model.Email);
        logger.LogInformation("Updating user record by Id: {Id}.", model.Id);
        model.Id = await userService.UpdateAsync(model);
        logger.LogInformation("Update user by Id: {Id} completed.", model.Id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(UserModel).Name, RecordAction.Update, model.Email, AuditLog.Summarize(oldModel, model)));
        return model.Id;
    }

    /// <summary>
    /// Method to Delete User Record
    /// </summary>
    /// <param name="id">Id of the User Record to delete</param>
    /// <returns>Id of the deleted record</returns>
    public async Task<Guid> DeleteUserAsync(Guid id)
    {
        var existingModel = await userService.GetAsync(id);
        logger.LogInformation("Deleting user by Id: {Id}.", id);
        await userService.DeleteAsync(id);
        logger.LogInformation("User with id: {id} deleted successfully.", id);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(UserModel).Name, RecordAction.Delete, existingModel!.Email));
        return id;
    }
}
