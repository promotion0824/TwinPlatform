using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing Users
/// </summary>
public class CanViewUsers : WillowAuthorizationRequirement
{
}

public class CanViewUsersEvaluator : GlobalPermissionAuthHandler<CanViewUsers>
{
    public CanViewUsersEvaluator(IAuthService authService, ILogger<CanViewUsersEvaluator> logger) : base(authService, logger)
    {
    }
}
