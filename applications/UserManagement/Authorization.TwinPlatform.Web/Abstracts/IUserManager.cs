using Authorization.Common.Models;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage User entity records
/// </summary>
public interface IUserManager
{
    /// <summary>
    /// Get count of user entity.
    /// </summary>
    /// <returns></returns>
    Task<int> GetCountAsync();

    /// <summary>
    /// Get batch of Users.
    /// </summary>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <returns>Batch DTO Of User Model.</returns>
    Task<BatchDto<UserModel>> GetUsersAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Get batch of Users by group.
    /// </summary>
    /// <param name="groupId">Group Id</param>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <param name="getOnlyNonMembers">Only Users who are not a member of the group will be returned</param>
    /// <returns>Batch DTO Of User Model.</returns>
    Task<BatchDto<UserModel>> GetUsersByGroupAsync(string groupId, BatchRequestDto batchRequest, bool getOnlyNonMembers=false);

    /// <summary>
    /// Get list of admin user by comparing the super admin emails from permission api.
    /// </summary>
    /// <returns>List of UserModel.</returns>
    public Task<IEnumerable<UserModel>> GeAdminUsersAsync();

    /// <summary>
    /// Method to get user by Id
    /// </summary>
    /// <param name="Id">Id of the User Record</param>
    /// <returns>UserModel</returns>
    public Task<UserModel?> GetUserByIdAsync(Guid Id);

	/// <summary>
	/// Method to get user by Email Id
	/// </summary>
	/// <param name="email">Id of the User Record</param>
	/// <returns>UserModel</returns>
	public Task<UserModel?> GetUserByEmailAsync(string email);

	/// <summary>
	/// Method to Add User Record
	/// </summary>
	/// <param name="model">User Model</param>
	/// <returns>Inserted User Model</returns>
	Task<UserModel> AddUserAsync(UserModel model);

	/// <summary>
	/// Method to Update User Record
	/// </summary>
	/// <param name="model">User Model to update</param>
	/// <returns>Id of the Updated User Record</returns>
	Task<Guid> UpdateUserAsync(UserModel model);

	/// <summary>
	/// Method to Delete User Record
	/// </summary>
	/// <param name="id">Id of the User Record to delete</param>
	/// <returns>Id of the deleted record</returns>
	Task<Guid> DeleteUserAsync(Guid id);	
}
