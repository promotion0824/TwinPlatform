namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Connector status.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConnectorStatusDto
{
    /// <summary>
    /// Gets or sets the status timestamp.
    /// </summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>
    /// Gets or sets the connector set state.
    /// </summary>
    public string SetState { get; set; }

    /// <summary>
    /// Gets or sets the connector status.
    /// </summary>
    public string Status { get; set; }
}
