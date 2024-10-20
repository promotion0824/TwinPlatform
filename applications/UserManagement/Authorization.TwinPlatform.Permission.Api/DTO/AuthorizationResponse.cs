namespace Authorization.TwinPlatform.Permission.Api.DTO;

/// <summary>
/// Class dto implementation for get authorized permission request
/// </summary>
public class AuthorizationResponse
{

	/// <summary>
	/// List of Authorized Permissions with its condition
	/// </summary>
	public IEnumerable<PermissionResponse>? Permissions { get; set; } 

	/// <summary>
	/// Tells if the current user is a super admin user who get full access to everything
	/// </summary>
	public bool IsAdminUser { get; set; }	
}
