// -----------------------------------------------------------------------
// <copyright file="UsedCpuPercentProcessor.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.EdgeMetricsProcessor.Metrics;

using Willow.EdgeMetricsProcessor.Models;

internal abstract class MetricProcessor : IMetricProcessor
{
    public abstract string MetricName { get; }

    public abstract void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions);
}

internal class UsedCpuPercentProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_used_cpu_percent";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackUsedCpuPercent(metric.Value, dimensions);
    }
}

internal class UsedMemoryBytesProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_used_memory_bytes";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackUsedMemoryBytes(metric.Value, dimensions);
    }
}

internal class TotalMemoryBytesProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_total_memory_bytes";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackTotalMemoryBytes(metric.Value, dimensions);
    }
}

internal class TotalDiskSpaceBytesProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_total_disk_space_bytes";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackTotalDiskSpaceBytes(metric.Value, dimensions);
    }
}

internal class AvailableDiskSpaceBytesProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_available_disk_space_bytes";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackAvailableDiskSpaceBytes(metric.Value, dimensions);
    }
}

internal class EdgeUptimeSecondsProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_iotedged_uptime_seconds";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackEdgeUptimeSeconds(metric.Value, dimensions);
    }
}

internal class HostUptimeSecondsProcessor : MetricProcessor
{
    public override string MetricName => "edgeAgent_host_uptime_seconds";

    public override void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions)
    {
        metricsCollector.TrackHostUptimeSeconds(metric.Value, dimensions);
    }
}
