using Authorization.TwinPlatform.Web.Auth;
using System.Security.Claims;

namespace Authorization.TwinPlatform.Web.Abstracts;

/// <summary>
/// Abstract to manage user management authorization permissions
/// </summary>
public interface IUserAuthorizationManager
{
	/// <summary>
	/// Method to get the authorization data for the current user from Permission API
	/// </summary>
	/// <param name="userEmail">Email address of the principal user</param>
	/// <returns>AuthorizationResponseDto instance</returns>
	public Task<AuthorizationResponseDto> GetAuthorizationPermissions(string userEmail);

    /// <summary>
    /// Get current user email address.
    /// </summary>
    /// <returns>Email address string if email claim is found; else null.</returns>
    public string CurrentEmail { get;}

    /// <summary>
    /// Check if the current user has any permission that matches the input permission name.
    /// </summary>
    /// <returns>True if current user has access;else false.</returns>
    public Task<bool> CheckCurrentUserHasPermission(string permissionName);

    /// <summary>
    /// Check if the current user is admin by querying permission api.
    /// </summary>
    /// <returns>True if admin; false if not.</returns>
    public Task<bool> IsCurrentUserAnAdminAsync();
}
