namespace Willow.ConnectorReliabilityMonitor
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;

    internal class AdxHealthCheck(IHealthMetricsRepository healthMetricsRepository, IOptions<AdxQueryConfig> adxQueryConfig) : IHealthCheck
    {
        private static readonly DateTime ApplicationStartTime = DateTime.UtcNow;

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var timeSinceStartup = DateTime.UtcNow - ApplicationStartTime;

            var lastQueryTime = (DateTime?)healthMetricsRepository.GetMetric(HealthMetricKey.LastSuccessfullQueryTime);
            var connectorEnabledCount = (int?)healthMetricsRepository.GetMetric(HealthMetricKey.ConnectorEnabledCount) ?? 0;

            if (lastQueryTime.HasValue && (DateTime.UtcNow - lastQueryTime.Value).TotalSeconds < adxQueryConfig.Value.ConnectorUpdateIntervalSeconds)
            {
                return Task.FromResult(HealthCheckResult.Healthy("OK"));
            }

            if (timeSinceStartup < TimeSpan.FromSeconds(adxQueryConfig.Value.ConnectorUpdateIntervalSeconds))
            {
                return Task.FromResult(HealthCheckResult.Healthy("Starting up"));
            }

            if (connectorEnabledCount == 0)
            {
                return Task.FromResult(HealthCheckResult.Healthy("No connectors enabled"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("No queries since " + lastQueryTime?.ToString()));
        }
    }
}
