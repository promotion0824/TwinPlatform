namespace Willow.Telemetry;

/// <summary>
/// Defines the types of metrics that can be collected.
/// </summary>
public enum MetricType
{
    /// <summary>
    /// A metric type that counts occurrences of an event.
    /// </summary>
    Counter,

    /// <summary>
    /// A metric type that records the statistical distribution of a set of values.
    /// </summary>
    Histogram,
}
