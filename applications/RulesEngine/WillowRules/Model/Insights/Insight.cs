using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Repository;
using WillowRules.Extensions;


// EF
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// The status of the insight
/// </summary>
public enum InsightStatus
{
	Open = 0,
	Ignored = 1,
	InProgress = 2,
	Resolved = 3,
	New = 4,
	Deleted = 5,
}

/// <summary>
/// An internal insight
/// </summary>
/// <remarks>
/// These may then be pushed to Command using the Insights API on command
/// </remarks>
public class Insight : IId, IWillowStandardRule
{
	/// <summary>
	/// The Id for persistence
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// The overall text message summarizing what's in the insight, may include newline characters
	/// which will be rendered as HTML paragraphs or otherwise
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// Rule description
	/// </summary>
	/// <remarks>
	/// This is copied from the Rule at the time the Insight is created (denormalized)
	/// </remarks>
	public string RuleDescription { get; set; }

	/// <summary>
	/// Rule recommendations
	/// </summary>
	/// <remarks>
	/// This is copied from the Rule at the time the Insight is created (denormalized)
	/// </remarks>
	public string RuleRecomendations { get; set; }

	/// <summary>
	/// List of tags associated to the Rule
	/// </summary>
	/// <remarks>
	/// This is copied from the RuleInstance at the time the Insight is created (denormalized)
	/// </remarks>
	public virtual IList<string> RuleTags { get; private set; } = new List<string>();

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	/// <remarks>
	/// This is suitable for filtering not for display, use a graph query for display purposes, this is flattened
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<TwinLocation> TwinLocations { get; set; }

	/// <summary>
	/// Feeds according to isFedBy relationship (and zones to rooms mapping)
	/// </summary>
	/// <remarks>
	/// This is suitable for filtering not for display, use a graph query for display purposes, this is flattened
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<string> Feeds { get; set; }

	/// <summary>
	/// Fed by according to isFedBy relationship
	/// </summary>
	/// <remarks>
	/// This is suitable for filtering not for display, use a graph query for display purposes, this is flattened
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<string> FedBy { get; set; }

	/// <summary>
	/// Occurrences that goes to its own table
	/// </summary>
	public virtual IList<InsightOccurrence> Occurrences { get; set; } = new List<InsightOccurrence>(0);

	/// <summary>
	/// Dependencies to other insights
	/// </summary>
	public virtual IList<InsightDependency> Dependencies { get; set; } = new List<InsightDependency>(0);

	/// <summary>
	/// The Rule Instance that created this insight
	/// </summary>
	public string RuleInstanceId { get; set; }

	/// <summary>
	/// The Rule that created this insight
	/// </summary>
	public string RuleId { get; set; }

	/// <summary>
	/// The model id of the primary equipment item
	/// </summary>
	public string PrimaryModelId { get; set; }

	/// <summary>
	/// The time zone of the primary equipment in the rule that created this insight
	/// </summary>
	/// <remarks>
	/// Currently settable because we have some legacy instances to update
	/// </remarks>
	public string TimeZone { get; set; }

	/// <summary>
	/// The rule name that created this insight
	/// </summary>
	public string RuleName { get; set; }

	/// <summary>
	/// The rule category from the origimal rule
	/// </summary>
	public string RuleCategory { get; set; }

	/// <summary>
	/// The rule template that created this insight
	/// </summary>
	public string RuleTemplateName { get; set; }

	/// <summary>
	/// The equipment item that is most likely the cause of this fault
	/// </summary>
	public string EquipmentName { get; set; }

	/// <summary>
	/// The equipment item that is most likely the cause of this fault
	/// </summary>
	public string EquipmentId { get; set; }

	/// <summary>
	/// Unique Id (legacy Command requirement)
	/// </summary>
	public Guid? EquipmentUniqueId { get; set; }

	/// <summary>
	/// The legacy SiteId necessary for posting insights to Command
	/// </summary>
	public Guid? SiteId { get; set; }

	/// <summary>
	/// Last updated to fail or success
	/// </summary>
	public DateTimeOffset LastUpdated { get; set; }

	/// <summary>
	/// The recent impact scores
	/// </summary>
	/// <remarks>
	/// For example, comfort. Discomfort level could be degrees above setpoint, humidity beyond comfortable range,
	/// wait times for elevator, ... normalized to some scale.		///
	/// </remarks>
	public ICollection<ImpactScore> ImpactScores { get; set; }

	/// <summary>
	/// The capability references
	/// </summary>
	public IList<NamedPoint> Points { get; set; }

	/// <summary>
	/// How many trend values the rule has received in aggregate since starting
	/// </summary>
	public long Invocations { get; set; }

	/// <summary>
	/// Insight is enabled for posting to command
	/// </summary>
	public bool CommandEnabled { get; set; }

	/// <summary>
	/// The Id of the insight in Command after first posting it, for updates
	/// </summary>
	/// <remarks>
	/// Starts as Guid.Empty, after first post this field is set and others need to be updates
	/// </remarks>
	public Guid CommandInsightId { get; set; }

	/// <summary>
	/// Earliest faulted date
	/// </summary>
	public DateTimeOffset EarliestFaultedDate { get; set; }

	/// <summary>
	/// Latest faulted date
	/// </summary>
	public DateTimeOffset LastFaultedDate { get; set; }

	/// <summary>
	/// The next date which the Insight is allowed to sync to command
	/// </summary>
	/// <remarks>
	/// For example when a user in command sets the insight to "Ignored" the insight may only sync again after the rule's window period has past since the user update
	/// </remarks>
	public DateTimeOffset NextAllowedSyncDateUTC { get; set; }

	/// <summary>
	/// Currently in faulted state
	/// </summary>
	public bool IsFaulty { get; set; }

	/// <summary>
	/// Indicator whether a standard rule for willow
	/// </summary>
	public bool IsWillowStandard { get; set; }

	/// <summary>
	/// Currently in valid state
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// The status of the insight
	/// </summary>
	public InsightStatus Status { get; set; } = InsightStatus.Open;

	/// <summary>
	/// How many times has it faulted
	/// </summary>
	public int FaultedCount { get; set; }

	/// <summary>
	/// Last updated date processed
	/// </summary>
	public DateTimeOffset LastUpdatedUTC { get; set; }

	/// <summary>
	/// Last date syned with Command
	/// </summary>
	public DateTimeOffset LastSyncDateUTC { get; set; }

	/// <summary>
	/// Serialization constructor
	/// </summary>
	public Insight()
	{
	}

	/// <summary>
	/// Creates a new <see cref="Insight"/>
	/// </summary>
	public Insight(
		RuleInstance ruleInstance,
		ActorState actor)
		: this(ruleInstance)
	{
		UpdateValues(actor, ruleInstance);
		UpdateOccurrences(actor, ruleInstance);
		UpdateImpactScores(actor, ruleInstance);
	}

	/// <summary>
	/// Creates a new <see cref="Insight"/>
	/// </summary>
	public Insight(
		RuleInstance ruleInstance)
	{
		this.Id = ruleInstance.Id ?? throw new ArgumentNullException("rulesInstance.Id");  // unique id
		this.EquipmentId = ruleInstance.EquipmentId;
		this.EquipmentName = ruleInstance.EquipmentName;
		this.SiteId = ruleInstance.SiteId;
		this.EquipmentUniqueId = ruleInstance.EquipmentUniqueId;
		this.RuleInstanceId = ruleInstance?.Id ?? throw new ArgumentNullException(nameof(ruleInstance));
		this.RuleId = ruleInstance.RuleId ?? throw new ArgumentNullException(nameof(ruleInstance));
		this.PrimaryModelId = ruleInstance.PrimaryModelId;
		this.RuleCategory = ruleInstance.RuleCategory;
		this.RuleTemplateName = ruleInstance.RuleTemplate ?? "Rule Template missing on Instance";
		this.CommandEnabled = ruleInstance.CommandEnabled;
		this.Occurrences = Array.Empty<InsightOccurrence>();
		this.CommandInsightId = Guid.Empty;
		this.TwinLocations = ruleInstance.TwinLocations;
		this.FedBy = ruleInstance.FedBy;
		this.Feeds = ruleInstance.Feeds;
		this.PrimaryModelId = ruleInstance.PrimaryModelId;
		this.TimeZone = ruleInstance.TimeZone;
		this.IsWillowStandard = ruleInstance.IsWillowStandard;
		this.ImpactScores = Array.Empty<ImpactScore>();
		this.RuleName = ruleInstance.RuleName;
		this.RuleTags = ruleInstance.RuleTags;
		this.Points = ruleInstance.PointEntityIds;

		SetDependencies(ruleInstance);
	}

	/// <summary>
	/// Indicates whether an insight can be resolved
	/// </summary>
	/// <returns></returns>
	public bool CanResolve()
	{
		return !IsFaulty && (Status == InsightStatus.InProgress || Status == InsightStatus.Open || Status == InsightStatus.New);
	}

	/// <summary>
	/// Indicates whether an insight can be reopened depending on its state
	/// </summary>
	/// <returns></returns>
	public bool CanReOpen()
	{
		return (Status == InsightStatus.Resolved || Status == InsightStatus.Ignored || Status == InsightStatus.Deleted) && IsFaulty;
	}

	/// <summary>
	/// Updates sync related values after the insight has synced
	/// </summary>
	public void InsightSynced(InsightStatus status, Guid commandId)
	{
		Status = status;
		CommandInsightId = commandId;
		LastSyncDateUTC = DateTime.UtcNow;
	}

	/// <summary>
	/// Indicates whether the insight should sync
	/// </summary>
	public bool ShouldSync()
	{
		if (!CommandEnabled)
		{
			return false;
		}

		//only invalid entries don't qualify
		//but at least one valid entry qualifies further checks
		if (!Occurrences.Any() || Occurrences.All(v => !v.IsValid))
		{
			return false;
		}

		//if never sync, sync at least once
		//(even if the last occurrence was invalid)
		if (LastSyncDateUTC == default)
		{
			return true;
		}

		//now that the insight has synced at least once,
		//don't sync invalid
		if(!IsValid)
		{
			return false;
		}

		//faulity syncs immediately
		if (IsFaulty)
		{
			return true;
		}

		var lastSyncSpan = DateTime.UtcNow - LastSyncDateUTC;

		//always sync valid insights at least every [x]hours
		if (lastSyncSpan > TimeSpan.FromHours(6))
		{
			return true;
		}

		var lastOccurrence = Occurrences.Last();

		var occurrenceSpan = lastOccurrence.Ended - lastOccurrence.Started;

		//only sync if last sync date is over [x]% of the last occurrence time range
		if (lastSyncSpan / occurrenceSpan > 0.25)
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Update an insight to the latest values from actor state
	/// </summary>
	public void UpdateValues(ActorState actorState, RuleInstance ruleInstance)
	{
		this.FaultedCount = actorState.OutputValues.FaultedCount;
		var last = actorState.OutputValues.Points.LastOrDefault();

		if (FaultedCount > 0)
		{
			this.Text = actorState.OutputValues.LastFaultedValue.GetOutputText(actorState, ruleInstance.Description);
			this.RuleRecomendations = actorState.OutputValues.LastFaultedValue.GetOutputText(actorState, ruleInstance.Recommendations);
		}
		else
		{
			//if this is invalid, the output text logic should remove both faulty and non-faulty areas and leave the rest
			this.RuleRecomendations = last.GetOutputText(actorState, ruleInstance.Recommendations);

			if (last.IsValid)
			{
				this.Text = last.GetOutputText(actorState, ruleInstance.Description);
			}
			else
			{
				this.Text = ruleInstance.Description;
			}
		}

		this.LastUpdated = actorState.Timestamp;
		this.EarliestFaultedDate = actorState.OutputValues.FirstFaultedTime;
		this.LastFaultedDate = actorState.OutputValues.LastFaultedValue.EndTime;
		this.IsFaulty = last.Faulted;
		this.IsValid = last.IsValid;
		this.Invocations = actorState.TriggerCount;
	}

	/// <summary>
	/// Updates insight occurrences from actor
	/// </summary>
	public void UpdateOccurrences(ActorState actor, RuleInstance ruleInstance)
	{
		//limit occurrences. if occurrences are required for updates during execution (or for debugging purposes), use actor outputs which has the full list
		Occurrences = actor.OutputValues.Points.ToInsightOccurrences(this, actor, ruleInstance).ToArray();
	}

	/// <summary>
	/// Updates impact scores from actor and ruleinstance
	/// </summary>
	public void UpdateImpactScores(ActorState actor, RuleInstance ruleInstance)
	{
		ImpactScores = actor.CreateImpactScores(this, ruleInstance).ToArray();
	}

	/// <summary>
	/// Updates dependencies from ruleinstance
	/// </summary>
	public void SetDependencies(RuleInstance ruleInstance)
	{
		Dependencies = ruleInstance.RuleDependenciesBound.Select(v => new InsightDependency(v.Relationship, v.RuleInstanceId)).ToArray();
	}

	/// <summary>
	/// Check for overlapping Occurrences
	/// </summary>
	public bool HasOverlappingOccurrences()
	{
		return this.Occurrences.HasConsecutiveCondition((previous, current) => previous.Ended > current.Started);
	}
}
