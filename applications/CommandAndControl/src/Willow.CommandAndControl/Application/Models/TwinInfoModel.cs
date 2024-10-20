namespace Willow.CommandAndControl.Application.Models;

/// <summary>
/// Twin Info Model.
/// </summary>
public record TwinInfoModel
{
    /// <summary>
    /// Gets or sets the Twin ID or Sensor ID.
    /// </summary>
    public string TwinId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the External ID.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the present value of Twin.
    /// </summary>
    public double? PresentValue { get; set; }

    /// <summary>
    /// Gets or sets the site ID.
    /// </summary>
    public string SiteId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connector ID.
    /// </summary>
    public string? ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the asset that hosts the twin.
    /// </summary>
    public string IsHostedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the asset which has this capability.
    /// </summary>
    public string IsCapabilityOf { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unit of the twin's value.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location hierarchy of the twin.
    /// </summary>
    public List<LocationTwin> Locations { get; set; } = [];
}
