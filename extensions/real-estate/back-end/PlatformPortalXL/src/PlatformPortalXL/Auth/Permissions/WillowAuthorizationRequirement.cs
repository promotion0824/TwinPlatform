using Microsoft.AspNetCore.Authorization;

namespace PlatformPortalXL.Auth.Permissions;

/// <summary>
/// The Willow authorization requirement for permissions.
/// </summary>
public abstract class WillowAuthorizationRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The name of the permission for this requirement.
    /// </summary>
    public string Name => GetType().Name;
}
