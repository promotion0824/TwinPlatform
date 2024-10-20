namespace Willow.MappedTopologyIngestionApi.HealthChecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Willow.HealthChecks;

    /// <summary>
    /// Checks the health of the TwinsApi.
    /// </summary>
    public class HealthCheckTwinsApi : HealthCheckBase<string>
    {
        /// <summary>
        /// Not configured.
        /// </summary>
        public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Twins Api client not configured.");

        /// <summary>
        /// Failing Calls.
        /// </summary>
        public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect to Twins Api.");
    }
}
