using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Extensions;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Represents the name of an auth policy and whether it was granted or not.
/// </summary>
/// <param name="Name">The name of the policy.</param>
/// <param name="HasSucceeded">True if the policy was granted.</param>
/// <param name="FailureReason">Reason for the failure when success is false.</param>
public record PolicyDecision(string Name, bool HasSucceeded, string FailureReason);

/// <summary>
/// A service for getting and caching information about the logged-in user's permissions.
/// </summary>
public interface IPolicyDecisionService
{
    /// <summary>
    /// Gets policy decisions for the current user.
    /// </summary>
    /// <param name="httpContext">The HttpContext</param>
    Task<IEnumerable<PolicyDecision>> GetPolicyDecisions(HttpContext httpContext);
}

public class PolicyDecisionService : IPolicyDecisionService
{
    private readonly IEnumerable<IAuthorizationHandler> _authorizationPolicyHandlers;
    private readonly IEnumerable<WillowAuthorizationRequirement> _authorizationRequirements;
    private readonly ILogger<PolicyDecisionService> _logger;
    private readonly IMemoryCache _memoryCache;

    public PolicyDecisionService(
        IEnumerable<IAuthorizationHandler> authorizationPolicyHandlers,
        IEnumerable<WillowAuthorizationRequirement> authorizationRequirements,
        ILogger<PolicyDecisionService> logger,
        IMemoryCache memoryCache)
    {
        _authorizationPolicyHandlers = authorizationPolicyHandlers;
        _authorizationRequirements = authorizationRequirements;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<IEnumerable<PolicyDecision>> GetPolicyDecisions(HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return [];
        }

        var sw = Stopwatch.StartNew();

        var key = UserAuthCachingKeys.CacheKeyForAuthPolicyDecisions(userId);

        var policyDecisions = await _memoryCache.GetOrCreateLockedAsync(key, async c =>
        {
            c.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            List<PolicyDecision> policyDecisions = [];

            var authHandlers = _authorizationPolicyHandlers.OfType<IWillowAuthorizationHandler>()
                    .Where(i => i.IsGlobalPermissionEvaluator())
                    .ToArray();

            _logger.LogTrace("UserPolicyAuthService: {Count} IWillowAuthorization handler(s) resolved for policy evaluation", authHandlers.Length);

            foreach (var authHandler in authHandlers)
            {
                var policyDecision = await GetPolicyDecision(authHandler, httpContext);

                policyDecisions.Add(policyDecision);
            }

            return policyDecisions;
        })!;

        _logger.LogTrace("==== UserPolicyAuthService: GetPolicyDecisions evaluation took {Time} ({ElapsedMilliseconds:0}ms) ====", sw, sw.ElapsedMilliseconds);

        return policyDecisions!;
    }

    private async Task<PolicyDecision> GetPolicyDecision(IWillowAuthorizationHandler authHandler, HttpContext httpContext)
    {
        var requirement = _authorizationRequirements.Single(ar => ar.GetType() == authHandler.RequirementType);

        var authorizationContext = new AuthorizationHandlerContext(new[] { requirement }, httpContext.User, httpContext);

        await authHandler.HandleAsync(authorizationContext);

        var reason = string.Join(", ", authorizationContext.FailureReasons.Select(r => r.Message));
        return new PolicyDecision(requirement.GetType().Name, authorizationContext.HasSucceeded, reason);
    }
}
