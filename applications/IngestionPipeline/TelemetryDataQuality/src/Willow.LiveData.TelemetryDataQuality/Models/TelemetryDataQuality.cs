// -----------------------------------------------------------------------
// <copyright file="TelemetryDataQuality.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Models;

/// <summary>
/// A representation of the input to the telemetry data quality table.
/// </summary>
public record TelemetryDataQuality
{
    /// <summary>
    /// Gets or sets the connector ID.
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the telemetry was generated.
    /// </summary>
    public DateTimeOffset SourceTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the telemetry was added to the queue.
    /// </summary>
    public DateTimeOffset EnqueuedTimestamp { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of when the validation for the timeseries last changed.
    /// </summary>
    public DateTimeOffset LastValidationUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the digital twin ID.
    /// </summary>
    public string? DtId { get; set; }

    /// <summary>
    /// Gets or sets the external ID of the twin.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets a dynamic object containing validation results for a timeseries.
    /// </summary>
    public dynamic? ValidationResults { get; set; }
}
