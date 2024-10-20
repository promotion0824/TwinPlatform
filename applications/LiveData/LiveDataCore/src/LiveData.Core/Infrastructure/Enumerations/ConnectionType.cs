namespace Willow.LiveData.Core.Infrastructure.Enumerations;

/// <summary>
/// Connection Type.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// IoT Edge.
    /// </summary>
    IoTEdge = 1,

    /// <summary>
    /// VM.
    /// </summary>
    VM = 2,

    /// <summary>
    /// Stream Analytics IoT Hub.
    /// </summary>
    StreamAnalyticsIoTHub = 3,

    /// <summary>
    /// Stream Analytics Event Hub.
    /// </summary>
    StreamAnalyticsEventHub = 4,

    /// <summary>
    /// Public API.
    /// </summary>
    PublicAPI = 5,
}
