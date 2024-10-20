using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Willow.Rules.Repository;


// POCO class serialized to DB
#nullable disable
namespace Willow.Rules.Model;

/// <summary>
/// Maps trendId or (connectorId + "_" + externalId) to twinId
/// </summary>
/// <remarks>
/// This table is written to during the Twin cache step
/// It is ready by the real-time processor which uses it to map
/// incoming telemetry to a TwinId which is then used to lookup
/// time series.
///
/// By ensuring we have only one writer and one reader this is
/// now more robust that the old way of trying to do everything
/// in the TimeSeries buffers themselves
/// </remarks>
public class TimeSeriesMapping : IId
{
	/// <summary>
	/// The id of the mapping, trendId if present or connectorId + externalId if not
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; set; }

	/// <summary>
	/// Trend id if present
	/// </summary>
	public string TrendId { get; set; }

	/// <summary>
	/// The connector Id if present
	/// </summary>
	public string ConnectorId { get; set; }

	/// <summary>
	/// The external Id if present
	/// </summary>
	public string ExternalId { get; set; }

	/// <summary>
	/// The DTDL Id
	/// </summary>
	public string DtId { get; set; }

	/// <summary>
	/// The DTDL Model Id
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// The twin's unit
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// The trend interval if set
	/// </summary>
	public int? TrendInterval { get; set; }

	/// <summary>
	/// Last updated timestamp
	/// </summary>
	public DateTimeOffset LastUpdate { get; set; }

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	public IList<TwinLocation> TwinLocations { get; set; } = new List<TwinLocation>(0);

}
