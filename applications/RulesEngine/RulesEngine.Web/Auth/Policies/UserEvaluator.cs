using System.Security.Claims;

namespace RulesEngine.Web.Auth;

/// <summary>
/// Helps with assessing a user
/// </summary>
public class UserEvaluator
{
	private readonly ClaimsPrincipal user;

	/// <summary>
	/// Creates a new <see cref="UserEvaluator" />
	/// </summary>
	public UserEvaluator(ClaimsPrincipal user)
	{
		this.user = user ?? throw new System.ArgumentNullException(nameof(user));
	}

	/// <summary>
	/// Gets the user's email claim
	/// </summary>
	public string Email => user.FindFirst(ClaimTypes.Email)?.Value ?? "";

	/// <summary>
	/// Checks the user is logged in
	/// </summary>
	public bool IsLoggedIn => !string.IsNullOrEmpty(this.Email);

	/// <summary>
	/// Checks if the user work for Willow (experimental)
	/// </summary>
	public bool IsWillowEmployee => this.Email.EndsWith("@willowinc.com", System.StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Checks if the user works for a partner (experimental)
	/// </summary>
	public bool IsPartnerEmployee => this.Email.EndsWith("@microsoft.com", System.StringComparison.OrdinalIgnoreCase) ||
		this.Email.EndsWith("@cbre.com", System.StringComparison.OrdinalIgnoreCase);

}