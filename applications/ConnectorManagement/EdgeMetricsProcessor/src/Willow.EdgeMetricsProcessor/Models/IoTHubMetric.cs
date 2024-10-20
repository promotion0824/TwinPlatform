// -----------------------------------------------------------------------
// <copyright file="IoTHubMetric.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.EdgeMetricsProcessor.Models;

internal record IoTHubMetric
{
    public DateTime TimeGeneratedUtc { get; set; }

    public string? Name { get; set; }

    public double Value { get; set; }

    public IReadOnlyDictionary<string, string>? Labels { get; set; }
}
