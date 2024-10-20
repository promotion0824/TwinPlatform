// -----------------------------------------------------------------------
// <copyright file="HealthCheckTwinsApi.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// A health check for Twins-Api.
/// </summary>
public class HealthCheckTwinsApi : HealthCheckBase<string>
{
    /// <summary>
    /// Authorization failure.
    /// </summary>
    public static readonly HealthCheckResult AuthorizationFailure = HealthCheckResult.Degraded("Authorization failure");

    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}
