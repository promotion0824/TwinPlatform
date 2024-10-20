// -----------------------------------------------------------------------
// <copyright file="ITelemetrySender.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Services.Abstractions;

using Willow.LiveData.TelemetryDataQuality.Models;

/// <summary>
/// Represents a telemetry sender that sends to telemetry data quality eventhub.
/// </summary>
internal interface ITelemetrySender
{
    public Task SendAsync(TelemetryDataQuality telemetry, CancellationToken cancellationToken = default);

    public Task SendAsync(IEnumerable<TelemetryDataQuality> batch, CancellationToken cancellationToken = default);
}
