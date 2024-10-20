using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.AuthHandlers;

/// <summary>
/// Policy evaluator for authorizing permission against a scope id.
/// </summary>
public abstract class TwinIdPermissionAuthHandler<TRequirement> :
    AuthorizationHandler<TRequirement, string>, IWillowAuthorizationHandler where TRequirement : WillowAuthorizationRequirement
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    /// <summary>
    /// Policy evaluator for authorizing permission against a scope id.
    /// </summary>
    protected TwinIdPermissionAuthHandler(IAuthService authService, ILogger logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Requirement type.
    /// </summary>
    public Type RequirementType => typeof(TRequirement);

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, string resource)
    {
        _logger.LogTrace("Handling requirement '{Requirement}' with scope '{Resource}'", requirement.Name, resource);

        if (string.IsNullOrWhiteSpace(resource))
        {
            _logger.LogInformation("Authorization failed for requirement '{Requirement}' with scope {Resource}", requirement.Name, resource);
            context.Fail(new AuthorizationFailureReason(this, "scope identifier for the resource not provided'"));
            return;
        }

        var hasPermission = await _authService.HasPermission<TRequirement>(context.User, resource);
        if (hasPermission)
        {
            _logger.LogInformation("Authorization succeeded for '{Requirement}' with scope {Resource}", requirement.Name, resource);
            context.Succeed(requirement);
            return;
        }

        _logger.LogInformation("Authorization failed for '{Requirement}' with scope {Resource}", requirement.Name, resource);
        context.Fail(new AuthorizationFailureReason(this, $"User is not linked to permission '{requirement.Name}'"));
    }
}
