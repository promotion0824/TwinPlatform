// POCO class
#nullable disable

using System.Collections.Generic;
using Willow.Rules.Model;

namespace RulesEngine.Web;

	/// <summary>
	/// Related entity
	/// </summary>
	public struct RelatedEntityDto
	{
		/// <summary>
		/// Id of the twin
		/// </summary>
		public string id { get; set; }

		/// <summary>
		/// Name
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// Model id
		/// </summary>
		public string modelId { get; set; }

		/// <summary>
		/// Relationship name
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
		/// Creates a new <see cref="RelatedEntityDto"/>
		/// </summary>
		public RelatedEntityDto(string name, string id, string modelId, string relationship, string substance, string unit)
		{
			this.name = name;
			this.id = id;
			this.modelId = modelId;
			this.relationship = relationship;
			this.substance = substance;
			this.unit = unit;
		}
	}
