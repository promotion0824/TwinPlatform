using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

internal class ViewSites : WillowAuthorizationRequirement
{
}

internal class ViewSitesLegacyPermissionEvaluator : TwinIdPermissionAuthHandler<ViewSites>
{
    public ViewSitesLegacyPermissionEvaluator(IAuthService authService, ILogger<ViewSitesLegacyPermissionEvaluator> logger) : base(authService, logger)
    {
    }
}
