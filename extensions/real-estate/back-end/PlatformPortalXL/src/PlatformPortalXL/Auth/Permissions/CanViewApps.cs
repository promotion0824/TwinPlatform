using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing Apps
/// </summary>
public class CanViewApps : WillowAuthorizationRequirement
{
}

public class CanViewAppsEvaluator : GlobalPermissionAuthHandler<CanViewApps>
{
    public CanViewAppsEvaluator(IAuthService authService, ILogger<CanViewAppsEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanViewAppsTwinIdEvaluator : TwinIdPermissionAuthHandler<CanViewApps>
{
    public CanViewAppsTwinIdEvaluator(IAuthService authService, ILogger<CanViewAppsTwinIdEvaluator> logger) : base(authService, logger)
    {
    }
}
