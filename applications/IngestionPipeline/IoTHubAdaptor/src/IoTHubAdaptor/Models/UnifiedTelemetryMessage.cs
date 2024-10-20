namespace Willow.LiveData.IoTHubAdaptor.Models;

using System.Text.Json;

/// <summary>
/// Telemetry message format received from the IoT Hub.
/// Version defines the shape of the 'values' property.
/// </summary>
internal record UnifiedTelemetryMessage(string Version, string? ConnectorId, object Values)
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
