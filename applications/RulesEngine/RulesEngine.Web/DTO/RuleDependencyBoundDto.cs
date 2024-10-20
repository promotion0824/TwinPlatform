using Newtonsoft.Json;
using System;
using Willow.Rules.Model;

#nullable disable  // just a poco

namespace RulesEngine.Web;

/// <summary>
/// A <see cref="RuleDependencyBoundDto"/> signifies a relationship between two rules
/// </summary>
public class RuleDependencyBoundDto
{
    /// <summary>
    /// Creates a new RuleDependencyBoundDto (for deserialization)
    /// </summary>
    [JsonConstructor]
	private RuleDependencyBoundDto()
	{
	}

    /// <summary>
    /// Creates a new RuleDependencyBoundDto
    /// </summary>
    public RuleDependencyBoundDto(RuleDependencyBound dependencyBound)
	{
        Relationship = dependencyBound.Relationship;
        RuleInstanceId = dependencyBound.RuleInstanceId;
        TwinId = dependencyBound.TwinId;
        TwinName = dependencyBound.TwinName;
        RuleId = dependencyBound.RuleId;
        RuleName = dependencyBound.RuleName;
	}

	/// <summary>
	/// The relationship to the referenced rule instance
	/// </summary>
	public string Relationship { get; init; }

	/// <summary>
	/// The rule instance id
	/// </summary>
	public string RuleInstanceId { get; init; }

	/// <summary>
	/// The twin id of the referenced rule instance
	/// </summary>
	public string TwinId { get; init; }

	/// <summary>
	/// The twin name of the referenced rule instance
	/// </summary>
	public string TwinName { get; init; }

	/// <summary>
	/// The rule id of the referenced rule
	/// </summary>
	public string RuleId { get; init; }

	/// <summary>
	/// The rule name of the referenced rule
	/// </summary>
	public string RuleName { get; init; }
}
