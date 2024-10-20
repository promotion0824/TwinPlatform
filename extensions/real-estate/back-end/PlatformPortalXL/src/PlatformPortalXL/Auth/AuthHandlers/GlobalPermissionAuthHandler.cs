using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.AuthHandlers;

/// <summary>
/// Policy evaluator for authorizing a global permission.
/// </summary>
/// <remarks>
/// A global permission is one that applies to all twins, i.e. is not scoped  to a particular twin.
/// </remarks>
public abstract class GlobalPermissionAuthHandler<TRequirement> :
    AuthorizationHandler<TRequirement>, IWillowAuthorizationHandler, IGlobalPermissionEvaluator where TRequirement : WillowAuthorizationRequirement
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    /// <summary>
    /// Policy evaluator for authorizing static permission.
    /// </summary>
    protected GlobalPermissionAuthHandler(IAuthService authService, ILogger logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement type.
    /// </summary>
    public Type RequirementType => typeof(TRequirement);

    /// <summary>
    /// Handle the authorization requirement.
    /// </summary>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        _logger.LogTrace("Handling requirement '{RequirementName}'", requirement.Name);

        var hasPermission = await _authService.HasPermission<TRequirement>(context.User);
        if (hasPermission)
        {
            _logger.LogDebug("Authorization succeeded for requirement '{RequirementName}'", requirement.Name);
            context.Succeed(requirement);
            return;
        }

        _logger.LogDebug("Authorization failed for requirement '{RequirementName}'", requirement.Name);
        context.Fail(new AuthorizationFailureReason(this, $"User is not linked to permission '{requirement.Name}'"));
    }
}
