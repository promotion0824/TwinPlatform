namespace Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;

internal interface IConnectorTypeFactory
{
    IConnectorType GetConnector(string connectionType);
}
