using Newtonsoft.Json;

#nullable disable  // just a poco

namespace Willow.Rules.Model;

/// <summary>
/// The type of rule trigger
/// </summary>
public enum RuleTriggerType
{
	/// <summary>
	/// Whether a <see cref="Command"/> should be triggered
	/// </summary>
	TriggerCommand = 1
}

/// <summary>
/// A <see cref="RuleTrigger"/> represents an output that a rule can generate
/// </summary>
public class RuleTrigger
{
	/// <summary>
	/// Constructor for deserialization
	/// </summary>
	public RuleTrigger()
	{

	}

	/// <summary>
	/// Constructor
	/// </summary>
	public RuleTrigger(RuleTriggerType triggerType)
	{
		TriggerType = triggerType;
	}

	/// <summary>
	/// The name of the trigger
	/// </summary>
	[JsonProperty(Order = 1)]
	public string Name { get; set; }

	/// <summary>
	/// The type of trigger
	/// </summary>
	[JsonProperty(Order = 2)]
	public RuleTriggerType TriggerType { get; init; }

	/// <summary>
	/// A point expression that defines whether this rule trigger will be triggered
	/// </summary>
	[JsonProperty(Order = 3)]
	public RuleParameter Condition { get; set; }

	/// <summary>
	/// The twin Id of the output setpoint twin that triggers this command
	/// </summary>
	[JsonProperty(Order = 4)]
	public RuleParameter Point { get; set; }

	/// <summary>
	/// The value associated with this command's point
	/// </summary>
	[JsonProperty(Order = 5)]
	public RuleParameter Value { get; set; }

	/// <summary>
	/// The <see cref="Model.CommandType"/> that defines how this command's value is interpreted
	/// </summary>
	[JsonProperty(Order = 6)]
	public CommandType CommandType { get; set; }
}
