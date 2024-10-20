using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Willow.Rules.DTO;
using Willow.Rules.Repository;

// EF
#nullable disable

namespace Willow.Rules.Model
{
	/// <summary>
	/// Information about the state of ADT
	/// </summary>
	public class ADTSummary : IId
	{
		/// <summary>
		/// The Id for persistence
		/// </summary>
		[JsonProperty("id")]  // necessary for Cosmos DB?
		public string Id { get; set; }

		/// <summary>
		/// When the record was created
		/// </summary>
		public DateTimeOffset AsOfDate { get; set; }

		/// <summary>
		/// The customer environment
		/// </summary>
		public string CustomerEnvironmentId { get; set; }

		/// <summary>
		/// The ADT instance
		/// </summary>
		public string ADTInstanceId { get; set; }

		/// <summary>
		/// How many twins
		/// </summary>
		public int CountTwins { get; set; }

		/// <summary>
		/// How many twins with trend Ids
		/// </summary>
		public int CountCapabilities { get; set; }

		/// <summary>
		/// How many relationships
		/// </summary>
		public int CountRelationships { get; set; }

		/// <summary>
		/// Count of twins with no relationships
		/// </summary>
		public int CountTwinsNotInGraph { get; set; }

		/// <summary>
		/// Count of models
		/// </summary>
		public int CountModels { get; set; }

		/// <summary>
		/// Count of models in use in twin
		/// </summary>
		public int CountModelsInUse { get; set; }

		/// <summary>
		/// Additional ADT data
		/// </summary>
		public ADTSummaryExtensionData ExtensionData { get; set; } = new ADTSummaryExtensionData();

		/// <summary>
		/// Overall system summary
		/// </summary>
		public SystemSummary SystemSummary { get; set; } = new SystemSummary();
	}

	/// <summary>
	/// Additional ADT data
	/// </summary>
	public class ADTSummaryExtensionData
	{
		/// <summary>
		/// Model summary
		/// </summary>
		public List<ModelSummary> ModelSummary { get; set; } = new List<ModelSummary>();
	}
}


/// <summary>
/// Represents a preoprty reference in ADT
/// </summary>
public class ModelSummary
{
	/// <summary>
	/// The full path property name
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// Properties used in ADT
	/// </summary>
	public List<PropertySummary> PropertyReferences { get; set; } = new();
}


/// <summary>
/// Represents a preoprty reference in ADT
/// </summary>
public class PropertySummary
{
	/// <summary>
	/// The full path property name
	/// </summary>
	public string PropertyName { get; set; }

	/// <summary>
	/// Total twins that has the property as part of its schema
	/// </summary>
	public int TotalDelcared { get; set; }

	/// <summary>
	/// Total twins which has a value for the property
	/// </summary>
	public int TotalUsed { get; set; }
}
