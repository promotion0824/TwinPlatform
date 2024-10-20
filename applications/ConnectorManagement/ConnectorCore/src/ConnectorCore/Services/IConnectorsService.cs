namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Dtos;
    using ConnectorCore.Entities;

    internal interface IConnectorsService
    {
        Task NotifyStateEventAsync(ConnectorEntity item);

        Task PublishToServiceBusAsync(ConnectorEntity connector, ConnectorUpdateStatus status);

        Task RegisterDevice(ConnectorEntity connector, bool updateDbEntity = false);

        Task UpsertConnectorApplication(ConnectorEntity connector);
    }
}
