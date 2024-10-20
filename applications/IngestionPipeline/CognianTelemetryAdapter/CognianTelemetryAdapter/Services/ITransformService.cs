namespace Willow.CognianTelemetryAdapter.Services;

using Willow.CognianTelemetryAdapter.Models;
using Willow.LiveData.Pipeline;

internal interface ITransformService
{
    IEnumerable<Telemetry> ProcessMessage(CognianTelemetryMessage inputMessage);

    IEnumerable<Telemetry> ProcessMessages(IEnumerable<CognianTelemetryMessage> inputMessages);
}
