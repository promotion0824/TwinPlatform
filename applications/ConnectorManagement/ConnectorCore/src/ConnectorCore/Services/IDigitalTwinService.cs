namespace ConnectorCore.Services;

internal interface IDigitalTwinService
{
    Task UpsertConnectorApplication(ConnectorCore.Models.ConnectorApplication connectorApplication, CancellationToken cancellationToken = default);
}
