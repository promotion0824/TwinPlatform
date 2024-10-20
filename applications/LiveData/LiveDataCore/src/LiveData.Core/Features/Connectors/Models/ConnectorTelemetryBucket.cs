namespace Willow.LiveData.Core.Features.Connectors.Models;

using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class ConnectorTelemetryBucket
{
    public string ConnectorId { get; set; }

    public DateTime StartTimestamp { get; set; }

    public DateTime EndTimestamp { get; set; }

    public int TotalTelemetryCount { get; set; }

    public int UniqueCapabilityCount { get; set; }

    public int ExpectedTelemetryCount { get; set; }
}
