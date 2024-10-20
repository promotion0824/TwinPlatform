using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing search and explore
/// </summary>
public class CanViewSearchAndExplore : WillowAuthorizationRequirement
{
}

public class CanViewSearchAndExploreEvaluator : GlobalPermissionAuthHandler<CanViewSearchAndExplore>
{
    public CanViewSearchAndExploreEvaluator(IAuthService authService, ILogger<CanViewSearchAndExploreEvaluator> logger) : base(authService, logger)
    {
    }
}
