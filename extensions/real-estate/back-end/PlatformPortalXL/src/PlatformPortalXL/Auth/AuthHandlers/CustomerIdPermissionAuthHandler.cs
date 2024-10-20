using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Features.Auth;

namespace PlatformPortalXL.Auth.AuthHandlers;

/// <summary>
/// Policy evaluator for authorizing permission against a customer id.
/// </summary>
public abstract class CustomerIdPermissionAuthHandler<TRequirement> :
    AuthorizationHandler<TRequirement, Guid>, IWillowAuthorizationHandler where TRequirement : WillowAuthorizationRequirement
{
    private readonly IAuthService _authService;
    private readonly ILogger _logger;

    /// <summary>
    /// Policy evaluator for authorizing permission against a customer id.
    /// </summary>
    protected CustomerIdPermissionAuthHandler(IAuthService authService, ILogger logger)
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
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, Guid resource)
    {
        _logger.LogTrace("Handling requirement '{RequirementName}'", requirement.Name);

        var customerIdClaim = context?.User.FindFirst(CustomClaimTypes.CustomerId)?.Value;
        if (!Guid.TryParse(customerIdClaim, out var customerIdClaimParsed) || resource != customerIdClaimParsed)
        {
            _logger.LogWarning("CustomerId is not that of the current user on checking user management customer permissions");
            context.Fail(new AuthorizationFailureReason(this, "CustomerId is invalid"));
        }

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
