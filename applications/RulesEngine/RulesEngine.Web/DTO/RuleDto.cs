using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

// POCO class
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// A rule with associated metadata in a DTO for consumption by client-side code
/// </summary>
public class RuleDto
{
    /// <summary>
    /// Primary key for <see cref="Rule"/> table
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// A human-readable name for a <see cref="Rule"/>
    /// </summary>
    public string Name { get; init; }

    ///<summary>
    /// A language dictionary that contains the localized name
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageNames { get; set; }

    /// <summary>
    /// Category (singular)
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Description of the rule
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicator whether a standard rule for willow
    /// </summary>
    public bool IsWillowStandard { get; set; }

    ///<summary>
    ///     A language dictionary that contains the localized description
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; }

    /// <summary>
    /// Recommendations for fixing the fault
    /// </summary>
    public string Recommendations { get; set; }

    ///<summary>
    ///     A language dictionary that contains the localized recommendations
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageRecommendations { get; set; }

    /// <summary>
    /// Tags
    /// </summary>
    public string[] Tags { get; init; }

    /// <summary>
    /// A query that returns a set of sub-graphs each of which contains one or more equipment items and the capability points of interest
    /// </summary>
    /// <remarks>
    /// This replaces Class in the SQL Rules as the way to select what entities and points should participate in a rule
    /// </remarks>
    public RuleTwinQueryDto TwinQuery { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }

    /// <summary>
    /// In a later version, rules will themselves be part of a directed acyclic graph allowing for fault-tree analysis
    /// </summary>
    public List<string> ParentIds { get; init; }

    /// <summary>
    /// The rule template for this rule
    /// </summary>
    public string TemplateId { get; init; }

    /// <summary>
    /// The points that this rule uses
    /// </summary>
    public RuleParameterDto[] Parameters { get; init; } = [];

    /// <summary>
    /// Impacts scores for cost, comfort, reliability, etc.
    /// </summary>
    public RuleParameterDto[] ImpactScores { get; init; } = [];

    /// <summary>
    /// A set of filters to exlude twins on expansion
    /// </summary>
    public RuleParameterDto[] Filters { get; init; } = [];

    /// <summary>
    /// Constant UI elements
    /// </summary>
    public RuleUIElementDto[] Elements { get; init; }

    /// <summary>
    /// A set rule dependencies for this rule
    /// </summary>
    public RuleDependencyDto[] Dependencies { get; init; }

    /// <summary>
    /// A set rule triggers for this rule
    /// </summary>
    public RuleTriggerDto[] RuleTriggers { get; init; }

    /// <summary>
    /// Metadata about the rule, how many instances it has, ...
    /// </summary>
    public RuleMetadataDto RuleMetadata { get; init; }

    /// <summary>
    /// Json serialized version for export / import
    /// </summary>
    public string Json { get; init; }

    /// <summary>
    /// The primary model id that roots the sub-graph to a system of equipment items
    /// </summary>
    public string PrimaryModelId { get; set; }

    /// <summary>
	/// The model ID the PrimaryModelId is related to
	/// </summary>
	public string RelatedModelId { get; init; }

    /// <summary>
    /// Rule is enabled for posting insights to command
    /// </summary>
    public bool CommandEnabled { get; init; }

    /// <summary>
    /// Indicator that a rule is in Draft mode
    /// </summary>
    public bool IsDraft { get; init; }

    /// <summary>
    /// Indicator that a rule is used for Calculated Point
    /// </summary>
    public bool IsCalculatedPoint { get; init; }

    /// <summary>
	/// Rule is enabled to manage calculated point twins to ADT
	/// </summary>
    public bool ADTEnabled { get; init; }

    /// <summary>
    /// The rule template for display purposes
    /// </summary>
    public string TemplateName { get; init; }

    /// <summary>
    /// Optionally polcy decisions
    /// </summary>
    public AuthenticatedUserAndPolicyDecisionsDto Policies { get; init; }

    /// <summary>
    /// Creates a new <see cref="RuleDto"/>
    /// </summary>
    public RuleDto(Rule rule, RuleMetadata ruleMetadata, bool canViewRule, AuthenticatedUserAndPolicyDecisionsDto policies = null)
    {
        this.Id = rule.Id;
        this.Name = rule.Name;
        this.PrimaryModelId = rule.PrimaryModelId;
        this.RelatedModelId = rule.RelatedModelId;
        this.Policies = policies;

        if (canViewRule)
        {
            this.Parameters = rule.Parameters
                .Select(p => new RuleParameterDto(p)).ToArray();
            this.ImpactScores = rule.ImpactScores
                .Select(p => new RuleParameterDto(p)).ToArray();
            this.Filters = rule.Filters
                .Select(p => new RuleParameterDto(p)).ToArray();
            this.Json = JsonConvert.SerializeObject(rule, jsonSettings);
        }

        this.Dependencies = rule.Dependencies
            .Select(p => new RuleDependencyDto(p)).ToArray();
        this.RuleTriggers = rule.RuleTriggers
            .Select(p => new RuleTriggerDto(p)).ToArray();
        this.ParentIds = rule.ParentIds.Select(x => x.Id).ToList();
        this.TemplateId = rule.TemplateId;
        this.TwinQuery = new RuleTwinQueryDto(rule.PrimaryModelId);
        this.Elements = rule.GetRuleElements().ToArray();
        this.RuleMetadata = ruleMetadata is RuleMetadata ? new RuleMetadataDto(ruleMetadata, rule.GetModelIdsForRule().ToList()) : null;
        this.Tags = rule.Tags?.ToArray();
        this.Description = rule.Description;
        this.Recommendations = rule.Recommendations;
        this.LanguageNames = rule.LanguageNames;
        this.LanguageDescriptions = rule.LanguageDescriptions;
        this.LanguageRecommendations = rule.LanguageRecommendations;
        this.CommandEnabled = rule.CommandEnabled;
        this.IsDraft = rule.IsDraft;
        this.IsCalculatedPoint = rule.TemplateId == RuleTemplateCalculatedPoint.ID;
        this.ADTEnabled = rule.ADTEnabled;
        this.Category = this.IsCalculatedPoint ? "Calculated point" : rule.Category;
        this.IsWillowStandard = rule.IsWillowStandard;
    }

    /// <summary>
    /// Constructor used for deserialization
    /// </summary>
    [JsonConstructor]
    private RuleDto()
    {
    }

    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new TokenExpressionJsonConverter() },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto
    };
}
