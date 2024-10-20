using System;

namespace Willow.Model.TimeSeries;

public class TimeSeriesAdxDto
{
    /// <summary>
    /// The connector id of twin
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// The digital twin id
    /// </summary>
    public string? DtId { get; set; }

    /// <summary>
    /// The external id of twin
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// The internal id of twin
    /// </summary>
    public string? TrendId { get; set; }

    /// <summary>
    /// The source time stamp
    /// </summary>
    public DateTime? SourceTimestamp { get; set; }

    /// <summary>
    /// The ingested time stamp
    /// </summary>
    public DateTime? EnqueuedTimestamp { get; set; }

    /// <summary>
	/// The number value of ?
	/// </summary>
	public object? ScalarValue { get; set; }

    /// <summary>
	/// The location latitude
	/// </summary>
	public double? Latitude { get; set; }

    /// <summary>
	/// The location longitude
	/// </summary>
	public double? Longitude { get; set; }

    /// <summary>
	/// The location altitude. Height of object above sea or ground level
	/// </summary>
	public double? Altitude { get; set; }

    /// <summary>
	/// The JSON object of additional properties
	/// </summary>
	public object? Properties { get; set; }
}
