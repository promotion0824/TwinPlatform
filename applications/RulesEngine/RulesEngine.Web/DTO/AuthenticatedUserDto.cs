namespace RulesEngine.Web;

/// <summary>
/// Information about the logged in user
/// </summary>
public class AuthenticatedUserDto
{
    /// <summary>
	/// Display name for this user
	/// </summary>
    public string DisplayName { get; set; }

	/// <summary>
	/// User id
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Email
	/// </summary>
	public string Email { get; set; }

}
