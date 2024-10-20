namespace Willow.CognitiveSearch;

using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// A health check for search service.
/// </summary>
public class HealthCheckSearch : HealthCheckBase<SearchClient>
{
    /// <summary>
    /// Not configured.
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured");

    /// <summary>
    /// Rebuilding.
    /// </summary>
    public static readonly HealthCheckResult Rebuilding = HealthCheckResult.Degraded("Rebuilding");

    /// <summary>
    /// Rate limited.
    /// </summary>
    public static readonly HealthCheckResult RateLimited = HealthCheckResult.Degraded("Rate limited");

    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");

    /// <summary>
    /// Forbidden.
    /// </summary>
    public static readonly HealthCheckResult Forbidden = HealthCheckResult.Degraded("Forbidden");

    /// <summary>
    /// MissingIndex.
    /// </summary>
    public static readonly HealthCheckResult MissingIndex = HealthCheckResult.Degraded("MissingIndex");
}
