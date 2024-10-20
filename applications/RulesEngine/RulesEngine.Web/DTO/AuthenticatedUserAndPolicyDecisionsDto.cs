using System.Collections.Generic;
using System.Linq;

namespace RulesEngine.Web;

/// <summary>
/// An authenticated user, groups and the policy decisions
/// </summary>
public class AuthenticatedUserAndPolicyDecisionsDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public AuthenticatedUserAndPolicyDecisionsDto()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public AuthenticatedUserAndPolicyDecisionsDto(AuthenticatedUserDto user, IEnumerable<AuthorizationDecisionDto> policies)
        : this(policies)
    {
        User = user;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public AuthenticatedUserAndPolicyDecisionsDto(IEnumerable<AuthorizationDecisionDto> policies)
    {
        PolicyDecisions = policies.ToDictionary(x => x.Name, x => new DecisionDto { Success = x.Success, Reason = x.Reason });
    }

    /// <summary>
    /// The user
    /// </summary>
    public AuthenticatedUserDto User { get; set; }

	/// <summary>
	/// Policy decisions
	/// </summary>
	public Dictionary<string, DecisionDto> PolicyDecisions { get; set; }
}
