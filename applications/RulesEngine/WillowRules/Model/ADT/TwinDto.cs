using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace WillowRules.DTO;

/// <summary>
/// DTO for a basic digital twin
/// </summary>
/// <remarks>
/// BasicDigitalTwinPoco is not suitable (has JSON attributes specific to ADT)
/// </remarks>
public class TwinDto
{
	/// <summary>
	/// Id
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Legacy Guid (as string) siteId
	/// </summary>
	public Guid? SiteId { get; set; }

	/// <summary>
	/// Legacy unique ID
	/// </summary>
	public Guid? EquipmentUniqueId { get; set; }

	/// <summary>
	/// TrendId
	/// </summary>
	public string TrendID { get; set; }

	/// <summary>
	/// ExternalId is used on some time series values instead of trendId
	/// </summary>
	/// <remarks>
	/// When it is used we also need to check the connector id
	/// </remarks>
	public string ExternalID { get; set; }

	/// <summary>
	/// ConnectorID + ExternalID is used on some time series values instead of trendId
	/// </summary>
	public string ConnectorID { get; set; }

	/// <summary>
	/// Unit
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// Interval that we expect this trend to follow
	/// </summary>
	public int? TrendInterval { get; set; }

	/// <summary>
	/// Position
	/// </summary>
	public string Position { get; set; }

	/// <summary>
	/// Description
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Tags
	/// </summary>
	public Dictionary<string, bool> tags { get; set; }

	/// <summary>
	/// Model
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// Etag
	/// </summary>
	public string ETag { get; set; }

	/// <summary>
	/// Contents contains all the other properties of the twin
	/// </summary>
	public Dictionary<string, object?> Contents { get; set; }

	/// <summary>
	/// Value expression
	/// </summary>
	public string ValueExpression { get; set; }

	/// <summary>
	/// Time zone
	/// </summary>
	public string Timezone { get; set; }

	/// <summary>
	/// The date and time the twin was last updated
	/// </summary>
	public DateTimeOffset? LastUpdatedOn { get; set; }

	/// <summary>
	/// Ascendant locations
	/// </summary>
	public TwinLocation[] Locations { get; set; } = [];

	/// <summary>
	/// Creates a new TwinDto
	/// </summary>
	/// <param name="twin"></param>
	public TwinDto(BasicDigitalTwinPoco twin)
	{
		if (string.IsNullOrEmpty(twin.Metadata.ModelId)) throw new ArgumentException("ModelId must be present");
		this.Id = twin.Id;
		this.Name = twin.name;
		if (Guid.TryParse(twin.siteID, out Guid siteGuid)) this.SiteId = siteGuid; else this.SiteId = Guid.Empty;
		if (Guid.TryParse(twin.uniqueID, out Guid uniqueGuid)) this.EquipmentUniqueId = uniqueGuid; else this.EquipmentUniqueId = Guid.Empty;
		this.ExternalID = twin.externalID;
		this.ConnectorID = twin.connectorID;
		this.TrendID = twin.trendID;
		this.Unit = twin.unit;
		this.TrendInterval = twin.trendInterval;
		this.ValueExpression = twin.ValueExpression;
		this.Position = twin.position;
		this.Description = twin.description;
		this.tags = twin.tags;
		this.ModelId = twin.Metadata.ModelId;
		this.Timezone = twin.TimeZone?.Name ?? "";
		this.ETag = twin.ETag.HasValue ? twin.ETag.Value.ToString() : "";
		this.Contents = twin.Contents;
		this.LastUpdatedOn = twin.LastUpdatedOn ?? twin.Metadata.LastUpdatedOn;
		this.Locations = twin.Locations;
	}
}
