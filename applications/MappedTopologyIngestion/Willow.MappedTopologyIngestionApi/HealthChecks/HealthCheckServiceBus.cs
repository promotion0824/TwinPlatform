namespace Willow.MappedTopologyIngestionApi.HealthChecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Willow.HealthChecks;

    /// <summary>
    /// Health check for the Sync Service.
    /// </summary>
    public class HealthCheckServiceBus : HealthCheckBase<string>
    {
        /// <summary>
        /// Not configured.
        /// </summary>
        public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("ServiceBus not configured.");

        /// <summary>
        /// Failing requests.
        /// </summary>
        public static readonly HealthCheckResult FailedRun = HealthCheckResult.Degraded("ServiceBus processing failed.");
    }
}
