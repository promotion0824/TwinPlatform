using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for can manage portfolios
/// </summary>
public class CanManagePortfolios : WillowAuthorizationRequirement
{
}

public class CanManagePortfoliosEvaluator : GlobalPermissionAuthHandler<CanManagePortfolios>
{
    public CanManagePortfoliosEvaluator(IAuthService authService, ILogger<CanManagePortfoliosEvaluator> logger) : base(authService, logger)
    {
    }
}
