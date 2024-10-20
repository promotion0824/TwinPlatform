using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing dashboards
/// </summary>
public class CanViewDashboards : WillowAuthorizationRequirement
{
}

public class CanViewDashboardsEvaluator : GlobalPermissionAuthHandler<CanViewDashboards>
{
    public CanViewDashboardsEvaluator(IAuthService authService, ILogger<CanViewDashboardsEvaluator> logger) : base(authService, logger)
    {
    }
}
