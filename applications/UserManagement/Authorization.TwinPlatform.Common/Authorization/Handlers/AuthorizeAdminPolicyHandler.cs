using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Security.Claims;

namespace Authorization.TwinPlatform.Common.Authorization.Handlers;

/// <summary>
/// Authorization Handler class to authorize user if the user is configured as admin in user management
/// </summary>
public class AuthorizeAdminPolicyHandler : AuthorizationHandler<AuthorizePermissionRequirement>
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AuthorizePermissionPolicyHandler> _logger;

    public AuthorizeAdminPolicyHandler(IAdminService adminService, ILogger<AuthorizePermissionPolicyHandler> logger)
    {
        _adminService = adminService;
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

            var adminList = await _adminService.GetAdminEmails();

            // if the user is an admin, pass the requirement
            if(adminList.Contains(userEmailClaim.Value))
            {
                context.Succeed(requirement);
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
