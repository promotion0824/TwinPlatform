namespace Willow.LiveData.IoTHubAdaptor.Services;

using Willow.LiveData.IoTHubAdaptor.Models;

internal interface ITransformService
{
    IEnumerable<Pipeline.Telemetry> ProcessMessage(UnifiedTelemetryMessage inputMessage, out int skipped);
}
