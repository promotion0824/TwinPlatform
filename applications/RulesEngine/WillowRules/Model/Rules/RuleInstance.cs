using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Willow.Rules.Repository;
using WillowRules.Extensions;

// POCO class serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// Rule Instance validity statuses
/// </summary>
[Flags]
public enum RuleInstanceStatus
{
	/// <summary>Instance has valid binding expression and/or capabilities</summary>
	Valid = 1,
	/// <summary>Instance could not be bound. Verify binding expression</summary>
	BindingFailed = 2,
	/// <summary>Instance could not be bound. Verify twin capabilities</summary>
	NonCommissioned = 4,
	/// <summary>One or more expression filters failed. Verify rule filter expressions</summary>
	FilterFailed = 8,
	/// <summary>One or more expression filters were applied. Verify rule filter expressions</summary>
	FilterApplied = 16,
	/// <summary>This instance has an expression expecting a single value result but is an array. Wrap the expression in an Aggregation function to get a single result</summary>
	ArrayUnexpected = 32
}

/// <summary>
/// A rule instance applies a rule to a small sub-graph (probably just a single piece of equipment and a few points on it in V1)
/// </summary>
/// <remarks>
/// In V2 the subgraph might be more complex, like a floor and all of the temperature sensors on it.
/// </remarks>
public class RuleInstance : IId, IWillowStandardRule
{
	/// <summary>
	/// A filter that can be used to read rule instances applicable for execution
	/// </summary>
	public static readonly Expression<Func<RuleInstance, bool>> ExecutableInstanceFilter = v => v.Status == RuleInstanceStatus.Valid;

	/// <summary>
	/// Primary key in RuleInstance table
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// The parent rule that was used to form this rule instance
	/// </summary>
	public string RuleId { get; set; }

	/// <summary>
	/// The parent rule name that was used to form this rule instance [denormalized]
	/// </summary>
	public string RuleName { get; set; }

	/// <summary>
	/// The rule template (id) that created the rule that created this instance
	/// </summary>
	public string RuleTemplate { get; set; }

	/// <summary>
	/// For debugging
	/// </summary>
	public string PrimaryModelId { get; set; }

	/// <summary>
	/// The model ID the PrimaryModelId is related to
	/// </summary>
	public string RelatedModelId { get; set; }

	/// <summary>
	/// Used to keep the descripiton of the rule and any equipment bound properties, e.g. {this.fanSpeed}
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Rule recommendations
	/// </summary>
	/// <remarks>
	/// This is copied from the Rule
	/// </remarks>
	public string Recommendations { get; set; }

	/// <summary>
	/// List of tags associated to the Rule
	/// </summary>
	/// <remarks>
	/// This is copied from the Rule
	/// </remarks>
	public virtual IList<string> RuleTags { get; init; } = new List<string>(0);

	/// <summary>
	/// The date which the rule was last updated
	/// </summary>
	public DateTimeOffset LastUpdated { get; set; }

	/// <summary>
	/// These are all the points that this rule cares about
	/// </summary>
	public IList<NamedPoint> PointEntityIds { get; set; } = new List<NamedPoint>(0);

	/// <summary>
	/// Expressions bound to trendIds
	/// </summary>
	/// <remarks>
	/// A simple rule will have just one of these, a more complex rule may have many points it monitors on one or more equipment items
	/// </remarks>
	public IList<RuleParameterBound> RuleParametersBound { get; init; } = new List<RuleParameterBound>(0);

	/// <summary>
	/// Filters bound to this instance
	/// </summary>
	/// <remarks>
	/// If any filter failed for the rule instance it is flagged as invalid
	/// </remarks>
	public IList<RuleParameterBound> RuleFiltersBound { get; init; } = new List<RuleParameterBound>(0);

	/// <summary>
	/// Expressions bound to impact scores
	/// </summary>
	/// <remarks>
	/// Impact scores may not depend on other impact scores for its result as this will create a circular reference
	/// </remarks>
	public IList<RuleParameterBound> RuleImpactScoresBound { get; init; } = new List<RuleParameterBound>(0);

	/// <summary>
	/// A set if rule instance dependencies for this rule instance
	/// </summary>
	public IList<RuleDependencyBound> RuleDependenciesBound { get; init; } = new List<RuleDependencyBound>(0);

	/// <summary>
	/// A set if rule triggers for this rule instance
	/// </summary>
	public IList<RuleTriggerBound> RuleTriggersBound { get; init; } = new List<RuleTriggerBound>(0);

	/// <summary>
	/// The count of rule instance dependencies (used on search grids)
	/// </summary>
	public int RuleDependencyCount { get; init; }

	/// <summary>
	/// Indicates validity status - If not valid then we were unable to bind all the variables to the equipment
	/// </summary>
	public RuleInstanceStatus Status { get; set; }

	/// <summary>
	/// User has disabled this rule, do not report insights based on it running
	/// </summary>
	public bool Disabled { get; set; }

	/// <inheritdoc />
	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	/// <summary>
	/// Equipment id for the primary fault
	/// </summary>
	public string EquipmentId { get; set; }

	/// <summary>
	/// Equipment name for the primary fault
	/// </summary>
	public string EquipmentName { get; set; }

	/// <summary>
	/// The legacy SiteId necessary for posting insights to Command
	/// </summary>
	public Guid? SiteId { get; set; }

	/// <summary>
	/// Unique Id (legacy for command, single equipment insights)
	/// </summary>
	/// <remarks>
	/// Can we remove this now?
	/// </remarks>
	public Guid? EquipmentUniqueId { get; set; }

	/// <summary>
	/// Timezone for the primary equipment in the rule instance
	/// </summary>
	/// <remarks>
	/// A rule could pull information from capabilities in different timezones(!)
	/// but we need just one to report on.
	/// </remarks>
	public string TimeZone { get; set; }

	/// <summary>
	/// RuleInstance is enabled for posting insights to command
	/// </summary>
	public bool CommandEnabled { get; set; }

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	/// <remarks>
	/// This is a flattened list sorted in the ascending direction.
	/// It is suitable for filtering but not for display: use a graph query for display purposes.
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<TwinLocation> TwinLocations { get; set; } = new List<TwinLocation>(0);

	/// <summary>
	/// Feeds according to isFedBy relationship (and zones to rooms mapping)
	/// </summary>
	/// <remarks>
	/// This is a flattened list sorted in the feeds direction.
	/// It is suitable for filtering but not for display: use a graph query for display purposes.
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<string> Feeds { get; set; } = new List<string>(0);

	/// <summary>
	/// Fed by according to isFedBy relationship
	/// </summary>
	/// <remarks>
	/// This is a flattened list sorted in the fed-by direction.
	/// It is suitable for filtering but not for display: use a graph query for display purposes.
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public virtual IList<string> FedBy { get; set; } = new List<string>(0);

	/// <summary>
	/// For calculated points there is an output trend Id (Guid)
	/// </summary>
	public string OutputTrendId { get; set; }

	/// <summary>
	/// For calculated points there is an output External Id (Guid)
	/// </summary>
	public string OutputExternalId { get; set; }

	/// <summary>
	/// The rule category from the rule
	/// </summary>
	public string RuleCategory { get; set; }

	/// <summary>
	/// Indicator whether a standard rule for willow
	/// </summary>
	public bool IsWillowStandard { get; set; }

	/// <summary>
	/// Total number of capabilities linked to the equipment
	/// </summary>
	public int CapabilityCount { get; set; }

	/// <summary>
	/// Extracts all possible variables in the description
	/// </summary>
	/// <returns></returns>
	public List<string> ParseVariables()
	{
		return StringExtensions.ExtractExpressionsFromText(Description).Union(StringExtensions.ExtractExpressionsFromText(Recommendations)).Distinct().ToList();
	}

	/// <summary>
	/// Gets all bound parameters for the rule instance
	/// </summary>
	public IEnumerable<RuleParameterBound> GetAllBoundParameters()
	{
		return RuleParametersBound
				.Concat(RuleImpactScoresBound)
				.Concat(RuleTriggersBound.SelectMany(v => v.GetBoundParameters()));
	}
}
