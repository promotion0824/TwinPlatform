using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Role Assignment Manager Class
/// </summary>
/// <param name="roleAssignmentService">Instance of IRoleAssignmentService.</param>
/// <param name="groupRoleAssignmentService">Instance of IGroupRoleAssignmentService.</param>
/// <param name="logger">Instance of ILogger.</param>
/// <param name="expressionService">Willow Expression Service.</param>
/// <param name="userAuthorization">User Authorization Manager.</param>
/// <param name="groupTypeManager">Group Type Manager.</param>
/// <param name="auditLogger">Audit Logger Implementation.</param>
public class RoleAssignmentManager(IRoleAssignmentService roleAssignmentService,
        IGroupRoleAssignmentService groupRoleAssignmentService,
        ILogger<RoleAssignmentManager> logger,
        IWillowExpressionService expressionService,
        IUserAuthorizationManager userAuthorization,
        IGroupTypeManager groupTypeManager,
        IAuditLogger<RoleAssignmentManager> auditLogger) : BaseManager, IRoleAssignmentManager
{
    /// <summary>
    /// Get count of user role assignment entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetUserRoleCountAsync()
    {
        return await roleAssignmentService.GetCountAsync();
    }

    /// <summary>
    /// Get batch of User Role Assignment records.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO Instance.</param>
    /// <returns>Batch DTO of User Role Assignment Model.</returns>
    public async Task<BatchDto<UserRoleAssignmentModel>> GetUserRoleAssignmentsAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Getting batch of User Role Assignments: {batch}", JsonSerializer.Serialize(batchRequest));

        var userAssignments = await roleAssignmentService.GetBatchAsync(batchRequest, null, true);

        SetConditionExpressionStatus<UserRoleAssignmentModel>(userAssignments.Items);
        return userAssignments;
    }

    /// <summary>
    /// Get count of group role assignment entity.
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetGroupRoleCountAsync()
    {
        return await groupRoleAssignmentService.GetCountAsync();
    }

    /// <summary>
    /// Get batch of Group Role Assignment records.
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO Instance.</param>
    /// <returns>Batch DTO of Group Role Assignment Model.</returns>
    public async Task<BatchDto<GroupRoleAssignmentModel>> GetGroupRoleAssignmentsAsync(BatchRequestDto batchRequest)
    {
        logger.LogTrace("Getting batch of Group Role Assignments: {batch}", JsonSerializer.Serialize(batchRequest));

        var groupSecurityFilter = await GetGroupRoleAssignmentSecurityFilter(userAuthorization, groupTypeManager, AppPermissions.CanViewAdGroup);
        var groupAssignments = await groupRoleAssignmentService.GetBatchAsync(batchRequest, groupSecurityFilter, true);
        
        SetConditionExpressionStatus<GroupRoleAssignmentModel>(groupAssignments.Items);
        return groupAssignments;
    }

    /// <summary>
    /// Method to add User Role Assignment Record
    /// </summary>
    /// <param name="roleAssignment">RoleAssignment Model</param>
    /// <returns>Model of the inserted record</returns>
    public async Task<SecuredResult<UserRoleAssignmentModel?>> AddUserRoleAssignmentAsync(UserRoleAssignmentModel roleAssignment)
    {
        logger.LogInformation("Adding new user role assignment record: {roleAssignment}.", roleAssignment);
        roleAssignment.Id = await roleAssignmentService.AddAsync(roleAssignment);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(UserRoleAssignmentModel).Name, RecordAction.Create, roleAssignment.Id.ToString(), roleAssignment.GetRowName()));
        return new SecuredResult<UserRoleAssignmentModel?>(roleAssignment);
    }

    /// <summary>
    /// Method to add Group Role Assignment Record
    /// </summary>
    /// <param name="roleAssignment">GroupRoleAssignment Model</param>
    /// <returns>Id of the inserted record</returns>
    public async Task<SecuredResult<GroupRoleAssignmentModel?>> AddGroupRoleAssignmentAsync(GroupRoleAssignmentModel roleAssignment)
    {
        // Apply Authorization
        var securedResult = await ApplySecurityFilter(userAuthorization, groupTypeManager, AppPermissions.CanManageAdGroup, roleAssignment.Group);
        if (securedResult == null || !securedResult.Any())
        {
            return new SecuredResult<GroupRoleAssignmentModel?>(null, failedAuthorization: true);
        }

        logger.LogInformation("Adding new group role assignment record: {roleAssignment}.", roleAssignment);
        roleAssignment.Id = await groupRoleAssignmentService.AddAsync(roleAssignment);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(GroupRoleAssignmentModel).Name, RecordAction.Create, roleAssignment.Id.ToString(), roleAssignment.GetRowName()));
        return new SecuredResult<GroupRoleAssignmentModel?>(roleAssignment);
    }

    /// <summary>
    /// Method to Update User Role Assignment
    /// </summary>
    /// <param name="roleAssignment">RoleAssignment Model</param>
    /// <returns>Task that can be awaited to return updated model.</returns>
    public async Task<SecuredResult<UserRoleAssignmentModel?>> UpdateUserRoleAssignment(UserRoleAssignmentModel roleAssignment)
    {
        var exAssignment = await roleAssignmentService.GetAssignmentByIdAsync(roleAssignment.Id);
        logger.LogInformation("Updating user role assignment record: {roleAssignment}.", roleAssignment);
        await roleAssignmentService.UpdateAsync(roleAssignment);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(RoleAssignmentModel).Name, RecordAction.Update, roleAssignment.Id.ToString(), AuditLog.Summarize(exAssignment, roleAssignment)));
        return new SecuredResult<UserRoleAssignmentModel?>(roleAssignment);
    }

    /// <summary>
    /// Method to Update Group Role Assignment
    /// </summary>
    /// <param name="groupRoleAssignmentModel">GroupRoleAssignment Model</param>
    /// <returns>Task</returns>
    public async Task<SecuredResult<GroupRoleAssignmentModel?>> UpdateGroupRoleAssignment(GroupRoleAssignmentModel groupRoleAssignmentModel)
    {
        var exAssignment = await groupRoleAssignmentService.GetAssignmentByIdAsync(groupRoleAssignmentModel.Id);
        if (exAssignment == null)
        {
            return new SecuredResult<GroupRoleAssignmentModel?>(null);
        }

        // Apply Authorization
        var securedResult = await ApplySecurityFilter(userAuthorization, groupTypeManager, AppPermissions.CanManageAdGroup, exAssignment.Group);
        if (securedResult == null || !securedResult.Any())
        {
            return new SecuredResult<GroupRoleAssignmentModel?>(null, failedAuthorization: true);
        }

        logger.LogTrace("Updating group role assignment record: {roleAssignment}.", groupRoleAssignmentModel);
        await groupRoleAssignmentService.UpdateAsync(groupRoleAssignmentModel);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(GroupRoleAssignmentModel).Name, RecordAction.Update, groupRoleAssignmentModel.Id.ToString(), AuditLog.Summarize(exAssignment, groupRoleAssignmentModel)));
        return new SecuredResult<GroupRoleAssignmentModel?>(groupRoleAssignmentModel);
    }

    /// <summary>
    /// Method to delete Role Assignment Record
    /// </summary>
    /// <param name="roleAssignmentId">Id of the Role Assignment Record</param>
    /// <returns>Task</returns>
    public async Task<SecuredResult<Guid?>> DeleteUserRoleAssignmentAsync(Guid roleAssignmentId)
    {
        var assignment = await roleAssignmentService.GetAssignmentByIdAsync(roleAssignmentId);
        if (assignment == null)
        {
            return new SecuredResult<Guid?>(null);
        }

        logger.LogTrace("Delete user role assignment record with Id: {Id}.", roleAssignmentId);
        await roleAssignmentService.RemoveAsync(roleAssignmentId);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(UserRoleAssignmentModel).Name, RecordAction.Delete, assignment?.Id.ToString(), assignment?.GetRowName()));
        return new SecuredResult<Guid?>(roleAssignmentId);
    }

    /// <summary>
    /// Method to delete Group Role Assignment Record
    /// </summary>
    /// <param name="groupRoleAssignmentId">Id of the Group Role Assignment Record</param>
    /// <returns>Task</returns>
    public async Task<SecuredResult<Guid?>> DeleteGroupRoleAssignmentAsync(Guid groupRoleAssignmentId)
    {
        var assignment = await groupRoleAssignmentService.GetAssignmentByIdAsync(groupRoleAssignmentId);
        if (assignment == null)
        {
            return new SecuredResult<Guid?>(null);
        }

        // Apply Authorization
        var securedResult = await ApplySecurityFilter(userAuthorization, groupTypeManager, AppPermissions.CanManageAdGroup, assignment.Group);
        if (securedResult == null || !securedResult.Any())
        {
            return new SecuredResult<Guid?>(null, failedAuthorization: true);
        }

        logger.LogTrace("Delete group role assignment record with Id: {Id}.", groupRoleAssignmentId);
        await groupRoleAssignmentService.RemoveAsync(groupRoleAssignmentId);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(GroupRoleAssignmentModel).Name, RecordAction.Delete, assignment?.Id.ToString(), assignment?.GetRowName()));
        return new SecuredResult<Guid?>(groupRoleAssignmentId);
    }

    /// <summary>
    /// Evaluate Role Assignment Condition Expression.
    /// </summary>
    /// <param name="expression">String Expression to validate.</param>
    /// <param name="errors">List of Errors while parsing or evaluating the expression.</param>
    /// <returns>ExpressionStatus.</returns>
    public ExpressionStatus EvaluateConditionExpression(string? expression, out List<string> errors)
    {
        return expressionService.GetExpressionStatus(expressionService.GetUMDefaultEnvironment(), expression, out errors);
    }

    private IEnumerable<T> SetConditionExpressionStatus<T>(IEnumerable<T> roleAssignments) where T : RoleAssignmentModel
    {
        var expEnv = expressionService.GetUMDefaultEnvironment();
        foreach (var roleAssignment in roleAssignments)
        {
            roleAssignment.ConditionExpressionStatus = expressionService.GetExpressionStatus(expEnv, roleAssignment.Condition, out _);
        }

        return roleAssignments;
    }
}
