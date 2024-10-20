using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth.AuthHandlers;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// Policy for editing Users
/// </summary>
public class CanEditUsers : WillowAuthorizationRequirement
{
}

public class CanEditUsersCustomerIdEvaluator : CustomerIdPermissionAuthHandler<CanEditUsers>
{
    public CanEditUsersCustomerIdEvaluator(IAuthService authService, ILogger<CanEditUsersCustomerIdEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanEditUsersTwinIdEvaluator : TwinIdPermissionAuthHandler<CanEditUsers>
{
    public CanEditUsersTwinIdEvaluator(IAuthService authService, ILogger<CanEditUsersTwinIdEvaluator> logger) : base(authService, logger)
    {
    }
}

public class CanEditUsersTwinScopedEvaluator : TwinScopedPermissionAuthHandler<CanEditUsers>
{
    public CanEditUsersTwinScopedEvaluator(IAuthService authService, ILogger<CanEditUsersTwinScopedEvaluator> logger) : base(authService, logger)
    {
    }
}
