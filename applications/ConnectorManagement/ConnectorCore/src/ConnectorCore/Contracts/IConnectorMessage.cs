namespace ConnectorCore.Contracts;

using System;
using ConnectorCore.Entities;
using MassTransit;

/// <summary>
/// Represents the service bus message contract for connector updates.
/// </summary>
[EntityName("Deployment-Dashboard-ConnectorUpdates")]
public interface IConnectorMessage
{
    /// <summary>
    /// Gets or sets the ConnectorId.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the CustomerId associated with the connector.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the SiteId associated with the connector.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Gets or sets the Name of the connector.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the ConnectorType.
    /// </summary>
    public string ConnectorType { get; set; }

    /// <summary>
    /// Gets or sets the ConnectionType.
    /// </summary>
    public string ConnectionType { get; set; }

    /// <summary>
    /// Gets or sets the Timestamp of the connector update.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets a value indicating the Enabled status of the connector.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connector is Archived.
    /// </summary>
    public bool Archived { get; set; }

    /// <summary>
    /// Gets or sets the type of update associated with the message.
    /// </summary>
    public ConnectorUpdateStatus Status { get; set; }
}
