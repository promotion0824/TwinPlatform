namespace Willow.LiveData.TelemetryStreaming.Metrics;

using System.Diagnostics.Metrics;
using Willow.Telemetry;

internal class MetricsCollector(Meter meter, MetricsAttributesHelper metricsAttributesHelper) : IMetricsCollector
{
    private const string MetricsPrefix = "TelemetryStreaming-";

    private readonly Counter<long> connectCounter = meter.CreateCounter<long>($"{MetricsPrefix}Connect", description: "The number of successful MQTT connections by the processor");

    private readonly Counter<long> disconnectCounter = meter.CreateCounter<long>($"{MetricsPrefix}Disconnect", description: "The number of MQTT disconnections by the processor");

    private readonly Counter<double> latencyMsCounter = meter.CreateCounter<double>($"{MetricsPrefix}LatencyMs", description: "The latency between SourceTimeStamp and when received by the processor");

    public void TrackMqttConnectCount(long value, IDictionary<string, string>? dimensions) => UpdateCounter(connectCounter, value, dimensions);

    public void TrackMqttDisconnectCount(long value, IDictionary<string, string>? dimensions) => UpdateCounter(disconnectCounter, value, dimensions);

    public void TrackMqttTelemetryLatency(double value, IDictionary<string, string>? dimensions) => UpdateCounter<double>(latencyMsCounter, value, dimensions);

    private void UpdateCounter<T>(Counter<T> counter, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        var extraDimensions = dimensions?.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray() ?? [];

        counter.Add(value, metricsAttributesHelper.GetValues(extraDimensions));
    }

    private void UpdateHistogram<T>(Histogram<T> histogram, T value, IDictionary<string, string>? dimensions)
        where T : struct
    {
        var extraDimensions = dimensions?.Select(dimension => new KeyValuePair<string, object?>(dimension.Key, dimension.Value)).ToArray() ?? [];

        histogram.Record(value, metricsAttributesHelper.GetValues(extraDimensions));
    }
}
