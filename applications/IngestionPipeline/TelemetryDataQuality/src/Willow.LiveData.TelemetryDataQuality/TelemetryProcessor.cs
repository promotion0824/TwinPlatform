// -----------------------------------------------------------------------
// <copyright file="TelemetryProcessor.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality;

using Willow.LiveData.Pipeline;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Telemetry;

/// <inheritdoc/>
internal class TelemetryProcessor(
    ITimeSeriesService timeSeriesService,
    ITwinsService twinsService,
    IMetricsCollector metricsCollector,
    ILogger<TelemetryProcessor> logger)
    : ITelemetryProcessor
{
    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(Telemetry telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<Telemetry> batch, CancellationToken cancellationToken = default)
    {
        var inputBatch = batch.ToList();
        var processed = 0;
        var failed = 0;

        foreach (var message in inputBatch.Where(msg => !string.IsNullOrEmpty(msg.ExternalId) && msg.ConnectorId != "a1001ffa-4372-4767-8fb5-aeb6468f353b"))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (processed, failed, inputBatch.Count - processed - failed);
            }

            try
            {
                var twin = await twinsService.GetTwin(message.ExternalId);
                metricsCollector.TrackMetric(twin is null ? "TwinCacheMiss" : "TwinCacheHit", 1, MetricType.Counter);

                var result = await timeSeriesService.UpdateTimeSeriesAsync(message, twin, cancellationToken);
                if (!result)
                {
                    metricsCollector.TrackMetric("TimeSeriesErrorCounter", 1, MetricType.Counter);
                    failed++;
                    continue;
                }

                processed++;
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to process telemetry message: {Message}", e.Message);
                failed++;
            }
        }

        try
        {
            return (processed, failed, inputBatch.Count - processed - failed);
        }
        catch (PipelineException)
        {
            return (0, inputBatch.Count, 0);
        }
    }
}
