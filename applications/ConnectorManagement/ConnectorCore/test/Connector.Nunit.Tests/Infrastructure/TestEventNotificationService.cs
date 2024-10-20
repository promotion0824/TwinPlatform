namespace Connector.Nunit.Tests.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Services;

    public class TestEventNotificationService : IEventNotificationService
    {
        public async Task EventHubNotifyAsync(string connectionString, ConnectorEntity connectorEntity)
        {
            await Task.FromResult<string>(null);
        }

        public async Task EventHubNotifyAsync(Guid clientId, ConnectorEntity connectorEntity)
        {
            await Task.FromResult<string>(null);
        }

        public async Task<string> GetConnectionString(Guid clientId)
        {
            return await Task.FromResult<string>(null);
        }
    }
}
