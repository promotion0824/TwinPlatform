using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using WillowRules.Utils;

// POCO class
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// A rule, see also <see cref="RuleDto"/>
/// </summary>
/// <remarks>
/// Parts of this would be editable by users of the system
/// </remarks>
public class Rule : IId, IWillowStandardRule
{
	/// <summary>
	/// Primary key for <see cref="Rule"/> table
	/// </summary>
	[JsonProperty("id", Order = 0)]  // necessary for Cosmos DB?
	public string Id { get; set; }

	/// <summary>
	/// A human-readable name for a <see cref="Rule"/>
	/// </summary>
	[JsonProperty(Order = 1)]
	public string Name { get; set; }

	///<summary>
	///     A language dictionary that contains the localized display names
	///     e.g. "en" = "..."
	///</summary>
	///<remarks>
	/// This is just a placeholder for now so that the rules can be uploaded / downloaded with these fields
	///</remarks>
	[JsonProperty(Order = 2, ItemTypeNameHandling = TypeNameHandling.None, TypeNameHandling = TypeNameHandling.None)]
	public IReadOnlyDictionary<string, string> LanguageNames { get; set; } = new Dictionary<string, string>();

	/// <summary>
	/// A description of the rule
	/// </summary>
	[JsonProperty(Order = 3)]
	public string Description { get; set; }

	///<summary>
	/// A language dictionary that contains the localized descriptions
	///</summary>
	///<remarks>
	/// This is just a placeholder for now so that the rules can be uploaded / downloaded with these fields
	///</remarks>
	[JsonProperty(Order = 4, ItemTypeNameHandling = TypeNameHandling.None, TypeNameHandling = TypeNameHandling.None)]
	public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; } = new Dictionary<string, string>();

	/// <summary>
	/// A single category for grouping rules, e.g. AirHandler Return Air
	/// </summary>
	[JsonProperty(Order = 5)]
	public string Category { get; set; }

	/// <summary>
	/// Recommendations to cure the fault
	/// </summary>
	[JsonProperty(Order = 6)]
	public string Recommendations { get; set; }

	///<summary>
	/// A language dictionary that contains the localized recommendations
	///</summary>
	[JsonProperty(Order = 7, ItemTypeNameHandling = TypeNameHandling.None, TypeNameHandling = TypeNameHandling.None)]
	public IReadOnlyDictionary<string, string> LanguageRecommendations { get; set; } = new Dictionary<string, string>();

	/// <summary>
	/// The primary model id that roots the sub-graph to a system of equipment items
	/// </summary>
	[JsonProperty(Order = 8)]
	public string PrimaryModelId { get; set; }

    /// <summary>
    /// Tags
    /// </summary>
    [JsonProperty(Order = 9)]
	public virtual IList<string> Tags { get; set; }

	/// <summary>
	/// In a later version, rules will themselves be part of a directed acyclic graph allowing for fault-tree analysis
	/// </summary>
	[JsonProperty(Order = 10)]
	public virtual ICollection<Rule> ParentIds { get; init; } = new List<Rule>();

	/// <summary>
	/// The rule template for this rule
	/// </summary>
	[JsonProperty(Order = 11)]
	public string TemplateId { get; init; }

	/// <summary>
	/// The values set in the UI for the fields the template for this rule uses
	/// </summary>
	/// <remarks>
	/// For example, the percentage and the time period for which the trigger needs to be true
	/// </remarks>
	[JsonProperty(Order = 12)]
	public virtual IList<RuleUIElement> Elements { get; set; } = new List<RuleUIElement>();

	/// <summary>
	/// The points that this rule uses
	/// </summary>
	[JsonProperty(Order = 13)]
	public virtual IList<RuleParameter> Parameters { get; set; } = new List<RuleParameter>();

	/// <summary>
	/// Impacts scores for cost, comfort, reliability, etc.
	/// </summary>
	[JsonProperty(Order = 14)]
	public virtual IList<RuleParameter> ImpactScores { get; set; } = new List<RuleParameter>();

	/// <summary>
	/// A set of filters to exlude twins on expansion
	/// </summary>
	[JsonProperty(Order = 15)]
	public virtual IList<RuleParameter> Filters { get; set; } = new List<RuleParameter>();

	/// <summary>
	/// A set of rule dependencies for this rule
	/// </summary>
	[JsonProperty(Order = 16)]
	public virtual IList<RuleDependency> Dependencies { get; set; } = new List<RuleDependency>();

	/// <summary>
	/// The model ID the PrimaryModelId is related to
	/// </summary>
	[JsonProperty(Order = 17)]
	public string RelatedModelId { get; set; }

	/// <summary>
	/// The triggers that exist for this rule
	/// </summary>
	[JsonProperty(Order = 18)]
	public virtual IList<RuleTrigger> RuleTriggers { get; set; } = new List<RuleTrigger>();

	/// <summary>
	/// Indicator whether a standard rule for willow
	/// </summary>
	[JsonProperty(Order = 19)]
	public bool IsWillowStandard { get; set; }

	/// <summary>
	/// Rule is enabled for posting insights to command
	/// </summary>
	[JsonIgnore]
	public bool CommandEnabled { get; set; }

	/// <summary>
	/// Indicator that a rule is in Draft mode
	/// </summary>
	/// <remarks>
	/// A rule in draft mode will be excluded from real-time and batch operations
	/// </remarks>
	[JsonIgnore]
	public bool IsDraft { get; set; }

	/// <summary>
	/// Rule is enabled to manage calculated point twins to ADT
	/// </summary>
	[JsonIgnore]
	public bool ADTEnabled { get; set; }

	/// <summary>
	/// Etag is tied to version of code so rules regenerate each time you push a new version of the code
	/// </summary>
	private static readonly string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

	/// <summary>
	/// Temporary fix so rules get regenerated only when they change (or at least once a day)
	/// </summary>
	[JsonIgnore]
	public string ETag =>
		HashUtility.GetSha256Hash(
			this.PrimaryModelId + DateTimeOffset.UtcNow.Year + DateTimeOffset.UtcNow.DayOfYear +
			version +
			this.Name +
			this.Description +
			this.Category +
			this.Recommendations +
			string.Join("", this.Elements is null ? Array.Empty<string>() : this.Elements.Select(p => p.Name + ":" + p.ValueDouble + ":" + p.ValueInt + ":" + p.ValueString)) +
			string.Join("", this.Parameters is null ? Array.Empty<string>() : this.Parameters.Select(p => p.Name + ":" + p.PointExpression)) +
			string.Join("", this.Filters is null ? Array.Empty<string>() : this.Filters.Select(p => p.Name + ":" + p.PointExpression)));

	/// <summary>
	/// Empty constructor for EF
	/// </summary>
	public Rule() {}

	/// <inheritdoc />
	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	/// <summary>
	/// Empty constructor for EF
	/// </summary>
	public Rule(string name, string templateId, string primaryModelId)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
		}

		if (string.IsNullOrWhiteSpace(templateId))
		{
			throw new ArgumentException($"'{nameof(templateId)}' cannot be null or empty.", nameof(templateId));
		}

		if (string.IsNullOrWhiteSpace(primaryModelId))
		{
			throw new ArgumentException($"'{nameof(primaryModelId)}' cannot be null or empty.", nameof(primaryModelId));
		}

		name = name.Trim();
		Id = name.ToIdStandard();
		Name = name;
		TemplateId = templateId;
		PrimaryModelId = primaryModelId;
	}

	/// <summary>
	/// Helper method to create parameters from fields
	/// </summary>
	public Rule WithParameter(RuleUIElement element, string pointExpression)
	{
		var parameter = RuleParameter.Create(element.Name, element.Id, pointExpression);
		this.Parameters.Add(parameter);
		return this;
	}

	/// <summary>
	/// Helper method to create parameters from fields
	/// </summary>
	public Rule WithImpactScore(RuleUIElement element, string pointExpression)
	{
		var score = RuleParameter.Create(element.Name, element.Id, pointExpression, element.Units);
		this.ImpactScores.Add(score);
		return this;
	}

	/// <summary>
	/// Helper method to create parameters from fields
	/// </summary>
	public Rule WithImpactScore(string name, string field, string pointExpression)
	{
		var score = RuleParameter.Create(name, field, pointExpression);
		this.ImpactScores.Add(score);
		return this;
	}

	/// <summary>
	/// Helper method to create parameters from fields
	/// </summary>
	public Rule WithDefaultParameter(string friendlyName, string pointExpression)
	{
		var parameter = RuleParameter.Create(friendlyName, Fields.Result.Id, pointExpression);
		this.Parameters.Add(parameter);
		return this;
	}

	/// <summary>
	/// Helper method to create parameters from fields
	/// </summary>
	public Rule Create(string name, string pointExpression)
	{
		var parameter = RuleParameter.Create(name, name.ToLower().Replace(' ', '-'), pointExpression);
		this.Parameters.Add(parameter);
		return this;
	}
}
