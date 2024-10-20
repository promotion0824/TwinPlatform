namespace RulesEngine.Web;

/// <summary>
/// Result of an authorization policy calculation
/// </summary>
public class AuthorizationDecisionDto
{
	/// <summary>
	/// Name of the auth decision
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Success or fail
	/// </summary>

	public bool Success { get; set; }

	/// <summary>
	/// Reason for failure when success is false
	/// </summary>
	public string Reason { get; set; }
}

/// <summary>
/// A policy decision
/// </summary>
public class DecisionDto
{
	/// <summary>
	/// Success or fail
	/// </summary>
	public bool Success { get; set; }

	/// <summary>
	/// The reason for a failure
	/// </summary>
	public string Reason { get; set; }
}
