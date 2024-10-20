using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for viewing twins
/// </summary>
public class CanViewTwins : WillowAuthorizationRequirement
{
}

public class CanViewTwinsEvaluator : GlobalPermissionAuthHandler<CanViewTwins>
{
    public CanViewTwinsEvaluator(IAuthService authService, ILogger<CanViewTwinsEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanViewTwinsTwinIdEvaluator : TwinIdPermissionAuthHandler<CanViewTwins>
{
    public CanViewTwinsTwinIdEvaluator(IAuthService authService, ILogger<CanViewTwinsTwinIdEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanViewTwinsTwinScopedEvaluator : TwinScopedPermissionAuthHandler<CanViewTwins>
{
    public CanViewTwinsTwinScopedEvaluator(IAuthService authService, ILogger<CanViewTwinsTwinScopedEvaluator> logger) : base(authService, logger)
    {
    }
}
