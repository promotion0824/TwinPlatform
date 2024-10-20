namespace Willow.LiveData.Core.Domain;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Represents the telemetry request body.
/// </summary>
[ExcludeFromCodeCoverage]
public class TelemetryRequestBody
{
    /// <summary>
    /// Gets or sets the ConnectorId.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the list of TwinIds.
    /// </summary>
    public List<string> DtIds { get; set; }

    /// <summary>
    /// Gets or sets the list of trend IDs for telemetry data.
    /// </summary>
    public List<string> TrendIds { get; set; }
}
