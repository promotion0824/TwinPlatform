using System;
using Newtonsoft.Json;
using Willow.Rules.Model;

#nullable disable  // just a poco

namespace RulesEngine.Web;

/// <summary>
/// Dto for A <see cref="RuleTrigger"/>
/// </summary>
public class RuleTriggerDto
{
    /// <summary>
    /// Creates a new RuleTrigger (for deserialization)
    /// </summary>
    public RuleTriggerDto(RuleTrigger ruleTrigger)
    {
        Name = ruleTrigger.Name;
        Condition = new RuleParameterDto(ruleTrigger.Condition ?? new RuleParameter());
        TriggerType = ruleTrigger.TriggerType;
        Point = new RuleParameterDto(ruleTrigger.Point ?? new RuleParameter());
        Value = new RuleParameterDto(ruleTrigger.Value ?? new RuleParameter());
        CommandType = ruleTrigger.CommandType;
    }

    /// <summary>
    /// Creates a new RuleTriggerDto (for deserialization)
    /// </summary>
    public RuleTriggerDto()
    {
    }

    /// <summary>
    /// The name of the trigger
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// A point expression that defines whether this rule trigger will be triggered
    /// </summary>
    public RuleParameterDto Condition { get; init; }

    /// <summary>
    /// The type of trigger
    /// </summary>
    public RuleTriggerType TriggerType { get; init; }

    /// <summary>
	/// The twin Id of the output setpoint twin that triggers this command
	/// </summary>
    public RuleParameterDto Point { get; init; }

    /// <summary>
    /// The value associated with this command's point
    /// </summary>
    public RuleParameterDto Value { get; init; }

    /// <summary>
    /// The <see cref="Willow.Rules.Model.CommandType"/> that defines how this command's value is interpreted
    /// </summary>
    public CommandType CommandType { get; init; }
}
