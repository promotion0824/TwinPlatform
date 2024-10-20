namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Connector statistics.
/// </summary>
public class ConnectorStatsDto
{
    /// <summary>
    /// Gets or sets the connector ID.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the connector set state.
    /// </summary>
    public string CurrentSetState { get; set; }

    /// <summary>
    /// Gets or sets the connector current status.
    /// </summary>
    public string CurrentStatus { get; set; }

    /// <summary>
    /// Gets or sets the total number of capabilities.
    /// </summary>
    public int TotalCapabilitiesCount { get; set; }

    /// <summary>
    /// Gets or sets the number of disabled capabilities.
    /// </summary>
    public int DisabledCapabilitiesCount { get; set; }

    /// <summary>
    /// Gets or sets the number of devices hosting the capabilities.
    /// </summary>
    public int HostingDevicesCount { get; set; }

    /// <summary>
    /// Gets the count of telemetry.
    /// </summary>
    public int TotalTelemetryCount => Telemetry != null ?
                 Telemetry.Sum(x => x.TotalTelemetryCount)
                 : 0;

    /// <summary>
    /// Gets or sets the telemetry.
    /// </summary>
    public IEnumerable<ConnectorTelemetryBucketDto> Telemetry { get; set; }

    /// <summary>
    /// Gets or sets detailed status information for the connector.
    /// </summary>
    public IEnumerable<ConnectorStatusDto> Status { get; set; }
}
