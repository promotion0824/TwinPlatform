namespace Willow.ConnectorReliabilityMonitor.Infrastructure
{
    using System.Collections.Concurrent;

    internal class HealthMetricsRepository : IHealthMetricsRepository
    {
        private readonly ConcurrentDictionary<HealthMetricKey, object> metrics = new();

        public void UpdateMetric(HealthMetricKey key, object value)
        {
            metrics[key] = value;
        }

        public object? GetMetric(HealthMetricKey key)
        {
            metrics.TryGetValue(key, out var value);
            return value;
        }
    }
}
