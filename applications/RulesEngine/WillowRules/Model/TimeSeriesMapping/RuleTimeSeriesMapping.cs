using System;
using Newtonsoft.Json;
using Willow.Rules.Repository;


// POCO class serialized to DB
#nullable disable
namespace Willow.Rules.Model;

/// <summary>
/// Maps trendId or (connectorId + "_" + externalId) to a rule id
/// </summary>
/// <remarks>
/// This table is typically used for single rule where ADX joins on to this table during execution to only return
/// telelmetry linked to a rule
/// </remarks>
public class RuleTimeSeriesMapping : IId
{
	public RuleTimeSeriesMapping()
	{

	}

	public RuleTimeSeriesMapping(RuleInstance ruleInstance, TimeSeriesMapping timeSeriesMapping, DateTimeOffset now)
	{
		this.Id = $"{ruleInstance.Id}_{timeSeriesMapping.DtId}";
		this.TrendId = timeSeriesMapping.TrendId;
		this.ExternalId = timeSeriesMapping.ExternalId;
		this.ConnectorId = timeSeriesMapping.ConnectorId;
		this.DtId = timeSeriesMapping.DtId;
		this.RuleId = ruleInstance.RuleId;
		this.RuleInstanceId = ruleInstance.Id;
		this.TrendId = timeSeriesMapping.TrendId;
		this.LastUpdate = now;
	}

	/// <summary>
	/// The Id stays empty for this table. It is not required
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Trend id if present
	/// </summary>
	public string TrendId { get; set; }

	/// <summary>
	/// The external Id if present
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The connector Id if present
	/// </summary>
	public string ConnectorId { get; set; }

	/// <summary>
	/// The DTDL Id
	/// </summary>
	public string DtId { get; set; }

	/// <summary>
	/// The Rule Id
	/// </summary>
	public string RuleId { get; set; }

	/// <summary>
	/// The Rule Instance Id
	/// </summary>
	public string RuleInstanceId { get; set; }

	/// <summary>
	/// Last updated timestamp
	/// </summary>
	public DateTimeOffset LastUpdate { get; set; }
}
