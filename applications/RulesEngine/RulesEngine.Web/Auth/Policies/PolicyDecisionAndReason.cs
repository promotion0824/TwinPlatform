using System;

namespace RulesEngine.Web.Auth.Policies;

/// <summary>
/// The outcode of a policy decision and the (non-disclosing-security-related-information) reason for failure
/// </summary>/
/// <remarks>
/// Be careful not to disclose any security-related information in the reason, e.g. "Contact your admin for permissions is OK"
/// but "Not a member of top-secret-admins-group" it not.
/// </remarks>
public class PolicyDecisionAndReason
{
	/// <summary>
	/// The succes, fail or unknown outcome
	/// </summary>
	public PolicyDecisionOutcome PolicyDecisionOutcome { get; set; }

	/// <summary>
	/// Reason why this policy failed or succeeded
	/// </summary>
	public string Reason { get; set; }

	/// <summary>
	/// Create a failed outcome
	/// </summary>
	internal static PolicyDecisionAndReason Fail(string name, string reason)
	{
		return new PolicyDecisionAndReason { PolicyDecisionOutcome = PolicyDecisionOutcome.Fail, Reason = reason };
	}

	/// <summary>
	/// Create a success outcome
	/// </summary>
	internal static PolicyDecisionAndReason Success(string name, string reason)
	{
		return new PolicyDecisionAndReason { PolicyDecisionOutcome = PolicyDecisionOutcome.Success, Reason = reason };
	}

	/// <summary>
	/// Create an unknown outcome
	/// </summary>
	internal static PolicyDecisionAndReason Unknown(string name, string reason)
	{
		return new PolicyDecisionAndReason { PolicyDecisionOutcome = PolicyDecisionOutcome.Undefined, Reason = reason };
	}
}
