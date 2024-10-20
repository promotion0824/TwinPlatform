namespace Willow.MappedTelemetryAdaptor;

using Azure.Messaging.EventHubs;
using Willow.LiveData.Pipeline;

internal class Version2ProcessorFilter : IProcessorFilter
{
    public bool OnDataReceived(EventData eventData)
    {
        if (!eventData.Properties.TryGetValue("v", out var version))
        {
            return false;
        }

        return int.TryParse(version.ToString(), out var versionNumber) && versionNumber == 2;
    }
}
