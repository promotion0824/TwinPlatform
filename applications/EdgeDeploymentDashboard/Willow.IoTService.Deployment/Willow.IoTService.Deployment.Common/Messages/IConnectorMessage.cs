using MassTransit;

//MassTransit consumes messages based on namespace and needs to match
namespace ConnectorCore.Contracts;

[EntityName("deployment-dashboard-connectorupdates")]
public interface IConnectorMessage
{
    public Guid ConnectorId { get; set; }
    public Guid CustomerId { get; set; }
    public string Name { get; set; }
    public string ConnectorType { get; set; }
    public string ConnectionType { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Enabled { get; set; }
    public bool Archived { get; set; }
    public ConnectorUpdateStatus Status { get; set; }
}

public enum ConnectorUpdateStatus
{
    New,
    Enable,
    Disable,
    Archive,
    Update,
    Export
}
