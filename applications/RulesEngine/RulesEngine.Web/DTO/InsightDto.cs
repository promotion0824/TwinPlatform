using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="Insight" />
/// </summary>
public class InsightDto
{
    /// <summary>
    /// Creates a <see cref="InsightDto" /> from an <see cref="Insight" />
    /// </summary>
    public InsightDto(Insight insight, string commandUrl = null)
    {
        this.Id = insight.Id;
        this.LastUpdated = insight.LastUpdated;
        this.Invocations = insight.Invocations;
        this.Occurrences = insight.Occurrences.OrderBy(v => v.Started).Select(x => new InsightOccurrenceDto(x)).ToArray();
        this.ImpactScores = insight.ImpactScores.Select(x => new InsightImpactDto(x)).ToArray();
        this.Dependencies = insight.Dependencies.Select(x => new InsightDependencyDto(x)).ToArray();
        this.RuleId = insight.RuleId;
        this.RuleInstanceId = insight.RuleInstanceId;
        this.PrimaryModelId = insight.PrimaryModelId;
        this.RuleName = insight.RuleName;
        this.RuleTemplate = insight.RuleTemplateName;
        this.Text = insight.Text;
        this.EquipmentId = insight.EquipmentId;
        this.EquipmentName = insight.EquipmentName;
        this.SiteId = insight.SiteId;
        this.EquipmentUniqueId = insight.EquipmentUniqueId;
        this.Locations = insight.TwinLocations;
        this.RuleTags = insight.RuleTags;
        this.FedBy = insight.FedBy;
        this.Feeds = insight.Feeds;
        this.CommandEnabled = insight.CommandEnabled;
        this.TimeZone = insight.TimeZone;
        this.CommandInsightId = insight.CommandInsightId.ToString();
        this.EarliestFaultedDate = insight.EarliestFaultedDate != DateTimeOffset.MinValue ? insight.EarliestFaultedDate : null;
        this.LastFaultedDate = insight.LastFaultedDate != DateTimeOffset.MinValue ? insight.LastFaultedDate : null;
        this.IsFaulty = insight.IsFaulty;
        this.IsValid = insight.IsValid;
        this.FaultedCount = insight.FaultedCount;
        this.Recommendations = insight.RuleRecomendations;
        this.Description = insight.RuleDescription;
        this.Status = insight.Status;
        this.LastUpdatedUTC = insight.LastUpdatedUTC;
        this.LastSyncDateUTC = insight.LastSyncDateUTC != DateTimeOffset.MinValue ? insight.LastSyncDateUTC : null;
        this.NextAllowedSyncDateUTC = insight.NextAllowedSyncDateUTC != DateTimeOffset.MinValue ? insight.NextAllowedSyncDateUTC : null;
        this.Points = [.. insight.Points.Select(point => new NamedPointDto(point))];

        if (!string.IsNullOrEmpty(commandUrl)
            && insight.SiteId is Guid siteId
            && siteId != Guid.Empty
            && insight.CommandEnabled
            && insight.CommandInsightId != Guid.Empty)
        {
            InsightUrl = $"{commandUrl.TrimEnd('/')}/sites/{siteId}/insights/{insight.CommandInsightId}";
        }
    }

    /// <summary>
    /// Id of the Insight
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Last time the insight was updated (live data time, not 'now')
    /// </summary>
    public DateTimeOffset LastUpdated { get; }

    /// <summary>
    /// The status of the insight
    /// </summary>
    public InsightStatus Status { get; }

    /// <summary>
    /// Number of invocations of the rule
    /// </summary>
    public long Invocations { get; }

    /// <summary>
    /// Insight impact scores, e.g. cost, comfort, reliability, etc.
    /// </summary>
    public ICollection<InsightImpactDto> ImpactScores { get; }

    /// <summary>
    /// Datetime ranges for which the insight was triggered
    /// </summary>
    public ICollection<InsightOccurrenceDto> Occurrences { get; }

    /// <summary>
	/// The capability references
    /// </summary>
    public NamedPointDto[] Points { get; }

    /// <summary>
    /// Dependencies to other insights
    /// </summary>
    public virtual IList<InsightDependencyDto> Dependencies { get; set; }

    /// <summary>
    /// Id of the rule that created the insight
    /// </summary>
    public string RuleId { get; }

    /// <summary>
    /// Id of the rule instance that created the insight
    /// </summary>
    public string RuleInstanceId { get; }

    /// <summary>
    /// The model of the primary equipment ID (anchor) for this rule
    /// </summary>
    public string PrimaryModelId { get; }

    /// <summary>
    /// The Id of the insight in Command after first posting it, for updates
    /// </summary>
    public string CommandInsightId { get; }

    /// <summary>
    /// Name of the rule used
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Id of the template used
    /// </summary>
    public string RuleTemplate { get; }

    /// <summary>
    /// Text of the insight
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// The twin id
    /// </summary>
    public string EquipmentId { get; }

    /// <summary>
    /// The twin name
    /// </summary>
    public string EquipmentName { get; }

    /// <summary>
    /// The twin unique id (Guid) legacy
    /// </summary>
    public Guid? EquipmentUniqueId { get; }

    /// <summary>
    /// The legacy SiteId necessary for posting insights to Command
    /// </summary>
    public Guid? SiteId { get; }

    // COPY THESE FROM THE RULE

    /// <summary>
    /// Category of the rule (e.g. Return Air))
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Description of the rule, maybe use in a tool tip
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// A link to the insight page in command
    /// </summary>
    public string InsightUrl { get; set; }

    /// <summary>
    /// Recommendations for fixing the fault
    /// </summary>
    public string Recommendations { get; set; }

    /// <summary>
    /// Locations that could be affected (search)
    /// </summary>
    public IList<TwinLocation> Locations { get; }

    /// <summary>
    /// List of tags associated to the Rule that created the Insight (search)
    /// </summary>
    public IList<string> RuleTags { get; }

    /// <summary>
    /// Downstream devices that could be affected (search)
    /// </summary>
    public IList<string> Feeds { get; }

    /// <summary>
    /// The insight is synchronized with Command
    /// </summary>
    public bool CommandEnabled { get; }

    /// <summary>
    /// The time zone of the equipment at the heart of this insight
    /// </summary>
    public string TimeZone { get; }

    /// <summary>
    /// Upstream devices that could affect (search)
    /// </summary>
    public IList<string> FedBy { get; }

    /// <summary>
    /// Earliest faulted date
    /// </summary>
    public DateTimeOffset? EarliestFaultedDate { get; }

    /// <summary>
    /// Latest faulted date
    /// </summary>
    public DateTimeOffset? LastFaultedDate { get; }

    /// <summary>
    /// Currently in faulted state
    /// </summary>
    public bool IsFaulty { get; }

    /// <summary>
    /// Currently in valid state
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// How many times has it faulted
    /// </summary>
    public int FaultedCount { get; }

    /// <summary>
    /// Last updated date processed
    /// </summary>
    public DateTimeOffset LastUpdatedUTC { get; set; }

    /// <summary>
    /// Last date syned with Command
    /// </summary>
    public DateTimeOffset? LastSyncDateUTC { get; set; }

    /// <summary>
    /// The next date which the Insight is allowed to sync to command
    /// </summary>
    public DateTimeOffset? NextAllowedSyncDateUTC { get; set; }

}
