using System.Security.Claims;
using PlatformPortalXL.Features.Auth;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Provides access to auth specific feature flags.
/// </summary>
public interface IAuthFeatureFlagService
{
    /// <summary>
    /// Returns true if fine-grained auth is enabled for the current user.
    /// </summary>
    /// <remarks>
    /// Fine-grained auth is enabled if the user has the "CanUseFineGrainedAuth" permission in User Management.
    /// </remarks>
    bool IsFineGrainedAuthEnabled { get; }
}

public class AuthFeatureFlagService : IAuthFeatureFlagService
{
    private readonly ICurrentUser _currentUser;

    public AuthFeatureFlagService(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public bool IsFineGrainedAuthEnabled
    {
        get
        {
            var fgaValue = _currentUser.Value.FindFirstValue(CustomClaimTypes.IsFineGrainedAuthEnabled);

            return bool.TryParse(fgaValue, out var fgaEnabled) && fgaEnabled;
        }
    }
}
