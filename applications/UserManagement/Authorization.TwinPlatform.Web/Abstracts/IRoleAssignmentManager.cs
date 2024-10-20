using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Auth;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Interface to manage User Assignment and Group Assignment entity records
/// </summary>
public interface IRoleAssignmentManager
{
    /// <summary>
    /// Get count of user role assignment entity.
    /// </summary>
    /// <returns></returns>
    public Task<int> GetUserRoleCountAsync();

    /// <summary>
    /// Get batch of User Role Assignment records.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO Instance.</param>
    /// <returns>Batch DTO of User Role Assignment Model.</returns>
    public Task<BatchDto<UserRoleAssignmentModel>> GetUserRoleAssignmentsAsync(BatchRequestDto batchRequest);


    /// <summary>
    /// Get count of group role assignment entity.
    /// </summary>
    /// <returns></returns>
    public Task<int> GetGroupRoleCountAsync();

    /// <summary>
    /// Get batch of Group Role Assignment records.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO Instance.</param>
    /// <returns>Batch DTO of Group Role Assignment Model.</returns>
    public Task<BatchDto<GroupRoleAssignmentModel>> GetGroupRoleAssignmentsAsync(BatchRequestDto batchRequest);

    /// <summary>
    /// Method to add User Role Assignment Record
    /// </summary>
    /// <param name="roleAssignment">RoleAssignment Model</param>
    /// <returns>Model of the inserted record</returns>
    public Task<SecuredResult<UserRoleAssignmentModel?>> AddUserRoleAssignmentAsync(UserRoleAssignmentModel roleAssignment);

	/// <summary>
	/// Method to add Group Role Assignment Record
	/// </summary>
	/// <param name="roleAssignment">GroupRoleAssignment Model</param>
	/// <returns>Model of the inserted record</returns>
	public Task<SecuredResult<GroupRoleAssignmentModel?>> AddGroupRoleAssignmentAsync(GroupRoleAssignmentModel roleAssignment);

	/// <summary>
	/// Method to Update Role Assignment
	/// </summary>
	/// <param name="roleAssignment">RoleAssignment Model</param>
	/// <returns>Task</returns>
	public Task<SecuredResult<UserRoleAssignmentModel?>> UpdateUserRoleAssignment(UserRoleAssignmentModel roleAssignment);

	/// <summary>
	/// Method to Update Group Role Assignment
	/// </summary>
	/// <param name="groupRoleAssignmentModel">GroupRoleAssignment Model</param>
	/// <returns>Task</returns>
	public Task<SecuredResult<GroupRoleAssignmentModel?>> UpdateGroupRoleAssignment(GroupRoleAssignmentModel groupRoleAssignmentModel);

	/// <summary>
	/// Method to delete Role Assignment Record
	/// </summary>
	/// <param name="roleAssignmentId">Id of the Role Assignment Record</param>
	/// <returns>Task</returns>
	public Task<SecuredResult<Guid?>> DeleteUserRoleAssignmentAsync(Guid roleAssignmentId);

	/// <summary>
	/// Method to delete Group Role Assignment Record
	/// </summary>
	/// <param name="groupRoleAssignmentId">Id of the Group Role Assignment Record</param>
	/// <returns>Task</returns>
	public Task<SecuredResult<Guid?>> DeleteGroupRoleAssignmentAsync(Guid groupRoleAssignmentId);

    /// <summary>
    /// Validate Role Assignment Condition Expression.
    /// </summary>
    /// <param name="expression">String Expression to validate.</param>
    /// <param name="errors">List of Errors while parsing or evaluating the expression.</param>
    /// <returns>ExpressionStatus.</returns>
    public ExpressionStatus EvaluateConditionExpression(string? expression, out List<string> errors);

}
