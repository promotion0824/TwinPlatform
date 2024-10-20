using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Willow.Expressions;
using Willow.Rules.Repository;

// POCO class serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// How the Calculated Point was created
/// </summary>
public enum CalculatedPointSource
{
	/// <summary>Manually created in ADT</summary>
	ADT = 0,
	/// <summary>Created using the RulesEngine</summary>
	RulesEngine = 1,
}

/// <summary>
/// What action should be performed on the Calculated Point Twin
/// </summary>
public enum ADTActionRequired
{
	/// <summary>No action needed</summary>
	None = 0,
	/// <summary>Create or update in ADT</summary>
	Upsert = 1,
	/// <summary>Delete in ADT and Rules Engine</summary>
	Delete = 2,
}

/// <summary>
/// The status of the action performed on the Calculated Point Twin
/// </summary>
public enum ADTActionStatus
{
	/// <summary>Twin exist in ADT</summary>
	TwinAvailable = 0,
	/// <summary>No Twin in ADT</summary>
	NoTwinExist = 1,
	/// <summary>No Twin in ADT</summary>
	Failed = 2
}

/// <summary>
/// A calculated point comes directly from ADT, a simple copy of what's there, later it gets expanded to a RuleInstance
/// </summary>
public class CalculatedPoint : IId
{
	/// <summary>
	/// Primary key, also the TwinId that contained the valueExpression
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// The output trend id for calculated points created manually in ADT
	/// </summary>
	public string TrendId { get; set; }

	/// <summary>
	/// The value expression specified for calculated points created manually in ADT
	/// </summary>
	public string ValueExpression { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// Model of the calculated point, usually a Sensor dervived class
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// The model ID the PrimaryModelId is related to
	/// </summary>
	public string IsCapabilityOf { get; set; }

	/// <summary>
	/// Name of the twin
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description from the twin
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The date which the rule was last updated
	/// </summary>
	public DateTimeOffset LastUpdated { get; set; }

	/// <summary>
	/// Calculated Point Twin ID (Same as Id)
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The rule the Calculated Point was created from
	/// </summary>
	public string RuleId { get; set; }

	/// <summary>
	/// Calculated = Min(TrendInterval of referenced capabilities) - guaranteed trend interval.
	/// </summary>
	public int TrendInterval { get; set; }

	/// <summary>
	/// Timezone for the primary equipment in the rule instance
	/// </summary>
	public string TimeZone { get; set; }

	/// <summary>
	/// Calculated Point Twin ID (Same as Id)
	/// </summary>
	public string ConnectorID { get; set; }

	/// <summary>
	/// The legacy site ID needed on created capabilities (Requirement in Command to be displayed on Time Series)
	/// </summary>
	public Guid? SiteId { get; set; }

	/// <summary>
	/// The type of calculated Point
	/// </summary>
	public UnitOutputType Type { get; set; }

	/// <summary>
	/// How the Calculated Point was created
	/// </summary>
	public CalculatedPointSource Source { get; set; }

	/// <summary>
	/// What action should be performed on the Calculated Point Twin
	/// </summary>
	public ADTActionRequired ActionRequired { get; set; }

	/// <summary>
	/// The status of the action performed on the Calculated Point Twin
	/// </summary>
	public ADTActionStatus ActionStatus { get; set; }

	/// <summary>
	/// Last date syned with ADT
	/// </summary>
	public DateTimeOffset LastSyncDateUTC { get; set; }

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	public IList<TwinLocation> TwinLocations { get; set; } = new List<TwinLocation>(0);

	/// <inheritdoc />
	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}
}
