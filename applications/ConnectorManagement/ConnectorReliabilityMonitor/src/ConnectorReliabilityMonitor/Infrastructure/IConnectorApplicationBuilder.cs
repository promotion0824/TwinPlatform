namespace Willow.ConnectorReliabilityMonitor.Infrastructure;

internal interface IConnectorApplicationBuilder
{
    Task<IEnumerable<ConnectorApplicationDto>> GetConnectorsAsync();
}
