using System;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Dto for <see cref="Command"/>
/// </summary>
public class CommandDto
{
    /// <summary>
    /// Creates a <see cref="CommandDto"/> from a <see cref="Command"/>
    /// </summary>
    public CommandDto(Command command)
    {
        this.Id = command.Id;
        this.CommandId = command.CommandId;
        this.CommandName = command.CommandName;
        this.TimeZone = command.TimeZone;
        this.PrimaryModelId = command.PrimaryModelId;
        this.CommandType = command.CommandType;
        this.Enabled = command.Enabled;
        this.ExternalId = command.ExternalId;
        this.ConnectorId = command.ConnectorId;
        this.TwinId = command.TwinId;
        this.TwinName = command.TwinName;
        this.EquipmentId = command.EquipmentId;
        this.EquipmentName = command.EquipmentName;
        this.Unit = command.Unit;
        this.RuleInstanceId = command.RuleInstanceId;
        this.RuleId = command.RuleId;
        this.RuleName = command.RuleName;
        this.IsTriggered = command.IsTriggered;
        this.Value = command.Value;
        this.StartTime = command.IsValid ? command.StartTime : null;
        this.EndTime = command.EndTime;
        this.IsValid = command.IsValid;
        this.LastSyncDate = command.LastSyncDate;
        this.Occurrences = command.Occurrences.Select(x => new CommandOccurrenceDto(x)).ToArray();
        this.Relationships = command.Relationships?.Select(v => new RuleTriggerBoundRelationshipDto(v)).ToArray() ?? [];
    }

    /// <summary>
    /// Unique Id of the command
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The ID for the Command
    /// </summary>
    public string CommandId { get; set; }

    /// <summary>
    /// Name of the command
    /// </summary>
    public string CommandName { get; }

    /// <summary>
    /// The type of command
    /// </summary>
    public CommandType CommandType { get; }

    /// <summary>
    /// The timezone for the rule instance
    /// </summary>
    public string TimeZone { get; set; }

    /// <summary>
	/// The model id of the primary equipment item
	/// </summary>
	public string PrimaryModelId { get; set; }

    /// <summary>
    /// The external id of the twin
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// The connector id of the twin
    /// </summary>
    public string ConnectorId { get; set; }

    /// <summary>
	/// The Twin Id for which the command is executed
	/// </summary>
	public string TwinId { get; set; }

    /// <summary>
    /// The Twin Name for which the command is executed
    /// </summary>
    public string TwinName { get; set; }

    /// <summary>
    /// The equipment name related to this Command
    /// </summary>
    public string EquipmentName { get; set; }

    /// <summary>
    /// The equipment id related to this Command
    /// </summary>
    public string EquipmentId { get; set; }

    /// <summary>
    /// Units of the value
    /// </summary>
    public string Unit { get; }

    /// <summary>
	/// The rule that created this command
	/// </summary>
	public string RuleId { get; set; }

    /// <summary>
    /// The rule instance id that created this command
    /// </summary>
    public string RuleInstanceId { get; set; }

    /// <summary>
    /// The rule that created this command
    /// </summary>
    public string RuleName { get; set; }

    /// <summary>
	/// Indicates whether the command is currently triggered
	/// </summary>
	public bool IsTriggered { get; set; }

    /// <summary>
    /// The current value for the command
    /// </summary>
    public double Value { get; set; }

    /// <summary>
	/// The start time for the command
	/// </summary>
	public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// An optinal end time for the command
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// A UTC timestamp that audits the last sync date to command
    /// </summary>
    public DateTimeOffset? LastSyncDate { get; set; }

    /// <summary>
    /// Occurrences
    /// </summary>
    public CommandOccurrenceDto[] Occurrences { get; set; }

    /// <summary>
    /// Indicates whether the command is enabled to sync
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Indicates whether the command is currently valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Additional twin relationship information sent to command
    /// </summary>
    public RuleTriggerBoundRelationshipDto[] Relationships { get; set; }
}
