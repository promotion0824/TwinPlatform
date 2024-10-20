namespace Willow.Telemetry;

/// <summary>
/// Metric collector.
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Track metric.
    /// </summary>
    /// <param name="name">Unique name of the metric.</param>
    /// <param name="value">Value of the metric.</param>
    /// <param name="metricType">Type of the metric.</param>
    /// <param name="description">Description.</param>
    /// <param name="dimensions">Extra dimensions.</param>
    /// <typeparam name="T">Data type of the metric.</typeparam>
    void TrackMetric<T>(string name, T value, MetricType metricType, string? description = null, IDictionary<string, string>? dimensions = null)
        where T : struct;
}
