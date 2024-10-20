using Authorization.Common.Models;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage Permission Entity Records
/// </summary>
public interface IPermissionManager
{
    /// <summary>
    /// Get count of permission entity.
    /// </summary>
    /// <returns></returns>
    public Task<int> GetCountAsync();

    /// <summary>
    /// Get Permission Batch
    /// </summary>
    /// <param name="batchRequest">BatchRequest DTO.</param>
    /// <returns>BatchDto Of PermissionModel</returns>
    public Task<BatchDto<PermissionModel>> GetPermissionsAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Get batch of Permissions by Role.
    /// </summary>
    /// <param name="roleId">Role Id</param>
    /// <param name="batchRequest">Batch Request configuration.</param>
    /// <param name="getOnlyNonMembers">Only Users who are not a member of the group will be returned</param>
    /// <returns>Batch DTO Of Permission Model.</returns>
    public Task<BatchDto<PermissionModel>> GetPermissionsByRoleAsync(string roleId, BatchRequestDto batchRequest, bool getOnlyNonMembers = false);

    /// <summary>
    /// Method to retrieve a Permission by Id
    /// </summary>
    /// <param name="permissionId">Id of the permission to retrieve</param>
    /// <returns>PermissionModel as response</returns>
    public Task<PermissionModel?> GetPermissionByIdAsync(Guid permissionId);

	/// <summary>
	/// Method to add permission to the collection
	/// </summary>
	/// <param name="permission">PermissionModel to insert</param>
	/// <returns>PermissionModel as response</returns>
	public Task<PermissionModel> AddPermissionAsync(PermissionModel permission);

	/// <summary>
	/// Method to Delete Permission Record
	/// </summary>
	/// <param name="idToDelete">Id of the Permission to Delete</param>
	/// <returns>True if success or false if not</returns>
	public Task<bool> DeletePermissionAsync(Guid idToDelete);

	/// <summary>
	/// Method to update Permission Record
	/// </summary>
	/// <param name="permissionModel">Permission Model to update</param>
	/// <returns>Updated model</returns>
	public Task<PermissionModel> UpdatePermissionAsync(PermissionModel permissionModel);

	/// <summary>
	/// Method to get all permission assigned to a user identified by the email
	/// </summary>
	/// <param name="email">Email of the User</param>
	/// <returns>List of conditional permission model</returns>
	public Task<IEnumerable<ConditionalPermissionModel>> GetPermissionsByUserEmail(string email);

	/// <summary>
	/// Method to get all permission inherited by a user based on AD Group membership
	/// </summary>
	/// <param name="email">Email of the User</param>
	/// <returns>List of conditional permission model</returns>
	public Task<IEnumerable<ConditionalPermissionModel>> GetPermissionBasedOnADGroupMembership(string email);
}
