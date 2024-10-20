namespace Willow.CognianTelemetryAdapter.Metrics
{
    internal interface IMetricsCollector
    {
        void TrackMessagesIngested(long value, IDictionary<string, string>? dimensions);
    }
}
