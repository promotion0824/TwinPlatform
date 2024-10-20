using System;

// POCO class
#nullable disable

namespace RulesEngine.UtilsApi.DTO;

/// <summary>
/// Equipment item and associated rule instances
/// </summary>
public class EquipmentDto
{
	/// <summary>
	/// ModelId for the equipment, currently just the one
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// Name of the equipment
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Capabilities
	/// </summary>
	public CapabilityDto[] Capabilities { get; set; }

	/// <summary>
	/// Related entities down
	/// </summary>
	public RelatedEntityDto[] InverseRelatedEntities { get; set; }

	/// <summary>
	/// Related entities up
	/// </summary>
	public RelatedEntityDto[] RelatedEntities { get; set; }

	/// <summary>
	/// Trend Id used in some time series values
	/// </summary>
	/// <remarks>
	/// If there's a trendId we match on that otherwise we match on connector Id + external Id
	/// </remarks>
	public string TrendId { get; set; }

	/// <summary>
	/// External Id used in some time series values
	/// </summary>
	/// <remarks>
	/// If there's a trendId we match on that otherwise we match on connector Id + external Id
	/// </remarks>
	public string ExternalId { get; set; }

	/// <summary>
	/// Connector Id used in some time series values
	/// </summary>
	public string ConnectorId { get; set; }

	/// <summary>
	/// Unit of measure (for capabilities)
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// Additional properties
	/// </summary>
	public Dictionary<string, object> Contents { get; set; }
}
