namespace Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;

using System;
using System.Text.Json.Serialization;
using Willow.Expressions;
using Willow.LiveData.TelemetryDataQuality.Filters;

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
internal class TimeSeries : TimeSeriesBuffer
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
        Id = id ?? throw new ArgumentNullException(nameof(id));
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

    /// <summary>
    /// Sets the status values according to the period and value vs expected.
    /// </summary>
    /// <param name="now">Current DateTimeOffset.</param>
    public void SetStatus(DateTimeOffset now)
    {
        var modelId = ModelId;

        this.IsValid = LatestValueValid;
        if (ValueOutOfRangeEstimator is not null)
        {
            IsValueOutOfRange = ValueOutOfRangeEstimator.State > ValueOutOfRangeEstimator.BenchMark;
        }
        else
        {
            // In case sensor changes type and no longer needs to be checked
            IsValueOutOfRange = false;
        }

        var isTextValue = !string.IsNullOrEmpty(LastValueString);

        // Don't declare it stuck until we have seen enough values (now 50)
        this.IsStuck = this.TotalValuesProcessed > 50 && this.MinValue == this.MaxValue &&

                  // Ignore sensors sat on zero - most of these are legitimate:
                  // damper closed, no electricity flowing, no people, ...
                  this.MinValue != 0 &&
                  !isTextValue &&

                  // ignore setpoints, and actuators they often don't change
                  !modelId.EndsWith("Actuator;1") &&
                  !modelId.EndsWith("Setpoint;1") &&
                  !modelId.EndsWith(
                      "Energy;1") && // ignore energy sensors, they can stay on the same value for a long time
                  !modelId.Equals("dtmi:com:willowinc:Sensor;1") && // ignore very generic sensors
                  !string.Equals(UnitOfMeasure,
                      "bool",
                      StringComparison.OrdinalIgnoreCase); //ignore capabilities for bool

        //some telemetry comes in slower than the configured interval, so we take the largest of trendInterval vs estimatedPeriod
        var trendInterval = Math.Max(EstimatedPeriod.TotalSeconds, TrendInterval ?? 0);

        this.IsOffline = (now - LastSeen > TimeSpan.FromDays(7) //data is older than 7 days
                     || (TrendInterval.HasValue &&
                         (now - LastSeen).TotalSeconds >
                         trendInterval * 3)) //last seen time is 3x greater than the twin's interval
                    &&
                    !isTextValue; //text based values (usually events) can be very far in between so they cant go offline

        this.IsPeriodOutOfRange = this.TotalValuesProcessed > 1 && this.TrendInterval.HasValue &&
                                  (this.EstimatedPeriod.TotalSeconds < 0.1 * this.TrendInterval.Value ||
                                   this.EstimatedPeriod.TotalSeconds > 1.9 * this.TrendInterval.Value);
    }

    /// <summary>
    /// Gets the status of the timeseries.
    /// </summary>
    /// <returns>The status of the timeseries as a <see cref="TimeSeriesStatus"/> value.</returns>
    public TimeSeriesStatus GetStatus()
    {
        var status = TimeSeriesStatus.Invalid;

        if (IsValid)
        {
            status |= TimeSeriesStatus.Valid;
        }

        if (IsStuck)
        {
            status |= TimeSeriesStatus.Stuck;
        }

        if (IsOffline)
        {
            status |= TimeSeriesStatus.Offline;
        }

        if (IsValueOutOfRange)
        {
            status |= TimeSeriesStatus.ValueOutOfRange;
        }

        if (IsPeriodOutOfRange)
        {
            status |= TimeSeriesStatus.PeriodOutOfRange;
        }

        if (string.IsNullOrWhiteSpace(DtId))
        {
            status |= TimeSeriesStatus.NoTwin;
        }

        return status;
    }

    /// <summary>
    /// Disables validation and clears related kalman filters.
    /// </summary>
    public void DisableValidation()
    {
        ValueOutOfRangeEstimator = null;
    }

    /// <summary>
    /// Enables applicable validations.
    /// </summary>
    public void EnableValidation()
    {
        if (Unit.HasRange(UnitOfMeasure, ModelId))
        {
            ValueOutOfRangeEstimator ??= new BinaryKalmanFilter();
        }
        else
        {
            ValueOutOfRangeEstimator = null;
        }
    }

    /// <summary>
    /// Disables estimated period and clears related kalman filter.
    /// </summary>
    public void DisableEstimatedPeriod()
    {
        AveragePeriodEstimator = null;
        EstimatedPeriod = TimeSpan.Zero;
    }

    /// <summary>
    /// Disables estimated period and clears related kalman filter.
    /// </summary>
    private void EnableEstimatedPeriod()
    {
        AveragePeriodEstimator ??= new Kalman();
    }

    /// <summary>
    /// Verify data quality and adds a point to the buffer.
    /// </summary>
    /// <param name="newValue">The new value to add to the buffer.</param>
    /// <param name="includeDataQualityCheck">Whether to include data quality checks. Default is true.</param>
    /// <returns>True if the point was successfully added to the buffer, False otherwise.</returns>
    public bool AddPoint(in TimedValue newValue,
        bool includeDataQualityCheck = true)
    {
        if (!IsValidIncomingPoint(newValue))
        {
            LatestValueValid = false;
            return false;
        }

        LatestValueValid = true;

        if (includeDataQualityCheck)
        {
            EnableValidation();
            EnableEstimatedPeriod();
        }

        var qualityCheckPassed = ValidateDataQualityAndUpdateFilters(newValue);

        if (includeDataQualityCheck && !qualityCheckPassed)
        {
            //still need to update last seen as it is used to check if timeseries is offline
            LastSeen = newValue.Timestamp;
            TotalValuesProcessed++;
            return false;
        }

        var added = base.AddPoint(in newValue);

        // On save, we will apply limits for maxCapacity and minDate
        UpdateCounters(in newValue);

        return added;
    }

    private void UpdateCounters(in TimedValue lastPoint)
    {
        EarliestSeen = lastPoint.Timestamp < EarliestSeen ? lastPoint.Timestamp : EarliestSeen;
        LastSeen = lastPoint.Timestamp;
        LastValueBool = lastPoint.ValueBool;
        LastValueDouble = lastPoint.ValueDouble;
        LastValueString = lastPoint.ValueText;

        // cannot just compare to >0 as many values are zero
        if (lastPoint.ValueDouble.HasValue || lastPoint.ValueBool.HasValue)
        {
            var numericValue = lastPoint.NumericValue;

            TotalValuesProcessed++;
            MaxValue = Math.Max(numericValue, MaxValue);
            MinValue = Math.Min(numericValue, MinValue);
            var totalValue = TotalValue + numericValue;

            if (!double.IsPositiveInfinity(totalValue)
                && !double.IsNegativeInfinity(totalValue)
                && !double.IsNaN(totalValue))
            {
                TotalValue = totalValue;
                if (TotalValue != 0
                    && TotalValuesProcessed > 0)
                {
                    AverageValue = TotalValue / TotalValuesProcessed;
                }
                else
                {
                    AverageValue = 0;
                }
            }
        }

        if (this.LastGap <= TimeSpan.Zero || this.AveragePeriodEstimator is null)
        {
            return;
        }

        var est = KalmanFilter.Update(this.AveragePeriodEstimator, this.LastGap.TotalSeconds);
        this.EstimatedPeriod = TimeSpan.FromSeconds(est);
    }

    private bool ValidateDataQualityAndUpdateFilters(TimedValue newValue)
    {
        var timedValuePointIsOutOfRange = false;

        if (this.ValueOutOfRangeEstimator is null)
        {
            return !timedValuePointIsOutOfRange;
        }

        (_, timedValuePointIsOutOfRange) = Unit.IsOutOfRange(this.UnitOfMeasure, this.ModelId, newValue.NumericValue);
        BinaryKalmanFilterFunctions.Update(this.ValueOutOfRangeEstimator, timedValuePointIsOutOfRange);

        return !timedValuePointIsOutOfRange;
    }

    /// <summary>
    /// Populates relevant time series status metrics into a dictionary.
    /// </summary>
    /// <returns>A dictionary of key-value pairs, where key is the metric name and value is it's evaluated value.</returns>
    public Dictionary<string, object> PopulateTimeSeriesStatusMetrics()
    {
        var dictionary = new Dictionary<string, object>();
        var timeSeriesStatus = this.GetStatus();

        dictionary["Valid"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.Valid);
        dictionary["NoTwin"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.NoTwin);
        dictionary["Offline"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.Offline);
        dictionary["ValueOutOfRange"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.ValueOutOfRange);
        dictionary["PeriodOutOfRange"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.PeriodOutOfRange);
        dictionary["Stuck"] = timeSeriesStatus.HasFlag(TimeSeriesStatus.Stuck);
        dictionary["TotalValuesProcessed"] = this.TotalValuesProcessed;
        dictionary["TimeSeriesBufferCount"] = this.Count;
        dictionary["EstimatedPeriod"] = this.EstimatedPeriod.TotalSeconds;

        return dictionary;
    }
}
