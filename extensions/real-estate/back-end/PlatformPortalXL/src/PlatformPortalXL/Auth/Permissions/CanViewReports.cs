using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing reports
/// </summary>
public class CanViewReports : WillowAuthorizationRequirement
{
}

public class CanViewReportsEvaluator : GlobalPermissionAuthHandler<CanViewReports>
{
    public CanViewReportsEvaluator(IAuthService authService, ILogger<CanViewReportsEvaluator> logger) : base(authService, logger)
    {
    }
}
