
namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// DTO Class for representing Authorization Response
/// </summary>
public class AuthorizationResponse
{
	/// <summary>
	/// List of Authorized Permissions with its condition
	/// </summary>
	public IEnumerable<AuthorizedPermission> Permissions { get; set; }
		=  new List<AuthorizedPermission>();
}
