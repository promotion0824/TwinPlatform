namespace Willow.LiveData.Core.Infrastructure.HealthCheck;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.LiveData.Core.Infrastructure.Database.Adx;

/// <summary>
/// Health check for ADX.
/// </summary>
internal class HealthCheckADX : HealthCheckBase<AdxQueryRunner>
{
    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect");

    /// <summary>
    /// Rate limited.
    /// </summary>
    public static readonly HealthCheckResult RateLimited = HealthCheckResult.Degraded("Rate limited");
}
