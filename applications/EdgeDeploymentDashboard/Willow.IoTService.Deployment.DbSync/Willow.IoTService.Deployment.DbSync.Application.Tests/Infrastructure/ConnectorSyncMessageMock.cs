namespace Willow.IoTService.Deployment.DbSync.Application.Tests.Infrastructure;

using ConnectorCore.Contracts;

public class ConnectorSyncMessageMock : IConnectorMessage
{
    public Guid ConnectorId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid SiteId { get; set; }

    public string Name { get; set; } = "TestConnector";

    public string ConnectorType { get; set; } = "DefaultBacnetConnector";

    public string ConnectionType { get; set; } = "IoTEdge";

    public bool Enabled { get; set; }

    public bool Archived { get; set; }

    public ConnectorUpdateStatus Status { get; set; }

    public DateTime Timestamp { get; set; }
}
