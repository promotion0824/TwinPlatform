namespace ConnectorCore.Data.Models;

/// <summary>
/// Represents a connector type.
/// </summary>
public class ConnectorType
{
    /// <summary>
    /// Gets or sets the unique identifier for the connector type.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the connector type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the connector configuration schema.
    /// </summary>
    public Guid ConnectorConfigurationSchemaId { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the device metadata schema.
    /// </summary>
    public Guid DeviceMetadataSchemaId { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the point metadata schema.
    /// </summary>
    public Guid PointMetadataSchemaId { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the scan configuration schema.
    /// </summary>
    public Guid? ScanConfigurationSchemaId { get; set; }

    /// <summary>
    /// Gets or sets the collection of connectors associated with this connector type.
    /// </summary>
    public ICollection<Connector> Connectors { get; set; }
}
