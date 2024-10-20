// POCO class
#nullable disable

namespace RulesEngine.Web
{
	/// <summary>
	/// Capability entity
	/// </summary>
	public struct CapabilityDto
	{
		/// <summary>
		/// Id of the twin
		/// </summary>
		public string id { get; set; }

		/// <summary>
		/// Name of the twin
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Model id
		/// </summary>
		public string modelId { get; set; }

		/// <summary>
		/// Relationship
		/// </summary>
		public string relationship { get; set; }

		/// <summary>
		/// Substance
		/// </summary>
		public string substance { get; set; }

		/// <summary>
		/// Unit of measure
		/// </summary>
		public string unit { get; set; }

		/// <summary>
		/// Haystack tags
		/// </summary>
		public string tags { get; set; }

		/// <summary>
		/// Creates a new <see cref="RelatedEntityDto"/>
		/// </summary>
		public CapabilityDto(string name, string id, string modelId, string relationship, string unit, string tags)
		{
			this.name = name;
			this.id = id;
			this.modelId = modelId;
			this.relationship = relationship;
			this.substance = "";
			this.unit = unit;
			this.tags = tags;
		}

	}
}