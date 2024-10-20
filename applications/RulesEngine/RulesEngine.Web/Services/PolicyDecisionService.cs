using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

#nullable enable

namespace RulesEngine.Web;

/// <summary>
/// A service for getting information about the logged in user's permissions
/// </summary>
public interface IPolicyDecisionService
{
    /// <summary>
    /// Get policy decisions for authorization
    /// </summary>
    Task<List<AuthorizationDecisionDto>> GetPolicyDecisions(ClaimsPrincipal user, object? resource);
}

/// <summary>
/// A service for getting information about the logged in user's permissions
/// </summary>
public class PolicyDecisionService : IPolicyDecisionService
{
    private readonly IEnumerable<IAuthorizationHandler> authorizationPolicyHandlers;
    private readonly IEnumerable<IWillowAuthorizationRequirement> authorizationRequirements;
    private readonly ILogger<PolicyDecisionService> logger;
    private readonly IMemoryCache memoryCache;

    /// <summary>
    /// Creates a new UserService
    /// </summary>
    public PolicyDecisionService(
        IEnumerable<IAuthorizationHandler> authorizationPolicyHandlers,
        IEnumerable<IWillowAuthorizationRequirement> authorizationRequirements,
        IMemoryCache memoryCache,
        ILogger<PolicyDecisionService> logger)
    {
        this.authorizationPolicyHandlers = authorizationPolicyHandlers ?? throw new ArgumentNullException(nameof(authorizationPolicyHandlers));
        this.authorizationRequirements = authorizationRequirements ?? throw new ArgumentNullException(nameof(authorizationRequirements));
        this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        this.memoryCache = memoryCache ?? throw new System.ArgumentNullException(nameof(memoryCache));
    }

    /// <summary>
    /// Gets policy decisions for a user and HttpContext
    /// </summary>
    /// <param name="user">The user claims principal</param>
    /// <param name="resource">The resource</param>
    /// <returns></returns>
    public async Task<List<AuthorizationDecisionDto>> GetPolicyDecisions(ClaimsPrincipal user, object? resource)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);  // A guid

        if (string.IsNullOrEmpty(userId)) return new List<AuthorizationDecisionDto>();

        var authorizationHandlerContext = new AuthorizationHandlerContext(authorizationRequirements, user, resource);

        List<AuthorizationDecisionDto> policyDecisions = new();

        foreach (var authorizationPolicyHandler in authorizationPolicyHandlers.OfType<IWillowAuthorizationHandler>())
        {
            // Find the matching requirement(s) object
            var type = authorizationPolicyHandler.RequirementType;
            // In this implementation we enforce a 1:1 between authorization handlers and requirements on controller methods
            var requirement = authorizationRequirements.Single(ar => ar.GetType() == type);

            var authorizationContext = new AuthorizationHandlerContext(new[] { requirement }, user, resource);
            await authorizationPolicyHandler.HandleAsync(authorizationContext);

            var result = new AuthorizationDecisionDto
            {
                Name = requirement.GetType().Name,
                Success = authorizationContext.HasSucceeded,
                Reason = string.Join(", ", authorizationContext.FailureReasons.Select(x => x.Message))
            };

            policyDecisions.Add(result);
        }

        return policyDecisions;
    }
}
