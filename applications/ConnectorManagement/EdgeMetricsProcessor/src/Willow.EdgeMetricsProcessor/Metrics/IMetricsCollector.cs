// -----------------------------------------------------------------------
// <copyright file="IMetricsCollector.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.EdgeMetricsProcessor.Metrics;

internal interface IMetricsCollector
{
    void TrackSuccessfulProcessedCount(long value, IDictionary<string, string>? dimensions = null);

    void TrackFailedProcessedCount(long value, IDictionary<string, string>? dimensions = null);

    void TrackProcessDuration(long value, IDictionary<string, string>? dimensions = null);

    void TrackUsedCpuPercent(double value, IDictionary<string, string>? dimensions = null);

    void TrackUsedMemoryBytes(double value, IDictionary<string, string>? dimensions = null);

    void TrackTotalMemoryBytes(double value, IDictionary<string, string>? dimensions = null);

    void TrackTotalDiskSpaceBytes(double value, IDictionary<string, string>? dimensions = null);

    void TrackAvailableDiskSpaceBytes(double value, IDictionary<string, string>? dimensions = null);

    void TrackEdgeUptimeSeconds(double value, IDictionary<string, string>? dimensions = null);

    void TrackHostUptimeSeconds(double value, IDictionary<string, string>? dimensions = null);
}
