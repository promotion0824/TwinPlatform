namespace Willow.LiveData.Core.Features.TimeSeries.Extensions;

using Willow.LiveData.Core.Features.TimeSeries.Models;
using Willow.LiveData.Pipeline;

internal static class ModelExtensions
{
    public static Telemetry MapTo(this IncomingTelemetry telemetry)
    {
        ArgumentNullException.ThrowIfNull(telemetry);

        return new Telemetry()
        {
            Altitude = telemetry.Altitude,
            Latitude = telemetry.Latitude,
            Longitude = telemetry.Longitude,
            ConnectorId = telemetry.ConnectorId,
            DtId = telemetry.DtId,
            EnqueuedTimestamp = DateTime.UtcNow,
            ExternalId = telemetry.ExternalId,
            Properties = telemetry.Properties,
            ScalarValue = telemetry.ScalarValue,
            SourceTimestamp = telemetry.SourceTimestamp,
        };
    }
}
