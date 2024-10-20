using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing livedata connectors
/// </summary>
public class CanViewConnectors : WillowAuthorizationRequirement
{
}

public class CanViewConnectorsEvaluator : GlobalPermissionAuthHandler<CanViewConnectors>
{
    public CanViewConnectorsEvaluator(IAuthService authService, ILogger<CanViewConnectorsEvaluator> logger) : base(authService, logger)
    {
    }
}
