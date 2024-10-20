namespace ConnectorCore.Services;

using Azure.DigitalTwins.Core;
using ConnectorCore.Models;
using Willow.AzureDigitalTwins.SDK.Client;

internal sealed class DigitalTwinService(ITwinsClient twinsClient) : IDigitalTwinService
{
    private const string ConnectorApplicationModelId = "dtmi:com:willowinc:ConnectorApplication;1";

    public async Task UpsertConnectorApplication(ConnectorApplication connectorApplication, CancellationToken cancellationToken = default)
    {
        await twinsClient.UpdateTwinAsync(new BasicDigitalTwin
        {
            Id = connectorApplication.Id,
            Metadata = new DigitalTwinMetadata
            {
                ModelId = ConnectorApplicationModelId,
            },
            Contents = new Dictionary<string, object>
            {
                ["name"] = connectorApplication.Name,
                ["connectorType"] = new
                {
                    id = $"willow-{connectorApplication.ConnectorType}",
                    name = connectorApplication.ConnectorType,
                },

                // TODO: Uncomment when ConnectorApplication ontology supports enabled and interval
                // ["interval"] = connectorApplication.Interval,
                // ["isEnabled"] = connectorApplication.IsEnabled,
            },
        },
            true,
            cancellationToken);
    }
}
