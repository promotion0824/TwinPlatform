using System;
using Newtonsoft.Json;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// The available relationship types for a <see cref=RuleDependency"/>
/// </summary>
public static class RuleDependencyRelationships
{
	/// <summary>
	/// isRelated
	/// </summary>
	public const string RelatedTo = "isRelated";
	/// <summary>
	/// isReferencedCapability
	/// </summary>
	public const string ReferencedCapability = "isReferencedCapability";
	/// <summary>
	/// isSibling
	/// </summary>
	public const string Sibling = "isSibling";
}

/// <summary>
/// A <see cref="RuleDependency"/> signifies a relationship between two rules
/// </summary>
public class RuleDependency
{
	/// <summary>
	/// Creates a new <see cref="RuleDependency"/> (for deserialization)
	/// </summary>
	[System.Text.Json.Serialization.JsonConstructor]
	private RuleDependency()
	{
	}

	/// <summary>
	/// Creates a new <see cref="RuleDependency"/>
	/// </summary>
	public RuleDependency(string ruleId, string relationship)
	{
		if (string.IsNullOrEmpty(ruleId))
		{
			throw new ArgumentException($"'{nameof(ruleId)}' cannot be null or empty.", nameof(ruleId));
		}

		if (string.IsNullOrEmpty(relationship))
		{
			throw new ArgumentException($"'{nameof(relationship)}' cannot be null or empty.", nameof(relationship));
		}

		this.RuleId = ruleId;
		this.Relationship = relationship;
	}

	/// <summary>
	/// The relationship to the referenced rule. <see cref="RuleDependencyRelationships"/> for possbile values
	/// </summary>
	[JsonProperty(Order = 0)]
	public string Relationship { get; set; }

	/// <summary>
	/// The rule id of the referenced rule
	/// </summary>
	[JsonProperty(Order = 1)]
	public string RuleId { get; init; }
}
