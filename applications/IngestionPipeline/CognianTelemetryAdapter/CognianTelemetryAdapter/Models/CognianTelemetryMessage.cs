namespace Willow.CognianTelemetryAdapter.Models;

using System.Text.Json;

/// <summary>
/// Telemetry message format received from the IoT Hub.
/// Version defines the shape of the 'values' property.
/// </summary>
internal record CognianTelemetryMessage(string? Topic, long Timestamp, object? Values, Dictionary<string, object>? Telemetry, CognianOrigin? Origin)
{
    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// Represents the origin of the telemetry.
/// </summary>
/// <param name="Device">Device.</param>
internal record CognianOrigin(CognianOriginDevice Device);

/// <summary>
/// Represents a device.
/// </summary>
/// <param name="DeviceId">DeviceId.</param>
internal record CognianOriginDevice(string DeviceId);

/// <summary>
/// Represents a BLE tag.
/// </summary>
/// <param name="Uuid">Uuid.</param>
/// <param name="Major">Major.</param>
/// <param name="Minor">Minor.</param>
internal record CognianBLETag(string Uuid, int Major, int Minor);
