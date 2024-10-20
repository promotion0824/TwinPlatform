// -----------------------------------------------------------------------
// <copyright file="MetricsCollector.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.EdgeMetricsProcessor.Metrics;

using System.Diagnostics.Metrics;

internal class MetricsCollector(Meter meter, Willow.Telemetry.MetricsAttributesHelper metricsAttributesHelper) : IMetricsCollector
{
    private const string MetricsPrefix = "EdgeMetrics-";

    private readonly Counter<long> successCounter = meter.CreateCounter<long>($"{MetricsPrefix}Processed", description: "The number of edge metrics processed by the processor");
    private readonly Counter<long> failedCounter = meter.CreateCounter<long>($"{MetricsPrefix}Failed", description: "The number of edge metrics failed to process by the processor");
    private readonly Histogram<long> processDuration = meter.CreateHistogram<long>($"{MetricsPrefix}ProcessDuration", "milliseconds", "The time taken to process a metric message to completion");

    private readonly Counter<double> edgeAgentUsedCpuPercent = meter.CreateCounter<double>($"{MetricsPrefix}UsedCpuPercent");
    private readonly Counter<double> edgeAgentUsedMemoryBytes = meter.CreateCounter<double>($"{MetricsPrefix}UsedMemoryBytes");
    private readonly Counter<double> edgeAgentTotalMemoryBytes = meter.CreateCounter<double>($"{MetricsPrefix}TotalMemoryBytes");
    private readonly Counter<double> edgeAgentTotalDiskSpaceBytes = meter.CreateCounter<double>($"{MetricsPrefix}TotalDiskSpaceBytes");
    private readonly Counter<double> edgeAgentAvailableDiskSpaceBytes = meter.CreateCounter<double>($"{MetricsPrefix}AvailableDiskSpaceBytes");
    private readonly Counter<double> edgeAgentEdgeUptimeSeconds = meter.CreateCounter<double>($"{MetricsPrefix}EdgeUptimeSeconds");
    private readonly Counter<double> edgeAgentHostUptimeSeconds = meter.CreateCounter<double>($"{MetricsPrefix}HostUptimeSeconds");

    public void TrackSuccessfulProcessedCount(long value, IDictionary<string, string>? dimensions) => UpdateCounter(successCounter, value, dimensions);

    public void TrackFailedProcessedCount(long value, IDictionary<string, string>? dimensions) => UpdateCounter(failedCounter, value, dimensions);

    public void TrackProcessDuration(long value, IDictionary<string, string>? dimensions) => UpdateHistogram(processDuration, value, dimensions);

    public void TrackUsedCpuPercent(double value, IDictionary<string, string>? dimensions) => UpdateCounter(edgeAgentUsedCpuPercent, value, dimensions);

    public void TrackUsedMemoryBytes(double value, IDictionary<string, string>? dimensions) => UpdateCounter(edgeAgentUsedMemoryBytes, value, dimensions);

    public void TrackTotalMemoryBytes(double value, IDictionary<string, string>? dimensions) => UpdateCounter(edgeAgentTotalMemoryBytes, value, dimensions);

    public void TrackTotalDiskSpaceBytes(double value, IDictionary<string, string>? dimensions) => UpdateCounter(edgeAgentTotalDiskSpaceBytes, value, dimensions);

    public void TrackAvailableDiskSpaceBytes(double value, IDictionary<string, string>? dimensions) => UpdateCounter(edgeAgentAvailableDiskSpaceBytes, value, dimensions);

    public void TrackEdgeUptimeSeconds(double value, IDictionary<string, string>? dimensions = null) => UpdateCounter(edgeAgentEdgeUptimeSeconds, value, dimensions);

    public void TrackHostUptimeSeconds(double value, IDictionary<string, string>? dimensions = null) => UpdateCounter(edgeAgentHostUptimeSeconds, value, dimensions);

    private void UpdateCounter<T>(Counter<T> counter, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        if (dimensions != null)
        {
            counter.Add(value,
                         metricsAttributesHelper.GetValues(dimensions.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray()));
        }
        else
        {
            counter.Add(value);
        }
    }

    private void UpdateHistogram<T>(Histogram<T> histogram, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        if (dimensions != null)
        {
            histogram.Record(value,
                             metricsAttributesHelper.GetValues(dimensions.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray()));
        }
        else
        {
            histogram.Record(value);
        }
    }
}
