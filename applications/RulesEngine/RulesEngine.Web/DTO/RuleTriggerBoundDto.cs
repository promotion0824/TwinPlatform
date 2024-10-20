using System.Linq;
using Willow.Rules.Model;

#nullable disable  // just a poco

namespace RulesEngine.Web;

/// <summary>
/// A <see cref="RuleTriggerBoundDto"/> represents an output trigger for a rule instance
/// </summary>
public class RuleTriggerBoundDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ruleTriggerBound"></param>
    public RuleTriggerBoundDto(RuleTriggerBound ruleTriggerBound)
    {
        Id = ruleTriggerBound.Id;
        Name = ruleTriggerBound.Name;
        Condition = new RuleParameterBoundDto(ruleTriggerBound.Condition);
        Value = new RuleParameterBoundDto(ruleTriggerBound.Value);
        Point = new RuleParameterBoundDto(ruleTriggerBound.Point);
        TwinId = ruleTriggerBound.TwinId;
        TwinName = ruleTriggerBound.TwinName;
        ExternalId = ruleTriggerBound.ExternalId;
        ConnectorId = ruleTriggerBound.ConnectorId;
        Status = ruleTriggerBound.Status;
        Relationships = ruleTriggerBound.Relationships?.Select(v => new RuleTriggerBoundRelationshipDto(v)).ToArray() ?? [];
    }

    /// <summary>
    /// The id of the trigger
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The name of the trigger
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The condition for the trigger
    /// </summary>
    public RuleParameterBoundDto Condition { get; set; }

	/// <summary>
	/// The value for the trigger
	/// </summary>
	public RuleParameterBoundDto Value { get; set; }

    /// <summary>
	/// The command point for the trigger
	/// </summary>
	public RuleParameterBoundDto Point { get; set; }

    /// <summary>
    /// The twin id for command triggers
    /// </summary>
    public string TwinId { get; set; }

    /// <summary>
    /// The twin name for command triggers
    /// </summary>
    public string TwinName { get; set; }

    /// <summary>
    /// The external id of the twin
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// The connector id of the twin
    /// </summary>
    public string ConnectorId { get; set; }

    /// <summary>
    /// The status of the bound trigger
    /// </summary>
    public RuleInstanceStatus Status { get; set; }

    /// <summary>
    /// Additional twin relationship information sent to command
    /// </summary>
    public RuleTriggerBoundRelationshipDto[] Relationships { get; set; }
}
