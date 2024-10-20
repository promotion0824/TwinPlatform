namespace ConnectorCore.Services
{
    using System;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IEventNotificationService
    {
        Task EventHubNotifyAsync(string connectionString, ConnectorEntity connectorEntity);

        Task EventHubNotifyAsync(Guid clientId, ConnectorEntity connectorEntity);

        Task<string> GetConnectionString(Guid clientId);
    }
}
