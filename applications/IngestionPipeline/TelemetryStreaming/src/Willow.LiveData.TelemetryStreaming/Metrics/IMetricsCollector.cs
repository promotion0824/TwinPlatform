namespace Willow.LiveData.TelemetryStreaming.Metrics
{
    internal interface IMetricsCollector
    {
        void TrackMqttConnectCount(long value, IDictionary<string, string>? dimensions);

        void TrackMqttDisconnectCount(long value, IDictionary<string, string>? dimensions);

        void TrackMqttTelemetryLatency(double value, IDictionary<string, string>? dictionary);
    }
}
