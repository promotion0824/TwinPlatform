namespace ConnectorCore.Data.Models;

/// <summary>
/// Represents a connector.
/// </summary>
public class Connector
{
    /// <summary>
    /// Gets or sets the unique identifier for the connector.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the connector.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the client associated with the connector.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the site associated with the connector.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Gets or sets the configuration details for the connector.
    /// </summary>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the type of connector.
    /// </summary>
    public Guid ConnectorTypeId { get; set; }

    /// <summary>
    /// Gets or sets the threshold for errors before the connector is disabled.
    /// </summary>
    public int ErrorThreshold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connector is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled for the connector.
    /// </summary>
    public bool IsLoggingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the registration identifier for the connector.
    /// </summary>
    public string RegistrationId { get; set; }

    /// <summary>
    /// Gets or sets the registration key for the connector.
    /// </summary>
    public string RegistrationKey { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last import operation.
    /// </summary>
    public DateTime? LastImport { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last update to the connector.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the type of connection used by the connector.
    /// </summary>
    public string ConnectionType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connector is archived.
    /// </summary>
    public bool? IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the type of the connector.
    /// </summary>
    public ConnectorType ConnectorType { get; set; }

    /// <summary>
    /// Gets or sets the collection of logs associated with the connector.
    /// </summary>
    public ICollection<Log> Logs { get; set; }

    /// <summary>
    /// Gets or sets the collection of scans associated with the connector.
    /// </summary>
    public ICollection<Scan> Scans { get; set; }
}
