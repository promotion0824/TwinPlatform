using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Willow.Rules.Repository;

// POCO class serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// The type of command
/// </summary>
public enum CommandType
{
	Set = 1,
	AtMost = 2,
	AtLeast = 3,
}

/// <summary>
/// An internal command
/// </summary>
public class Command : IId
{
	/// <summary>
	/// The Id for persistence which is the rule instance id + the command id
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// The Command Id is generated from the name for internal purposes, eg used in actor timed values
	/// </summary>
	public string CommandId { get; set; }

	/// <summary>
	/// The Name for the Command
	/// </summary>
	public string CommandName { get; set; }

	/// <summary>
	/// The type of command
	/// </summary>
	public CommandType CommandType { get; set; }

	/// <summary>
	/// The external id of the twin
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The connector id of the twin
	/// </summary>
	public string ConnectorId { get; set; }

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
	/// Units of the value associated with this command's point
	/// </summary>
	public string Unit { get; set; }

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
	/// The timezone for the rule instance
	/// </summary>
	public string TimeZone { get; set; }

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
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// An optional end time for the command
	/// </summary>
	public DateTimeOffset? EndTime { get; set; }

	/// <summary>
	/// A UTC timestamp that audits the last sync date to command
	/// </summary>
	public DateTimeOffset? LastSyncDate { get; set; }

	/// <summary>
	/// Occurrences
	/// </summary>
	public IList<CommandOccurrence> Occurrences { get; set; }

	/// <summary>
	/// Indicates whether the command is enabled to sync
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Indicates whether the command is currently valid
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// The model id of the primary equipment item
	/// </summary>
	public string PrimaryModelId { get; set; }

	/// <summary>
	/// Additional twin relationship information sent to command
	/// </summary>
	public IList<RuleTriggerBoundRelationship> Relationships { get; set; }

	/// <summary>
	/// Indicates whether the command can sync to command and control
	/// </summary>
	/// <returns></returns>
	public bool CanSync()
	{
		return Enabled = true && !string.IsNullOrEmpty(ExternalId) && !string.IsNullOrEmpty(ConnectorId);
	}

	/// <summary>
	/// Serialization constructor
	/// </summary>
	[JsonConstructor]
	public Command()
	{
	}

	/// <summary>
	/// Constructor
	/// </summary>
	public Command(
		RuleTriggerBound trigger,
		RuleInstance ruleInstance,
		ActorState actor)
	{
		Id = $"{ruleInstance.Id}_{trigger.Id}";
		Enabled = ruleInstance.CommandEnabled;
		TimeZone = ruleInstance.TimeZone;
		PrimaryModelId = ruleInstance.PrimaryModelId;
		CommandId = trigger.Id;
		CommandName = trigger.Name;
		CommandType = trigger.CommandType;
		ExternalId = trigger.ExternalId;
		ConnectorId = trigger.ConnectorId;
		TwinId = trigger.TwinId;
		TwinName = trigger.TwinName;
		Unit = trigger.Value.Units;
		EquipmentId = ruleInstance.EquipmentId;
		EquipmentName = ruleInstance.EquipmentName;
		RuleId = ruleInstance.RuleId;
		RuleInstanceId = ruleInstance.Id;
		RuleName = ruleInstance.RuleName;
		Relationships = trigger.Relationships;
		Occurrences = Array.Empty<CommandOccurrence>();
		IsValid = false;

		if (actor.OutputValues.Commands.TryGetValue(CommandId, out var output))
		{
			IsValid = actor.IsValid;
			var last = output.Points.LastOrDefault();
			IsTriggered = last.Triggered;
			Value = last.Value;
			StartTime = last.TriggerStartTime;
			EndTime = last.TriggerEndTime;
			Occurrences = output.Points.ToCommandOccurrences(int.MaxValue).ToArray();
		}
	}
}
