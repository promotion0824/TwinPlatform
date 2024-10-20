using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using Willow.Expressions;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Class for Twin Platform Permission Aggregation Service
/// </summary>
public class PermissionAggregatorService(TwinPlatformAuthContext authContext,
        IMapper mapper,
        ILogger<PermissionAggregatorService> logger,
        HealthCheckSqlServer healthCheckSqlServer,
        IWillowExpressionService willowExpressionService) : IPermissionAggregatorService
{

    /// <summary>
    /// Method to query list of permission for a user optionally filtered by an extension namespace
    /// </summary>
    /// <param name="userEmail">User principal name</param>
    /// <returns>Permission Model and its associated condition</returns>
    /// <exception cref="ArgumentNullException">Thrown when useremail is empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when an invalid useremail is supplied</exception>
    public async Task<IEnumerable<ConditionalPermissionModel>> GetUserPermissions(string userEmail)
    {
        logger.LogTrace("Received request for User Email : {UserEmail}", userEmail);

        if (string.IsNullOrEmpty(userEmail)) throw new ArgumentNullException(nameof(userEmail));

        if (!MailAddress.TryCreate(userEmail, out var mailAddress))
        {
            throw new InvalidOperationException("Invalid user email address.");
        }

        IEnumerable<ConditionalPermissionModel> result = [];
        try
        {
            var allUserRolesQuery = authContext.GetRoleAssignmentsByUser(userEmail).AsNoTracking();

            var userPermissionsQuery = allUserRolesQuery.Join(authContext.RolePermissions.Include(i => i.Permission.Application), roleAssignment => roleAssignment.RoleId,
                rp => rp.RoleId, (roleAssignment, rp) => new { rp.Permission, roleAssignment.Condition, roleAssignment.Expression });

            // Include the Application and Fetch the Permission from the database
            var userPermissions = await userPermissionsQuery.ToListAsync();

            // We create and pass the willow Expression Environment to the evaluator, so that the dynamic vars don't change for each calls.
            var expressionEnvironment = willowExpressionService.GetUMDefaultEnvironment();
            userPermissions = userPermissions.Where(up => EvaluateExpression(up.Condition, expressionEnvironment)).ToList();

            result = userPermissions.Select(x =>
            new ConditionalPermissionModel(mapper.Map<Permission, PermissionModel>(x.Permission),
            x.Expression ?? string.Empty,
            x.Condition ?? string.Empty));

            healthCheckSqlServer.Current = HealthCheckSqlServer.Healthy;
        }
        catch (Exception ex)
        {
            healthCheckSqlServer.Current = HealthCheckSqlServer.FailingCalls;
            logger.LogError(ex, "Error executing request for User Email : {UserEmail}", userEmail);
        }
        finally
        {
            logger.LogTrace("Request completed with {PermissionCount} permissions for user:{user}", result.Count(), userEmail);
        }

        return result;
    }

    private bool EvaluateExpression(string? expression, Env expressionEnvironment)
    {
        // Null or empty expression in an assignment will be treated as an active assignment
        if (string.IsNullOrWhiteSpace(expression))
            return true;

        try
        {
            var evaluationResult = willowExpressionService.Evaluate<bool>(expression, expressionEnvironment, out List<string> errors);
            return evaluationResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to evaluate expression :{Expression}", expression);
            return false;
        }
    }

    /// <summary>
    /// Gets the list of permission for the application client.
    /// </summary>
    /// <param name="clientId">Id of the client.</param>
    /// <param name="applicationName">Name of the application.</param>
    /// <returns>Enumerable of Conditional Permission Model.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IEnumerable<ConditionalPermissionModel>> GetClientPermissions(string clientId, string applicationName)
    {
        logger.LogTrace("Received request for Client : {ClientId}", clientId);

        if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException(nameof(clientId));
        IEnumerable<ConditionalPermissionModel> result = [];

        try
        {
            // Check if the client exists
            var client = await authContext.ApplicationClients.AsNoTracking()
                                    .Where(w => w.ClientId.ToString() == clientId && w.Application.Name == applicationName)
                                    .FirstOrDefaultAsync();

            //  Return an empty list if no client is found
            if (client is null)
            {
                return result;
            }

            var rawResult = await authContext.ClientAssignments.AsNoTracking()
                                         .Include(i => i.ClientAssignmentPermissions).ThenInclude(t => t.Permission).ThenInclude(i=>i.Application)
                                         .Where(w => w.ApplicationClientId == client.Id)
                                         .ToListAsync();

            var expressionEnvironment = willowExpressionService.GetUMDefaultEnvironment();
            var activeAssignments = rawResult.Where(w => EvaluateExpression(w.Condition, expressionEnvironment)).ToList();

            result = activeAssignments.SelectMany(s => s.ClientAssignmentPermissions.Select(ca =>
            new ConditionalPermissionModel(mapper.Map<Permission, PermissionModel>(ca.Permission), s.Expression ?? string.Empty, s.Condition ?? string.Empty)));

            healthCheckSqlServer.Current = HealthCheckSqlServer.Healthy;
        }
        catch (Exception ex)
        {
            healthCheckSqlServer.Current = HealthCheckSqlServer.FailingCalls;
            logger.LogError(ex, "Error executing request for Client : {ClientId}", clientId);
        }
        finally
        {
            logger.LogTrace("Request completed with {PermissionCount} permissions for Client:{ClientId}", result.Count(), clientId);
        }

        return result;
    }
}
