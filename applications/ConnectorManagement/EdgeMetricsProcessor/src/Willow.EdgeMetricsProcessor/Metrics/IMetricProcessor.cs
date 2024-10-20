// -----------------------------------------------------------------------
// <copyright file="IMetricProcessor.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.EdgeMetricsProcessor.Metrics;

using Willow.EdgeMetricsProcessor.Models;

internal interface IMetricProcessor
{
    string MetricName { get; }

    void ProcessMetric(IMetricsCollector metricsCollector, IoTHubMetric metric, IDictionary<string, string> dimensions);
}
