using Authorization.Common.Abstracts;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;

namespace Authorization.TwinPlatform.Abstracts;

/// <summary>
/// Interface to manager Users Entity
/// </summary>
public interface IUserService : IBatchRequestEntityService<User, UserModel>
{
	/// <summary>
	/// Method to get list of users
	/// </summary>
	/// <param name="filterPropertyModel">Filter Property Model</param>
	/// <returns>List of users</returns>
	Task<List<T>> GetListAsync<T>(FilterPropertyModel searchPropertyModel) where T : IUser;

    /// <summary>
    /// Method to get User entity by Id
    /// </summary>
    /// <param name="id">Id of the User</param>
    /// <returns>User Model of the User entity</returns>
    Task<UserModel?> GetAsync(Guid id);

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
    Task<List<UserModel>> GetUsersByIdsAsync(Guid[] ids);

    /// <summary>
    /// Adds new User Entity to the database
    /// </summary>
    /// <param name="model">UserModel to add</param>
    /// <returns>Id of the inserted user entity</returns>
    Task<UserModel> AddAsync(UserModel model);

	/// <summary>
	/// Method to update User entity
	/// </summary>
	/// <param name="model">Model of the User</param>
	/// <returns>Id of the Updated user</returns>
	Task<Guid> UpdateAsync(UserModel model);

	/// <summary>
	/// Method to delete user entity
	/// </summary>
	/// <param name="Id">Id of the user</param>
	/// <returns>Task</returns>
	Task DeleteAsync(Guid Id);

    /// <summary>
    /// Import user records from file data model
    /// </summary>
    /// <param name="userRecordToImport">User Record File models to import</param>
    /// <returns>List of Import Errors</returns>
    Task<IEnumerable<UserFileModel>> ImportAsync(IEnumerable<UserFileModel> userRecordToImport);
}
