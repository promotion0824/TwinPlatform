using Microsoft.AspNetCore.Authorization;

namespace Authorization.TwinPlatform.Common.Authorization.Requirements;

/// <summary>
/// Authorization Requirement class for permission based policy evaluation
/// </summary>
public class AuthorizePermissionRequirement : IAuthorizationRequirement
{
	/// <summary>
	/// Name of the Permission
	/// </summary>
	public string PermissionName { get; private set; }

	public AuthorizePermissionRequirement(string permissionName) {
		this.PermissionName = permissionName;
	}
}
