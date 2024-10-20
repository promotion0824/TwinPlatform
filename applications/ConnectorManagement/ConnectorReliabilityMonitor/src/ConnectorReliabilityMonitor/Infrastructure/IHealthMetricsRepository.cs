namespace Willow.ConnectorReliabilityMonitor.Infrastructure
{
    internal interface IHealthMetricsRepository
    {
        object? GetMetric(HealthMetricKey key);

        void UpdateMetric(HealthMetricKey key, object value);
    }
}
