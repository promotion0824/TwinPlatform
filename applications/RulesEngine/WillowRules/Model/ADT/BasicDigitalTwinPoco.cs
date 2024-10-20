using Azure;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// Poco classes
#nullable disable

namespace Willow.Rules.Model;


/// <summary>
/// JSON.net serializable form of BasicDigitalTwin
/// </summary>
public class BasicDigitalTwinPoco : IEquatable<BasicDigitalTwinPoco>
{
	[JsonConstructor]
	public BasicDigitalTwinPoco() { }

	/// <summary>
	/// Preferred constructor for tests
	/// </summary>
	public BasicDigitalTwinPoco(string dtid)
	{
		if (string.IsNullOrEmpty(dtid)) throw new ArgumentException(nameof(dtid), "Must be a valid id");
		this.Id = dtid;
	}

	/// <summary>
	//     The unique Id of the digital twin in a digital twins instance. This field is
	//     present on every digital twin.
	/// </summary>
	[JsonPropertyName("$dtId")]
	public string Id { get; set; }

	/// <summary>
	/// Display name of the digital twin
	/// </summary>
	[JsonPropertyName("name")]
	public string name { get; set; }

	/// <summary>
	/// Site ID
	/// </summary>
	/// <remarks>
	/// Should be a Guid but who knows!
	/// </remarks>
	[JsonPropertyName("siteID")]
	public string siteID { get; set; }

	/// <summary>
	/// The other unique Id of the digital twin
	/// </summary>
	/// <remarks>
	/// Should be a Guid but who knows!
	/// </remarks>
	[JsonPropertyName("uniqueID")]
	public string uniqueID { get; set; }

	/// <summary>
	/// Yet another form of ID, connectorID + externalID
	/// </summary>
	[JsonPropertyName("connectorID")]
	public string connectorID { get; set; }

	/// <summary>
	/// Yet another form of ID, connectorID + externalID
	/// </summary>
	[JsonPropertyName("externalID")]
	public string externalID { get; set; }

	// /// <summary>
	// /// Multiple other forms of external Id
	// /// </summary>
	// [JsonPropertyName("externalIds")]
	// public Dictionary<string, object> externalIds { get; set; }

	/// <summary>
	/// Trend Id for a time series point capability twin
	/// </summary>
	[JsonPropertyName("trendID")]
	public string trendID { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	[JsonPropertyName("unit")]
	public string unit { get; set; }

	/// <summary>
	/// Interval that we expect this trend to follow
	/// </summary>
	[JsonPropertyName("trendInterval")]
	public int? trendInterval { get; set; }

	/// <summary>
	/// Calculated point expression
	/// </summary>
	[JsonPropertyName("valueExpression")]
	public string ValueExpression { get; set; }

	/// <summary>
	/// TimeZone (only on building or higher nodes)
	/// </summary>
	[JsonPropertyName("timeZone")]
	public TimeZone TimeZone { get; set; }

	/// <summary>
	/// GEts the time zone
	/// </summary>
	public string TimeZoneName()
	{
		return TimeZone?.Name ?? "";
	}

	/// <summary>
	/// Gets the ModelId
	/// </summary>
	public string ModelId()
	{
		return Metadata?.ModelId ?? "";
	}

	/// <summary>
	/// Position like inflow, return, supply, ...
	/// </summary>
	[JsonPropertyName("position")]
	public string position { get; set; }

	/// <summary>
	/// Description
	/// </summary>
	[JsonPropertyName("description")]
	public string description { get; set; }

	/// <summary>
	/// Haystack tags
	/// </summary>
	[JsonPropertyName("tags")]
	public Dictionary<string, bool> tags { get; set; }

	[JsonIgnore]
	public string TagString => tags is null ? "" : string.Join(",", tags.Keys);

	//
	// Summary:
	//     A string representing a weak ETag for the entity that this request performs an
	//     operation against, as per RFC7232.
	//[JsonConverter(typeof(OptionalETagConverter))]
	[JsonPropertyName("$etag")]
	[JsonIgnore]
	public ETag? ETag { get; set; }

	/// <summary>
	///     Information about the model a digital twin conforms to. This field is present
	///     on every digital twin.
	/// </summary>
	[JsonPropertyName("$metadata")]
	public DigitalTwinMetadataPoco Metadata { get; set; }

	/// <summary>
	/// The date and time the twin was last updated
	/// </summary>
	[JsonPropertyName("$lastUpdateTime")]
	public DateTimeOffset? LastUpdatedOn { get; set; }

	/// <summary>
	//     This field will contain properties and components as defined in the contents
	//     section of the DTDL definition of the twin.
	/// </summary>
	/// <remarks>
	// Remarks:
	//     If the property is a component, use the Azure.DigitalTwins.Core.BasicDigitalTwinComponent
	//     class to deserialize the payload.
	/// </remarks>
	[JsonExtensionData]
	[Newtonsoft.Json.JsonExtensionData]
	public Dictionary<string, object> Contents { get; set; }

	/// <summary>
	/// Parent chain by locatedIn and isPartOf
	/// </summary>
	/// <remarks>
	/// This is a flattened list sorted in the ascending direction.
	/// It is suitable for filtering but not for display: use a graph query for display purposes.
	/// This is copied from a RuleInstance when the insight is created
	/// </remarks>
	public TwinLocation[] Locations { get; set; } = [];

	/// <summary>
	/// IEquatable Equals implementation
	/// </summary>
	public bool Equals(BasicDigitalTwinPoco other) => this.Id.Equals(other.Id);

	public override bool Equals(object other) => other is BasicDigitalTwinPoco oth && this.Id.Equals(oth.Id);
	public override int GetHashCode() => this.Id.GetHashCode();
}

/// <summary>
/// Time zone object in twin has a name and metadata
/// </summary>
public class TimeZone
{
	/// <summary>
	/// Name of time zone
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; }
}

/// <summary>
/// An edge from ADT with the relationship and the far end object
/// </summary>
public struct Edge
{
	/// <summary>
	/// Type of relationship
	/// </summary>
	public string RelationshipType { get; set; }

	/// <summary>
	/// Substance
	/// </summary>
	public string Substance { get; set; }

	/// <summary>
	/// The related digital twin
	/// </summary>
	public BasicDigitalTwinPoco Destination { get; set; }
}

