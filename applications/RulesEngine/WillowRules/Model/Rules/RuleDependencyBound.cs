using Newtonsoft.Json;
using System;
using WillowRules.RepositoryConfiguration;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// A <see cref="RuleDependencyBound"/> signifies a relationship between two <see cref="RuleInstance"/>'s
/// </summary>
public class RuleDependencyBound
{
	/// <summary>
	/// Creates a new <see cref="RuleDependencyBound"/> (for deserialization)
	/// </summary>
	[JsonConstructor]
	private RuleDependencyBound()
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleDependencyBound"/>
	/// </summary>
	public RuleDependencyBound(string relationship, string ruleInstanceId, string twinId, string twinName, string ruleId, string ruleName)
	{
		Relationship = relationship ?? throw new ArgumentNullException(nameof(relationship));
		RuleInstanceId = ruleInstanceId ?? throw new ArgumentNullException(nameof(ruleInstanceId));
		TwinId = twinId ?? throw new ArgumentNullException(nameof(twinId));
		TwinName = twinName ?? "";
		RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
		RuleName = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
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
