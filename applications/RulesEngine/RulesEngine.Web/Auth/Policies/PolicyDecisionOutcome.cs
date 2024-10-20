namespace RulesEngine.Web.Auth.Policies;

/// <summary>
/// Policy decision outcome
/// </summary>
public enum PolicyDecisionOutcome
{
	/// <summary>
	/// Not enough information to ealuate this policy
	/// </summary>
	Undefined = 0,

	/// <summary>
	/// Successful
	/// </summary>
	Success = 1,

	/// <summary>
	/// Failed
	/// </summary>
	Fail = 2
}
