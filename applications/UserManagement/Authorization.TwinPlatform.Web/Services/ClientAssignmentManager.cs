using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;

namespace Authorization.TwinPlatform.Web.Services;
public class ClientAssignmentManager(
    IClientAssignmentService clientAssignmentService,
    IWillowExpressionService expressionService,
    ILogger<ClientAssignmentManager> logger,
    IUserAuthorizationManager userAuthorization,
    IAuditLogger<ClientAssignmentManager> auditLogger) : BaseManager, IClientAssignmentManager
{
    /// <summary>
    /// Get Client Assignment Records for an Application.
    /// </summary>
    /// <param name="applicationName">Name of the Application.</param>
    /// <param name="filter">Filter Property Model.</param>
    /// <returns>List of Client Assignment Model.</returns>
    public async Task<List<ClientAssignmentModel>> GetClientAssignmentsAsync(string applicationName, FilterPropertyModel filter)
    {
        logger.LogTrace("Getting client assignment records for application:{Application}", applicationName);
        var clientAssignmentRecords = await clientAssignmentService.GetListAsync(filter);
        SetConditionExpressionStatus(clientAssignmentRecords);
        return clientAssignmentRecords;
    }

    /// <summary>
    /// Adds Client Assignment Record
    /// </summary>
    /// <param name="clientAssignment">ClientAssignmentModel</param>
    /// <returns>Model of the inserted record</returns>
    public async Task<SecuredResult<ClientAssignmentModel?>> AddClientAssignmentAsync(ClientAssignmentModel clientAssignment)
    {
        logger.LogTrace("Adding new client assignment record: {ClientAssignment}.", clientAssignment);
        clientAssignment.Id = await clientAssignmentService.AddAsync(clientAssignment);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(ClientAssignmentModel).Name, RecordAction.Create, clientAssignment.Id.ToString()));
        return new SecuredResult<ClientAssignmentModel?>(clientAssignment);
    }

    /// <summary>
    /// Update Client Assignment Record
    /// </summary>
    /// <param name="clientAssignmentModel">Client Assignment Model.</param>
    /// <returns>Task</returns>
    public async Task<SecuredResult<ClientAssignmentModel?>> UpdateClientAssignmentAsync(ClientAssignmentModel clientAssignmentModel)
    {
        var assignment = await clientAssignmentService.GetAsync(clientAssignmentModel.Id);
        if (assignment == null)
        {
            return new SecuredResult<ClientAssignmentModel?>(null);
        }

        logger.LogTrace("Updating client assignment record with Id: {Id}.", clientAssignmentModel.Id);
        await clientAssignmentService.UpdateAsync(clientAssignmentModel);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(ClientAssignmentModel).Name, RecordAction.Update, assignment?.Id.ToString()));
        return new SecuredResult<ClientAssignmentModel?>(clientAssignmentModel);
    }

    /// <summary>
    /// Delete Client Assignment Record
    /// </summary>
    /// <param name="clientAssignmentId">Id of the Client Assignment Record</param>
    /// <returns>Task</returns>
    public async Task<SecuredResult<Guid?>> DeleteClientAssignmentAsync(Guid clientAssignmentId)
    {
        var assignment = await clientAssignmentService.GetAsync(clientAssignmentId);
        if (assignment == null)
        {
            return new SecuredResult<Guid?>(null);
        }

        logger.LogTrace("Deleting client assignment record with Id: {Id}.", clientAssignmentId);
        await clientAssignmentService.DeleteAsync(clientAssignmentId);
        auditLogger.LogInformation(userAuthorization.CurrentEmail, AuditLog.Format(typeof(ClientAssignmentModel).Name, RecordAction.Delete, assignment?.Id.ToString()));
        return new SecuredResult<Guid?>(clientAssignmentId);
    }

    /// <summary>
    /// Evaluate Client Assignment Condition Expression.
    /// </summary>
    /// <param name="expression">String Expression to validate.</param>
    /// <param name="errors">List of Errors while parsing or evaluating the expression.</param>
    /// <returns>ExpressionStatus.</returns>
    public ExpressionStatus EvaluateConditionExpression(string? expression, out List<string> errors)
    {
        return expressionService.GetExpressionStatus(expressionService.GetUMDefaultEnvironment(), expression, out errors);
    }


    private IEnumerable<T> SetConditionExpressionStatus<T>(IEnumerable<T> clientAssignments) where T : ClientAssignmentModel
    {
        var expEnv = expressionService.GetUMDefaultEnvironment();
        foreach (var clientAssignment in clientAssignments)
        {
            clientAssignment.ConditionExpressionStatus = expressionService.GetExpressionStatus(expEnv, clientAssignment.Condition, out _);
        }

        return clientAssignments;
    }
}
