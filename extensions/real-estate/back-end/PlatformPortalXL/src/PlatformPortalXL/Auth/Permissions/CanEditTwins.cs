using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for edit twins
/// </summary>
public class CanEditTwins : WillowAuthorizationRequirement
{
}

public class CanEditTwinsEvaluator : GlobalPermissionAuthHandler<CanEditTwins>
{
    public CanEditTwinsEvaluator(IAuthService authService, ILogger<CanEditTwinsEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanEditTwinsTwinIdEvaluator : TwinIdPermissionAuthHandler<CanEditTwins>
{
    public CanEditTwinsTwinIdEvaluator(IAuthService authService, ILogger<CanEditTwinsTwinIdEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanEditTwinsTwinScopedEvaluator : TwinScopedPermissionAuthHandler<CanEditTwins>
{
    public CanEditTwinsTwinScopedEvaluator(IAuthService authService, ILogger<CanEditTwinsTwinScopedEvaluator> logger) : base(authService, logger)
    {
    }
}
