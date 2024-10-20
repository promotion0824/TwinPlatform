namespace Willow.LiveData.Core.Features.Connectors.Models;

using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class HeartbeatTelemetry
{
    public Guid ConnectorId { get; set; }

    public Guid PointId { get; set; }

    public DateTime Timestamp { get; set; }

    public bool Value { get; set; }
}
