using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for editing Apps
/// </summary>
public class CanEditApps : WillowAuthorizationRequirement
{
}

public class CanEditAppsEvaluator : GlobalPermissionAuthHandler<CanEditApps>
{
    public CanEditAppsEvaluator(IAuthService authService, ILogger<CanEditAppsEvaluator> logger) : base(authService, logger)
    {
    }
}
