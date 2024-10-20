using Authorization.Common.Abstracts;
using Authorization.Common.Models;

namespace Authorization.TwinPlatform.Permission.Api.Abstracts;

/// <summary>
/// User Manager Interface.
/// </summary>
public interface IUserManager
{
    /// <summary>
    /// Method to get list of users
    /// </summary>
    /// <param name="filterPropertyModel">Filter Property Model</param>
    /// <returns>List of users</returns>
    Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IUser;

    /// <summary>
    /// Method to get User entity by Email
    /// </summary>
    /// <param name="email">Email Id of the User</param>
    /// <returns>User Model of the User entity</returns>
    Task<UserModel?> GetByEmailAsync(string email);

    /// <summary>
    /// Get List of user by Ids
    /// </summary>
    /// <param name="ids">Array of User Ids</param>
    /// <returns>List of User Model</returns>
    Task<List<UserModel>> GetByIds(Guid[] ids);
}
