namespace Willow.LiveData.TelemetryDataQuality.Services;

using Willow.LiveData.Pipeline;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Telemetry;

/// <inheritdoc/>
internal class TimeSeriesService(
    ISender<TelemetryDataQuality> telemetrySender,
    ICacheService<TimeSeries> timeSeriesCache,
    IMetricsCollector metricsCollector,
    ILogger<TimeSeriesService> logger) : ITimeSeriesService
{
    private const int MaxBufferCount = 3;
    private const int DefaultTrendInterval = 300;
    private const int DaysToKeepTimeSeriesInCache = 7;

    public async Task<bool> UpdateTimeSeriesAsync(
        Telemetry data,
        TwinDetails? twin,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{Constants.TimeSeriesCachePrefix}-{data.ExternalId}";

            var timeSeries = await GetTimeSeries(data.ExternalId!);
            if (timeSeries is null)
            {
                metricsCollector.TrackMetric("TimeSeriesCacheMiss", 1, MetricType.Counter);
                timeSeries = CreateNewTimeSeries(twin, data.ExternalId!);
            }
            else
            {
                metricsCollector.TrackMetric("TimeSeriesCacheHit", 1, MetricType.Counter);
            }

            if (twin is not null)
            {
                UpdateAttributes(timeSeries, twin);
            }

            timeSeries.SetMaxBufferCount(MaxBufferCount);
            timeSeries.EnableValidation();

            var timedValue = GetTimedValue(data);
            timeSeries = await UpdateTimeSeries(timeSeries, data.SourceTimestamp, timedValue, cancellationToken);

            await timeSeriesCache.SetAsync(key, timeSeries, TimeSpan.FromDays(DaysToKeepTimeSeriesInCache));

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while updating TimeSeries for {Id}: {Message}", data.ExternalId, e.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets the timed value for a given telemetry data.
    /// </summary>
    /// <param name="data">The telemetry data.</param>
    /// <returns>The timed value.</returns>
    private static TimedValue GetTimedValue(Telemetry data)
    {
        if (bool.TryParse(data.ScalarValue?.ToString(), out bool bScalarValue))
        {
            return new TimedValue(data.SourceTimestamp, bScalarValue);
        }

        return double.TryParse(data.ScalarValue?.ToString(), out double dScalarValue)
            ? new TimedValue(data.SourceTimestamp, dScalarValue)
            : new TimedValue(data.SourceTimestamp, data.ScalarValue?.ToString());
    }

    public async Task<TimeSeries?> GetTimeSeries(string id)
    {
        var key = $"{Constants.TimeSeriesCachePrefix}-{id}";
        return await timeSeriesCache.GetAsync(key);
    }

    /// <summary>
    /// Creates a new TimeSeries object based on the provided TwinDetails and externalId.
    /// </summary>
    /// <param name="twin">The TwinDetails object for the twin.</param>
    /// <param name="externalId">The external ID of the twin.</param>
    /// <returns>
    /// The newly created TimeSeries object.
    /// </returns>
    private static TimeSeries CreateNewTimeSeries(TwinDetails? twin, string externalId)
    {
        var interval = (int)(twin?.TrendInterval is null or <= 0 ? DefaultTrendInterval : twin.TrendInterval);
        var timeSeries = new TimeSeries(externalId, twin?.Unit ?? string.Empty)
        {
            ConnectorId = twin?.ConnectorId,
            DtId = twin?.Id,
            ExternalId = externalId,
            ModelId = twin?.ModelId ?? string.Empty,
            TrendInterval = interval,
        };

        timeSeries.SetMaxBufferCount(MaxBufferCount);
        timeSeries.EnableValidation();
        return timeSeries;
    }

    /// <summary>
    /// Updates the time series with a new value at a specific timestamp.
    /// </summary>
    /// <param name="timeSeries">The TimeSeries object to update.</param>
    /// <param name="sourceTimestamp">The timestamp of the new value.</param>
    /// <param name="value">The new value to add to the time series.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated TimeSeries object.</returns>
    /// <remarks>Additionally tracks any state change in the TimeSeries status and sends to
    /// Telemetry Data Quality table on change.
    /// </remarks>
    private async Task<TimeSeries> UpdateTimeSeries(
        TimeSeries timeSeries,
        DateTime sourceTimestamp,
        TimedValue value,
        CancellationToken cancellationToken = default)
    {
        var currentStatus = timeSeries.GetStatus();
        timeSeries.AddPoint(value);

        timeSeries.SetStatus(sourceTimestamp);

        if (currentStatus != timeSeries.GetStatus())
        {
            logger.LogDebug(
                "TimeSeries {TimeSeriesId} status changed from {PrevStatus} to {Status}. ValuesProcessed: {ValueCount}",
                timeSeries.Id,
                currentStatus,
                timeSeries.GetStatus(),
                timeSeries.TotalValuesProcessed);
            timeSeries.LastValidationStatusChange = DateTimeOffset.UtcNow;
            await telemetrySender.SendAsync(GetTelemetryDataQualityPayload(timeSeries), cancellationToken);
        }

        if (timeSeries.Count < MaxBufferCount * 2)
        {
            return timeSeries;
        }

        timeSeries.ApplyLimits(DateTime.UtcNow, TimeSpan.FromDays(7));

        return timeSeries;
    }

    public async Task CheckStatus(IEnumerable<string> externalIds)
    {
        try
        {
            foreach (var externalId in externalIds)
            {
                var key = $"{Constants.TimeSeriesCachePrefix}-{externalId}";
                var timeSeries = await timeSeriesCache.GetAsync(key);
                if (timeSeries is null)
                {
                    continue;
                }

                var currentStatus = timeSeries.GetStatus();
                timeSeries.SetStatus(DateTimeOffset.UtcNow);

                if (currentStatus == timeSeries.GetStatus())
                {
                    continue;
                }

                logger.LogDebug(
                    "TimeSeries {TimeSeriesId} status changed from {PrevStatus} to {Status}. ValuesProcessed: {ValueCount}",
                    timeSeries.Id,
                    currentStatus,
                    timeSeries.GetStatus(),
                    timeSeries.TotalValuesProcessed);
                timeSeries.LastValidationStatusChange = DateTimeOffset.UtcNow;

                await timeSeriesCache.SetAsync(key, timeSeries, TimeSpan.FromDays(DaysToKeepTimeSeriesInCache));
                await telemetrySender.SendAsync(GetTelemetryDataQualityPayload(timeSeries));
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error checking TimeSeries status: {Message}", ex.Message);
        }
    }

    public async Task PersistToDatabaseAsync(IEnumerable<string> externalIds)
    {
        List<TelemetryDataQuality> outputBatch = [];

        foreach (var externalId in externalIds)
        {
            var key = $"{Constants.TimeSeriesCachePrefix}-{externalId}";
            var timeSeries = await timeSeriesCache.GetAsync(key);
            if (timeSeries is not null)
            {
                outputBatch.Add(GetTelemetryDataQualityPayload(timeSeries));
            }
        }

        await telemetrySender.SendAsync(outputBatch);
        metricsCollector.TrackMetric("PersistedTimeSeriesCount", outputBatch.Count, MetricType.Counter);
    }

    /// <summary>
    /// Generates the telemetry data quality payload for a given time series.
    /// </summary>
    /// <param name="timeSeries">The time series for which the payload is generated.</param>
    /// <returns>The telemetry data quality payload.</returns>
    private TelemetryDataQuality GetTelemetryDataQualityPayload(TimeSeries timeSeries)
    {
        var dictionary = timeSeries.PopulateTimeSeriesStatusMetrics();
        if (string.IsNullOrWhiteSpace(timeSeries.ExternalId))
        {
            logger.LogWarning("ExternalId is null or empty for TimeSeries {TimeSeriesId}", timeSeries.Id);
        }

        return new TelemetryDataQuality
        {
            DtId = timeSeries.DtId,
            ConnectorId = timeSeries.ConnectorId,
            ExternalId = timeSeries.ExternalId,
            ValidationResults = dictionary,
            LastValidationUpdatedAt = timeSeries.LastValidationStatusChange,
            EnqueuedTimestamp = DateTime.UtcNow,
            SourceTimestamp = timeSeries.LastSeen,
        };
    }

    /// <summary>
    /// Updates the attributes of a TimeSeries object based on the TwinDetails object.
    /// </summary>
    /// <param name="timeSeries">The TimeSeries object to update.</param>
    /// <param name="twin">The TwinDetails object containing the attribute values.</param>
    private static void UpdateAttributes(TimeSeries timeSeries, TwinDetails twin)
    {
        timeSeries.Id = twin.ExternalId;
        timeSeries.ConnectorId = twin.ConnectorId;
        timeSeries.DtId = twin.Id;
        timeSeries.ExternalId = twin.ExternalId;
        timeSeries.ModelId = twin.ModelId;
        timeSeries.TrendInterval = twin.TrendInterval;
        timeSeries.UnitOfMeasure = twin.Unit ?? string.Empty;
    }
}
