using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Security.Claims;

namespace Authorization.TwinPlatform.Common.Authorization.Handlers;

/// <summary>
/// Authorization Handler class to authorize user based on the authorized permissions
/// </summary>
public class AuthorizePermissionPolicyHandler : AuthorizationHandler<AuthorizePermissionRequirement>
{
    private readonly IUserAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizePermissionPolicyHandler> _logger;

    public AuthorizePermissionPolicyHandler(IUserAuthorizationService authorizationService, ILogger<AuthorizePermissionPolicyHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizePermissionRequirement requirement)
    {
        try
        {
            //Fail Authorization if any identity has failed authentication
            if (context.User != null && context.User.Identities.Any(a => !a.IsAuthenticated))
                return;

            var userEmailClaim = context.User?.FindFirst(IsUserEmailClaim);

            //Skip if no user principal found
            if (userEmailClaim == null) return;

            var authorizationResponse = await _authorizationService.GetAuthorizationResponse(userEmailClaim.Value);

            if (authorizationResponse != null)
            {
                if (authorizationResponse.Permissions.Any(p => string.Equals(p.Name, requirement.PermissionName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogError("Failed Authorization: User:{Email} has no matching permission:{Permission}.", userEmailClaim.Value, requirement.PermissionName);
                }
            }
            else
            {
                _logger.LogError("Error retrieving permission from the Authorization API.");
            }
        }
        catch (SocketException)
        {
            _logger.LogError("Authorization service is not reachable. Please see if UM extension is installed and configured.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Call to Permission API failed. Unable to validate authorization requirement {name}.", requirement.PermissionName);
        }

    }

    private bool IsUserEmailClaim(Claim claim)
    {
        return claim.Type == "emails" || claim.Type == ClaimTypes.Email;
    }
}
