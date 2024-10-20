namespace Connector.XL.Features.ConnectorFeatureGroup.ConnectorsFeature;

using System;

/// <summary>
/// Represents a connector entity.
/// </summary>
public class ConnectorEntity
{
    /// <summary>
    /// Gets or sets id of the connector.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets connector's name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets id of the client.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets id of the site.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// Gets or sets connector's configuration.
    /// </summary>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets id of connector's type.
    /// </summary>
    public Guid ConnectorTypeId { get; set; }

    /// <summary>
    /// Gets or sets error threshold.
    /// </summary>
    public int ErrorThreshold { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether flag marking enabled connectors.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether flag marking if logging is enabled for the connector.
    /// </summary>
    public bool IsLoggingEnabled { get; set; }

    /// <summary>
    /// Gets or sets timestamp when connector was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets timestamp of the last import.
    /// </summary>
    public DateTime? LastImport { get; set; }
}
