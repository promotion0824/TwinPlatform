namespace Connector.Nunit.Tests.Infrastructure;

using System.Threading.Tasks;
using ConnectorCore.Models;
using ConnectorCore.Services;

internal class TestDigitalTwinService : IDigitalTwinService
{
    public async Task UpsertConnectorApplication(ConnectorApplication connectorApplication, System.Threading.CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
