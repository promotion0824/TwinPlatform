using System;
using System.Text.Json.Serialization;

// Poco classes
#nullable disable

namespace Willow.Rules.Model
{
	/// <summary>
	/// An optional, helper class for deserializing a digital twin. The $metadata class
	///  on a Azure.DigitalTwins.Core.BasicDigitalTwin.
	/// </summary>
	public class DigitalTwinMetadataPoco
	{
		public DigitalTwinMetadataPoco() { }

		//
		// Summary:
		//     The Id of the model that the digital twin or component is modeled by.
		[JsonPropertyName("$model")]
		public string ModelId { get; set; }

		/// <summary>
		/// The date and time the twin was last updated
		/// </summary>
		[JsonPropertyName("$lastUpdateTime")]
		public DateTimeOffset? LastUpdatedOn { get; set; }

		////
		//// Summary:
		////     This field will contain metadata about changes on properties on the digital twin.
		////     The key will be the property name, and the value is the metadata.
		//public IDictionary<string, DigitalTwinPropertyMetadata> PropertyMetadata { get; set; }
	}
}

