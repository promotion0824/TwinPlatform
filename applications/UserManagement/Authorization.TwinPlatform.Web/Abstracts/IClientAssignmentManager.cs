using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Auth;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Client Assignment Manager Contract
/// </summary>
public interface IClientAssignmentManager
{
    /// <summary>
    /// Get Client Assignment Records for an Application.
    /// </summary>
    /// <param name="applicationName">Name of the Application.</param>
    /// <param name="filter">Filter Property Model.</param>
    /// <returns>List of Client Assignment Model.</returns>
    Task<List<ClientAssignmentModel>> GetClientAssignmentsAsync(string applicationName, FilterPropertyModel filter);

    /// <summary>
    /// Adds Client Assignment Record
    /// </summary>
    /// <param name="clientAssignment">ClientAssignmentModel</param>
    /// <returns>Model of the inserted record</returns>
    Task<SecuredResult<ClientAssignmentModel?>> AddClientAssignmentAsync(ClientAssignmentModel clientAssignment);


    /// <summary>
    /// Update Client Assignment Record
    /// </summary>
    /// <param name="clientAssignmentModel">Client Assignment Model.</param>
    /// <returns>Task</returns>
    Task<SecuredResult<ClientAssignmentModel?>> UpdateClientAssignmentAsync(ClientAssignmentModel clientAssignmentModel);

    /// <summary>
    /// Delete Client Assignment Record
    /// </summary>
    /// <param name="clientAssignmentId">Id of the Client Assignment Record</param>
    /// <returns>Task</returns>
    Task<SecuredResult<Guid?>> DeleteClientAssignmentAsync(Guid clientAssignmentId);

    /// <summary>
    /// Evaluate Client Assignment Condition Expression.
    /// </summary>
    /// <param name="expression">String Expression to validate.</param>
    /// <param name="errors">List of Errors while parsing or evaluating the expression.</param>
    /// <returns>ExpressionStatus.</returns>
    ExpressionStatus EvaluateConditionExpression(string? expression, out List<string> errors);
}
