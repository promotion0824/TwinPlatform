namespace Willow.Telemetry;

using System.Diagnostics.Metrics;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Wrapper for tracking metrics.
/// </summary>
public class MetricsCollector : IMetricsCollector
{
    private readonly Dictionary<string, object> metrics = new();
    private readonly Meter meter;
    private readonly MetricsAttributesHelper metricsAttributesHelper;
    private readonly string prefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsCollector"/> class.
    /// </summary>
    /// <param name="configuration">Configuration of the WillowContext.</param>
    /// <param name="meter">Meter.</param>
    /// <param name="metricsAttributesHelper">Metric attribute helper.</param>
    public MetricsCollector(Microsoft.Extensions.Configuration.IConfiguration configuration, Meter meter, MetricsAttributesHelper metricsAttributesHelper)
    {
        this.meter = meter;
        this.metricsAttributesHelper = metricsAttributesHelper;
        var context = configuration.GetSection("WillowContext").Get<Willow.AppContext.WillowContextOptions>();
        prefix = context?.AppName ?? System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
    }

    /// <inheritdoc />
    public void TrackMetric<T>(string name, T value, MetricType metricType, string? description = null, IDictionary<string, string>? dimensions = null)
        where T : struct
    {
        var namePrefixed = $"{prefix}-{name}";
        switch (metricType)
        {
            case MetricType.Counter:
                var counter = GetOrCreateMetric(namePrefixed, () => meter.CreateCounter<T>(namePrefixed, description: description));
                UpdateCounter(counter, value, dimensions);
                break;

            case MetricType.Histogram:
                var histogram = GetOrCreateMetric(namePrefixed, () => meter.CreateHistogram<T>(namePrefixed, description: description));
                UpdateHistogram(histogram, value, dimensions);
                break;

            default:
                throw new InvalidOperationException($"Unsupported metric type: {metricType} for metric {namePrefixed}.");
        }
    }

    private T GetOrCreateMetric<T>(string name, Func<T> createFunc)
        where T : class
    {
        if (!metrics.TryGetValue(name, out var metric))
        {
            metric = createFunc();
            metrics[name] = metric;
        }

        return metric as T ?? throw new InvalidOperationException($"Failed to create or cast metric with name {name}.");
    }

    private void UpdateCounter<T>(Counter<T>? counter, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        if (counter == null)
        {
            return;
        }

        var extraDimensions = dimensions?.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray() ?? [];

        counter.Add(value, metricsAttributesHelper.GetValues(extraDimensions));
    }

    private void UpdateHistogram<T>(Histogram<T>? histogram, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        if (histogram == null)
        {
            return;
        }

        var extraDimensions = dimensions?.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray() ?? [];

        histogram.Record(value, metricsAttributesHelper.GetValues(extraDimensions));
    }
}
