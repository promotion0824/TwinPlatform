namespace Authorization.TwinPlatform.Web.Auth;

/// <summary>
/// Class defining User Management Authorization Response for Front End
/// </summary>
public class AuthorizationResponseDto
{
	/// <summary>
	/// List of User Permissions
	/// </summary>
	public IEnumerable<string> Permissions { get; set; } = null!;

	/// <summary>
	/// True if user has admin privilege; or else false
	/// </summary>
	public bool IsAdminUser { get; set; }
}
