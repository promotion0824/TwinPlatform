using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Microsoft.Extensions.Caching.Memory;

namespace Authorization.TwinPlatform.Permission.Api.Services;

/// <summary>
/// User Manager class implementation.
/// </summary>
public class UserManager(IUserService userService, IMemoryCache memoryCache) : IUserManager
{
    /// <summary>
    /// Method to get list of users
    /// </summary>
    /// <param name="filterPropertyModel">Filter Property Model</param>
    /// <returns>List of users</returns>
    public Task<UserModel?> GetByEmailAsync(string email)
    {
        return memoryCache.GetOrCreateAsync($"User_by_email_{email}", async (cacheEntry) =>
        {
            // set cache expiration for 2 minute
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await userService.GetByEmailAsync(email);
        });
    }

    /// <summary>
    /// Method to get User entity by Email
    /// </summary>
    /// <param name="email">Email Id of the User</param>
    /// <returns>User Model of the User entity</returns>
    public Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IUser
    {
        return userService.GetListAsync<T>(searchPropertyModel);
    }

    /// <summary>
    /// Get List of user by Ids
    /// </summary>
    /// <param name="ids">Array of User Ids</param>
    /// <returns>List of User Model</returns>
    public Task<List<UserModel>> GetByIds(Guid[] ids)
    {
        return userService.GetUsersByIdsAsync(ids);
    }
}
