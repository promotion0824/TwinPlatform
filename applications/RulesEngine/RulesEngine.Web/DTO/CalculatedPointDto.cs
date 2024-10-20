using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="CalculatedPoint" /> and <see cref="RuleInstance" />
/// </summary>
public class CalculatedPointDto
{
	private static readonly DateTimeOffset zero = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

	/// <summary>
	/// Creates a <see cref="CalculatedPointDto" /> from an <see cref="CalculatedPoint" />
	/// </summary>
	/// <remarks>
	/// Will expand this to include information about successful bindings, invocations, last value, ...
	/// </remarks>
	public CalculatedPointDto(CalculatedPoint calculatedPoint, RuleInstance instance, RuleInstanceMetadata metadata, ActorState actor)
	{
		this.Id = calculatedPoint.Id;
		this.Name = calculatedPoint.Name;
		this.Description = calculatedPoint.Description;
		this.ValueExpression = calculatedPoint.ValueExpression;
		this.TrendId = calculatedPoint.TrendId;
        this.TrendInterval = calculatedPoint.TrendInterval;
        this.Unit = calculatedPoint.Unit;
        this.ModelId = calculatedPoint.ModelId;
        this.IsCapabilityOf = calculatedPoint.IsCapabilityOf;
        this.ExternalId = calculatedPoint.ExternalId;
        this.RuleId = calculatedPoint.RuleId;
        this.TimeZone = calculatedPoint.TimeZone;
        this.ConnectorId = calculatedPoint.ConnectorID;
        this.SiteId = calculatedPoint.SiteId;
        this.Type = calculatedPoint.Type;
        this.Source = calculatedPoint.Source;
        this.ActionRequired = calculatedPoint.ActionRequired;
        this.ActionStatus = calculatedPoint.ActionStatus;
        this.TwinLocations = calculatedPoint.TwinLocations.ToArray();
        this.LastSyncDateUTC = calculatedPoint.LastSyncDateUTC != DateTimeOffset.MinValue ? calculatedPoint.LastSyncDateUTC : null;

        this.Valid = instance?.Status == RuleInstanceStatus.Valid;
		this.PointEntityIds = instance?.PointEntityIds.Select(v => new NamedPointDto(v)).ToList() ?? new List<NamedPointDto>();
		this.RuleParametersBound = instance?.RuleParametersBound.Select(v => new RuleParameterBoundDto(v)).ToList() ?? new List<RuleParameterBoundDto>();

		this.LastTriggered = metadata?.LastTriggered ?? new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
		this.TriggerCount = metadata?.TriggerCount ?? 0;
        this.OutputValues = actor?.OutputValues.Points.Select(v => new OutputValueDto(v)).ToList() ?? new List<OutputValueDto>();
    }

	/// <summary>
	/// Id of the calculated point, the calculated point instance and the instance metadata
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// Name of the calculated point
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// A description of the calculated point
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// The expression
	/// </summary>
	public string ValueExpression { get; }

	/// <summary>
	/// The OUTPUT trend id
	/// </summary>
	public string TrendId { get; }

	/// <summary>
	/// Did the expression parse and were the points found?
	/// </summary>
	public bool Valid { get; }

	/// <summary>
	/// The guids that trigger this instance
	/// </summary>
	public IList<NamedPointDto> PointEntityIds { get; }

	/// <summary>
	/// Rule parameter bound
	/// </summary>
	public IList<RuleParameterBoundDto> RuleParametersBound { get; }

    /// <summary>
	/// Actor output values
	/// </summary>
	public IList<OutputValueDto> OutputValues { get; }

    /// <summary>
    /// Last time a time series value was received for this calculated point
    /// </summary>
    public DateTimeOffset LastTriggered { get; }

	/// <summary>
	/// Count of how many times this calculated point has been triggered
	/// </summary>
	public int TriggerCount { get; }

    /// <summary>
	/// Unit of measure
	/// </summary>
	public string Unit { get; }

    /// <summary>
	/// Model of the calculated point, usually a Sensor dervived class
	/// </summary>
	public string ModelId { get; }

    /// <summary>
	/// The model ID the PrimaryModelId is related to
	/// </summary>
	public string IsCapabilityOf { get; }

    /// <summary>
	/// Calculated Point Twin ID (Same as Id)
	/// </summary>
	public string ExternalId { get; }

    /// <summary>
	/// Rules Engine connectorId
	/// </summary>
	public string ConnectorId { get; }

    /// <summary>
	/// The rule the Calculated Point was created from
	/// </summary>
	public string RuleId { get; }

    /// <summary>
	/// Calculated = Min(TrendInterval of referenced capabilities) - guaranteed trend interval.
	/// </summary>
	public int TrendInterval { get; }

    /// <summary>
	/// Timezone for the primary equipment in the rule instance
	/// </summary>
	public string TimeZone { get; }

    /// <summary>
	/// The legacy site ID needed on created capabilities (Requirement in Command to be displayed on Time Series)
	/// </summary>
	public Guid? SiteId { get; }

    /// <summary>
	/// The type of calculated Point
	/// </summary>
	public UnitOutputType Type { get; }

    /// <summary>
	/// How the Calculated Point was created
	/// </summary>
	public CalculatedPointSource Source { get; }

    /// <summary>
    /// What action should be performed on the Calculated Point Twin
    /// </summary>
    public ADTActionRequired ActionRequired { get; }

    /// <summary>
    /// The status of the action performed on the Calculated Point Twin
    /// </summary>
    public ADTActionStatus ActionStatus { get; }

    /// <summary>
    /// Last date syned with ADT
    /// </summary>
    public DateTimeOffset? LastSyncDateUTC { get; set; }

    /// <summary>
    /// Parent chain by locatedIn and isPartOf
    /// </summary>
    public TwinLocation[] TwinLocations { get; set; } = [];

}
