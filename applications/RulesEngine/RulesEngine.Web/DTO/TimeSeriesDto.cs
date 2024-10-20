using System;
using System.Linq;
using Willow.Rules.Model;

namespace WillowRules.DTO;

/// <summary>
/// Time series summary
/// </summary>
public class TimeSeriesDto
{
    /// <summary>
    /// Creates a new <see cref="TimeSeriesDto"/>
    /// </summary>
    public TimeSeriesDto(TimeSeries buffer)
    {
        StartTime = buffer.EarliestSeen;
        EndTime = buffer.LastSeen;
        Id = buffer.Id;
        DtId = buffer.DtId;
        ModelId = buffer.ModelId;
        TrendInterval = buffer.TrendInterval;
        AverageInBuffer = buffer.AverageInBuffer;
        AverageValue = buffer.AverageValue;
        EstimatedPeriod = buffer.EstimatedPeriod.TotalSeconds;
        TotalValuesProcessed = buffer.TotalValuesProcessed;
        UnitOfMeasure = buffer.UnitOfMeasure;
        MaxCountToKeep = buffer.MaxCountToKeep ?? -1;
        MaxTimeToKeep = (buffer.MaxTimeToKeep ?? TimeSpan.Zero).TotalSeconds;
        MaxValue = buffer.MaxValue;
        MinValue = buffer.MinValue;
        BufferCount = buffer.Count;
        TotalValue = buffer.TotalValue;
        Status = buffer.GetStatus();
        ExternalId = buffer.ExternalId;
        ConnectorId = buffer.ConnectorId;
        Compression = buffer.GetCompression();
        Latency = buffer.Latency.TotalSeconds;
        TwinLocations = buffer.TwinLocations?.ToArray() ?? [];
    }

    /// <summary>
    /// An Id for the time series
    /// </summary>
    public string Id { get; init; }

    // TODO: How to get the actual twin Id too?  Should put that in timeseries?

    /// <summary>
    /// The start time for the timeseries data
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// The end time for the timeseries data
    /// </summary>
    public DateTimeOffset EndTime { get; init; }

    /// <summary>
    /// Count in the buffer
    /// </summary>
    public int BufferCount { get; init; }

    /// <summary>
    /// Max time to keep
    /// </summary>
    public double? MaxTimeToKeep { get; init; }

    /// <summary>
    /// Max count to keep
    /// </summary>
    public int? MaxCountToKeep { get; init; }

    /// <summary>
    /// Total values processed
    /// </summary>
    public long TotalValuesProcessed { get; init; }

    /// <summary>
    /// Estimate of period in seconds
    /// </summary>
    public double EstimatedPeriod { get; init; }

    /// <summary>
    /// Average value calculated how?
    /// </summary>
    public double? AverageValue { get; init; }

    /// <summary>
    /// Average of values in the buffer
    /// </summary>
    public double AverageInBuffer { get; init; }

    /// <summary>
    /// Unit of measure (same as Twin provides)
    /// </summary>
    public string UnitOfMeasure { get; init; }

    /// <summary>
    /// The maximum value over time
    /// </summary>
    public double MaxValue { get; init; }

    /// <summary>
    /// The minimum value over time
    /// </summary>
    public double MinValue { get; init; }

    /// <summary>
    /// The sum of all values over time
    /// </summary>
    public double TotalValue { get; init; }

    /// <summary>
    /// The digital twin Id (or null)  Not set until caching has run after point was found
    /// </summary>
    public string DtId { get; init; }

    /// <summary>
    /// The DTDL Model
    /// </summary>
    public string ModelId { get; init; }

    /// <summary>
	/// The Connector Id for the twin. Optionally used to identify a twin with ExternalId
	/// </summary>
	public string ConnectorId { get; set; }

    /// <summary>
    /// The External Id for the twin. Optionally used to identify a twin with ConnectorId
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// The Trend interval (in seconds) copied over from the twin
    /// </summary>
    public int? TrendInterval { get; init; }

    /// <summary>
    /// The status of the timeseries
    /// </summary>
    public TimeSeriesStatus Status { get; init; }

    /// <summary>
    /// The compression for the buffer
    /// </summary>
    public double Compression { get; init; }

    /// <summary>
	/// Kalman filtered latency
	/// </summary>
	public double Latency { get; init; }

    /// <summary>
    /// Parent chain by locatedIn and isPartOf
    /// </summary>
    public TwinLocation[] TwinLocations { get; set; } = [];
}
