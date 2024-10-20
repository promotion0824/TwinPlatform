using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

public class CanInstallConnectors : WillowAuthorizationRequirement
{
}

public class CanInstallConnectorsEvaluator : GlobalPermissionAuthHandler<CanInstallConnectors>
{
    public CanInstallConnectorsEvaluator(IAuthService authService, ILogger<CanInstallConnectorsEvaluator> logger) : base(authService, logger)
    {
    }
}
