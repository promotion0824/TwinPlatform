namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Connector Telemetry Bucket DTO.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConnectorTelemetryBucketDto
{
    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the total telemetry count.
    /// </summary>
    public int TotalTelemetryCount { get; set; }

    /// <summary>
    /// Gets or sets the unique capability count.
    /// </summary>
    public int UniqueCapabilityCount { get; set; }

    /// <summary>
    /// Gets or sets the expected telemetry count.
    /// </summary>
    public int ExpectedTelemetryCount { get; set; }

    /// <summary>
    /// Gets or sets the set state.
    /// </summary>
    public string SetState { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; }
}
