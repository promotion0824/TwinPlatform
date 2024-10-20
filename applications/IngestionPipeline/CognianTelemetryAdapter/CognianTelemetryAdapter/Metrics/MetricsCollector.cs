namespace Willow.CognianTelemetryAdapter.Metrics;

using System.Diagnostics.Metrics;

internal class MetricsCollector(Meter meter, Willow.Telemetry.MetricsAttributesHelper metricsAttributesHelper) : IMetricsCollector
{
    private const string MetricsPrefix = "CognianTelemetryAdapter-";

    private readonly Counter<long> messagesIngestedCounter = meter.CreateCounter<long>($"{MetricsPrefix}MessagesIngested");

    public void TrackMessagesIngested(long value, IDictionary<string, string>? dimensions) => UpdateCounter(messagesIngestedCounter, value, dimensions);

    private void UpdateCounter<T>(Counter<T> counter, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        var extraDimensions = dimensions?.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray() ?? [];

        counter.Add(value, metricsAttributesHelper.GetValues(extraDimensions));
    }
}
