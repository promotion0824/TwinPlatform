namespace Willow.Caching.Telemetry.DataQuality.Models.TimeSeries;

using System;
using System.Text.Json.Serialization;
using Willow.Caching.Telemetry.DataQuality.Filters;

/// <summary>
/// TimeSeries validity statuses.
/// </summary>
/// <remarks>Original code source from Activate Technology code base.</remarks>
[Flags]
public enum TimeSeriesStatus
{
    /// <summary>Timeseries is invalid; init value.</summary>
    Invalid = 0,

    /// <summary>Timeseries is valid.</summary>
    Valid = 1,

    /// <summary>The sensor appears to be offline, no data has been received for > 3 x expected period.</summary>
    Offline = 2,

    /// <summary>The sensor has been stuck on the same value for a long time.</summary>
    Stuck = 4,

    /// <summary>The value is above the maximum or below the minimum for this type.</summary>
    ValueOutOfRange = 8,

    /// <summary>The period is too far from the expected interval.</summary>
    PeriodOutOfRange = 16,

    /// <summary>The sensor time series has no twin Id.</summary>
    NoTwin = 32,
}

/// <summary>
/// A buffered window of time series values for a single point / capability.
/// </summary>
public class TimeSeries : TimeSeriesBuffer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSeries"/> class.
    /// Deserialization only constructor, do not call.
    /// </summary>
    [JsonConstructor]
    public TimeSeries()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSeries"/> class.
    /// </summary>
    /// <param name="id">Id for the timeseries.</param>
    /// <param name="unit">Unit of measure associated with the timeseries.</param>
    public TimeSeries(string id, string unit)
    {
        this.Id = id ?? throw new ArgumentNullException(nameof(id));
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Id cannot be empty", nameof(id));
        }

        UnitOfMeasure = unit;
    }

    /// <summary>
    /// Gets or sets the Id associated with the TimeSeries.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the digital twin Id (or null).
    /// </summary>
    public string? DtId { get; set; }

    /// <summary>
    /// Gets or sets the DTDL model id, e.g. ...Sensor;1.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Connector Id for the twin.
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the External Id for the twin.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the Trend interval (in seconds) copied over from the twin.
    /// </summary>
    public int? TrendInterval { get; set; }

    /// <summary>
    /// Gets or sets the first time a point was seen.
    /// </summary>
    public DateTimeOffset EarliestSeen { get; set; } = DateTimeOffset.MaxValue;

    /// <summary>
    /// Gets or sets the last time a point was seen.
    /// </summary>
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// Gets or sets the last time a point was seen.
    /// </summary>
    public DateTimeOffset LastValidationStatusChange { get; set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// Gets or sets the total values processed so far.
    /// </summary>
    public long TotalValuesProcessed { get; set; }

    /// <summary>
    /// Gets or sets kalman filter state for estimating average period.
    /// </summary>
    public Kalman? AveragePeriodEstimator { get; set; }

    /// <summary>
    /// Gets or sets kalman filter state for estimating out of range value.
    /// </summary>
    public BinaryKalmanFilter? ValueOutOfRangeEstimator { get; set; }

    /// <summary>
    /// Gets or sets a Kalman filtered estimate of the average period.
    /// </summary>
    public TimeSpan EstimatedPeriod { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets a value indicating whether the latest timeseries value is valid.
    /// </summary>
    public bool LatestValueValid { get; set; }

    /// <summary>
    /// Gets or sets the average value over time.
    /// </summary>
    public double AverageValue { get; set; }

    /// <summary>
    /// Gets or sets last double type value.
    /// </summary>
    public double? LastValueDouble { get; set; }

    /// <summary>
    /// Gets or sets last boolean type value.
    /// </summary>
    public bool? LastValueBool { get; set; }

    /// <summary>
    /// Gets or sets last string type value.
    /// </summary>
    public string? LastValueString { get; set; }

    /// <summary>
    /// Gets or sets the maximum value over time.
    /// </summary>
    public double MaxValue { get; set; } = -1E300;

    /// <summary>
    /// Gets or sets the minimum value over time.
    /// </summary>
    public double MinValue { get; set; } = 1E300;

    /// <summary>
    /// Gets or sets the sum of all values over time.
    /// </summary>
    public double TotalValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the timeseries value is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the value is above the maximum or below the minimum for this type.
    /// </summary>
    public bool IsValueOutOfRange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the period is too far from the expected interval.
    /// </summary>
    public bool IsPeriodOutOfRange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sensor has been stuck on the same value for a long time.
    /// </summary>
    public bool IsStuck { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the sensor appears to be offline, no data has been received for > 3 x expected period.
    /// </summary>
    public bool IsOffline { get; set; }
}
