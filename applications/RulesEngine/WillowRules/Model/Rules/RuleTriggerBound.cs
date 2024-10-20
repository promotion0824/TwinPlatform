using Newtonsoft.Json;
using System.Collections.Generic;
using WillowRules.RepositoryConfiguration;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// A <see cref="RuleTriggerBound"/> represents an output trigger for a rule instance
/// </summary>
public class RuleTriggerBound
{
	/// <summary>
	/// Construct for serialization
	/// </summary>
	public RuleTriggerBound()
	{
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public RuleTriggerBound(RuleTrigger trigger)
	{
		Id = trigger.Name.Trim().ToIdStandard();
		Name = trigger.Name;
		CommandType = trigger.CommandType;
		Status = 0;
	}

	/// <summary>
	/// The id of the trigger
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// The type of command
	/// </summary>
	public CommandType CommandType { get; init; }

	/// <summary>
	/// The name of the trigger
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// The condition for the trigger
	/// </summary>
	public RuleParameterBound Condition { get; set; }

	/// <summary>
	/// The value for the trigger
	/// </summary>
	public RuleParameterBound Value { get; set; }

	/// <summary>
	/// The command point for the trigger
	/// </summary>
	public RuleParameterBound Point { get; set; }

	/// <summary>
	/// The external id of the twin
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The connector id of the twin
	/// </summary>
	public string ConnectorId { get; set; }

	/// <summary>
	/// The twin id for command triggers
	/// </summary>
	public string TwinId { get; set; }

	/// <summary>
	/// The twin name for command triggers
	/// </summary>
	public string TwinName { get; set; }

	/// <summary>
	/// Additional twin relationship information sent to command
	/// </summary>
	public IList<RuleTriggerBoundRelationship> Relationships { get; set; } = new List<RuleTriggerBoundRelationship>();

	/// <summary>
	/// The status of the bound trigger
	/// </summary>
	public RuleInstanceStatus Status { get; set; }

	/// <summary>
	/// Gets all bound parameters for the trigger
	/// </summary>
	/// <returns></returns>
	public IEnumerable<RuleParameterBound> GetBoundParameters()
	{
		yield return Condition;
		yield return Value;
		yield return Point;
	}
}

/// <summary>
/// Additional twin relationship information sent to command
/// </summary>
public class RuleTriggerBoundRelationship
{
	/// <summary>
	/// Gets or sets the ID of the twin at the other end of the relationship.
	/// </summary>
	public string TwinId { get; init; }

	/// <summary>
	/// Gets or sets the name of the twin at the other end of the relationship.
	/// </summary>
	public string TwinName { get; init; }

	/// <summary>
	/// Gets or sets the model ID of the twin at the other end of the relationship.
	/// </summary>
	public string ModelId { get; init; }

	/// <summary>
	/// Gets or sets the type of relationship between the two twins.
	/// </summary>
	/// <example>
	/// - isCapabilityOf
	/// - hostedBy
	/// - locatedIn.
	/// </example>
	public string RelationshipType { get; init; }
}
