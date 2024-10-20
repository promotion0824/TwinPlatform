using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.AuthHandlers;

/// <summary>
/// Policy evaluator for authorizing permission against a scoped twin instance.
/// </summary>
public abstract class TwinScopedPermissionAuthHandler<TRequirement> :
    AuthorizationHandler<TRequirement, ITwinWithAncestors>, IWillowAuthorizationHandler where TRequirement : WillowAuthorizationRequirement
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    /// <summary>
    /// Policy evaluator for authorizing permission against a scoped twin instance.
    /// </summary>
    protected TwinScopedPermissionAuthHandler(IAuthService authService, ILogger logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement type.
    /// </summary>
    public Type RequirementType => typeof(TRequirement);

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, ITwinWithAncestors resource)
    {
        _logger.LogTrace("Handling requirement '{Requirement}' for twin '{TwinId}'", requirement.Name, resource.TwinId);

        var hasPermission = await _authService.HasPermission<TRequirement>(context.User, resource);
        if (hasPermission)
        {
            _logger.LogDebug("Authorization succeeded for '{Requirement}' with twin {TwinId}", requirement.Name, resource.TwinId);
            context.Succeed(requirement);
            return;
        }

        _logger.LogDebug("Authorization failed for '{Requirement}' with twin {TwinId}", requirement.Name, resource.TwinId);
        context.Fail(new AuthorizationFailureReason(this, $"User is not linked to permission '{requirement.Name}'"));
    }
}
