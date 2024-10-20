namespace Willow.EdgeMetricsProcessor;

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Willow.EdgeMetricsProcessor.HealthChecks;
using Willow.EdgeMetricsProcessor.Metrics;
using Willow.EdgeMetricsProcessor.Models;
using Willow.LiveData.Pipeline;

/// <summary>
/// Listens to Event Hub for telemetry of the specified type and forwards it to the configured processor.
/// </summary>
internal class EdgeMetricsProcessor(ILogger<EdgeMetricsProcessor> logger,
                                    IMetricsCollector metricsCollector,
                                    HealthCheckEdgeMetricsProcessor healthCheck)
    : ITelemetryProcessor<IoTHubMetric[]>
{
    private static readonly string[] RequiredMetrics =
    [
        "edgeAgent_used_cpu_percent",
        "edgeAgent_used_memory_bytes",
        "edgeAgent_total_memory_bytes",
        "edgeAgent_total_disk_space_bytes",
        "edgeAgent_available_disk_space_bytes",
        "edgeAgent_iotedged_uptime_seconds",
        "edgeAgent_host_uptime_seconds",
    ];

    private static readonly string[] RequiredDimensions =
    [
        "edge_device",
        "module_name",
        "iothub"
    ];

    private readonly List<IMetricProcessor> metricProcessors =
    [
        new UsedCpuPercentProcessor(),
        new TotalMemoryBytesProcessor(),
        new UsedMemoryBytesProcessor(),
        new TotalDiskSpaceBytesProcessor(),
        new AvailableDiskSpaceBytesProcessor(),
        new EdgeUptimeSecondsProcessor(),
        new HostUptimeSecondsProcessor(),
    ];

    /// <summary>
    /// Determines whether a given IoTHubMetric is valid for processing.
    /// </summary>
    /// <param name="metric">The IoTHubMetric to validate.</param>
    /// <returns>True if the metric is valid; otherwise, false.</returns>
    /// <remarks>For certain metrics such as CPU percent used, multiple metrics are returned
    /// corresponding to different quantile values. For simplicity reasons, we only process
    /// metrics corresponding to 0.99 quantile value if present.
    /// <p>For disk usage metrics, we skip the overlay filesystem as it is not relevant to our use case.</p>
    /// </remarks>
    private static bool IsValidMetric(IoTHubMetric metric)
    {
        return metric.Labels is not null
               && (!metric.Labels.TryGetValue("disk_filesystem", out var diskFilesystemValue) || diskFilesystemValue != "overlay")
               && (!metric.Labels.TryGetValue("quantile", out var quantileValue) || quantileValue == "0.99");
    }

    private void ProcessMetric(IoTHubMetric metric)
    {
        var dimensions = metric.Labels!.Where(k => RequiredDimensions.Contains(k.Key))
                               .ToDictionary(k => k.Key, k => k.Value);

        //Based on the metric name, update the appropriate counter with the value and dimensions
        var processor = metricProcessors.FirstOrDefault(p => p.MetricName == metric.Name);
        processor?.ProcessMetric(metricsCollector, metric, dimensions);
    }

    private int ProcessEdgeMetrics(IEnumerable<IoTHubMetric> metrics)
    {
        var processed = 0;
        var filteredMetrics = metrics.Where(m => RequiredMetrics.Contains(m.Name));

        foreach (var metric in filteredMetrics)
        {
            if (!IsValidMetric(metric))
            {
                continue;
            }

            ProcessMetric(metric);
            processed++;
        }

        return processed;
    }

    public Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IoTHubMetric[] telemetry, CancellationToken cancellationToken = default) => ProcessAsync([telemetry], cancellationToken);

    public async Task<(int Succeeded, int Failed, int Skipped)> ProcessAsync(IEnumerable<IoTHubMetric[]> batch, CancellationToken cancellationToken = default)
    {
        healthCheck.Current = HealthCheckEdgeMetricsProcessor.Starting;

        var stopwatch = Stopwatch.StartNew();
        var processedMetricCount = 0;

        foreach (var telemetry in batch)
        {
            try
            {
                processedMetricCount += ProcessEdgeMetrics(telemetry);

                metricsCollector.TrackSuccessfulProcessedCount(processedMetricCount);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing incoming message: {Telemetry}. Error: {Message}", telemetry, e.Message);
                metricsCollector.TrackFailedProcessedCount(1);
                healthCheck.Current = HealthCheckEdgeMetricsProcessor.FailedToProcess;
            }
            finally
            {
                stopwatch.Stop();
                metricsCollector.TrackProcessDuration(stopwatch.ElapsedMilliseconds);
            }
        }

        //Insert small delay to simulate processing time
        await Task.Delay(0, cancellationToken);
        return (processedMetricCount, 0, 0);
    }
}
