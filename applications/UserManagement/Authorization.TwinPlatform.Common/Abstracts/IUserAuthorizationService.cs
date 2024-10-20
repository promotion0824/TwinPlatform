using Authorization.TwinPlatform.Common.Model;
using Willow.Batch;

namespace Authorization.TwinPlatform.Common.Abstracts;

/// <summary>
/// Contract for Authorization Service used by the Client Application (Extension) to make calls to the permission api
/// </summary>
public interface IUserAuthorizationService
{
    /// <summary>
    /// Method to get the list of authorized permission for the supplied user email.
    /// <param name="userName">Email Address of the User</param>
    /// <returns>Authorization Response model</returns>
    Task<AuthorizationResponse> GetAuthorizationResponse(string userName);

    /// <summary>
    /// Get batch of groups (Group Type: Application)
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <returns>Batch DTO of GroupModel</returns>
    Task<BatchDto<GroupModel>> GetApplicationGroupsAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Get batch of groups (Group Type: Application) for a User (: UserId)
    /// </summary>
    /// <param name="userId">Guid of the User record.</param>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <returns>Batch DTO of GroupModel</returns>
    Task<BatchDto<GroupModel>> GetApplicationGroupsByUserAsync(string userId, BatchRequestDto batchRequest);

    /// <summary>
    /// Get list of users based on the filter property model.
    /// </summary>
    /// <param name="filterModel">Filter Property Model.</param>
    /// <returns>ListResponse<UserModel></returns>
    Task<ListResponse<UserModel>> GetUsersAsync(FilterPropertyModel filterModel);

    /// <summary>
    /// Get user by email address.
    /// </summary>
    /// <param name="email">Email address of the user.</param>
    /// <returns>UserModel if found; else null.</returns>
    Task<UserModel?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Get List of User By Ids.
    /// </summary>
    /// <param name="userIds">Ids of the user.</param>
    /// <returns>List of UserModel.</returns>
    Task<ListResponse<UserModel>> GetListOfUserByIds(string[] userIds);

    /// <summary>
    /// Invalidate cache for the requested cache store types.
    /// </summary>
    /// <param name="cacheStoreTypes">Types of cache stores.</param>
    /// <returns>Awaitable Task.</returns>
    public Task InvalidateCache(CacheStoreType[] cacheStoreTypes);
}
