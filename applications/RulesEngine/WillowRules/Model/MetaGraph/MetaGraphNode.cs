using Abodit.Graph;
using System;
using System.Collections.Generic;

namespace Willow.Rules.Model
{
	/// <summary>
	/// Meta node for summary graph
	/// </summary>
	public class MetaGraphNode : IEquatable<MetaGraphNode>, IDotGraphNode
	{
		/// <summary>
		/// Gets the DTDL model Id
		/// </summary>
		public string ModelId { get; init; }

		/// <summary>
		/// Get a name for the UI
		/// </summary>
		public string Name { get; set; }

		///<summary>
		///     A language dictionary that contains the localized display names as specified
		///     in the model definition.
		///</summary>
		public IReadOnlyDictionary<string, string> LanguageDisplayNames { get; set; }

		///<summary>
		///     A language dictionary that contains the localized descriptions as specified in
		///     the model definition.
		///</summary>
		public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; }

		/// <summary>
		/// Gets or sets the aggregated set of units in use for this model type
		/// </summary>
		public IList<string> Units { get; init; }

		/// <summary>
		/// Get the decommissioned flag
		/// </summary>
		public bool Decommissioned { get; }

		/// <summary>
		/// Gets a count of nodes
		/// </summary>
		/// <remarks>
		/// If this is zero the node is an abstract type and has never been instantiated in the Twin
		/// </remarks>
		public int Count { get; set; }

		/// <summary>
		/// Counts nodes that inherit from this type also
		/// </summary>
		public int CountWithInherited { get; set; }


		/// <summary>
		/// Creates a new <see cref="MetaGraphNode" />
		/// </summary>
		/// <remarks>
		/// For deserialization only
		/// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public MetaGraphNode()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{

		}

		/// <summary>
		/// Creates a new <see cref="MetaGraphNode" />
		/// </summary>
		public MetaGraphNode(ModelData model)
		{
			ArgumentNullException.ThrowIfNull(model);
			this.Name = model.Label;
			this.ModelId = model.Id;
			this.Decommissioned = model.Decommissioned ?? false;
			this.Units = new List<string>(); // added by scan
			this.Count = 0; // set by scan
			this.CountWithInherited = 0; // set by scan
			this.LanguageDescriptions = model?.LanguageDescriptions ?? new Dictionary<string, string>();
			this.LanguageDisplayNames = model?.LanguageDisplayNames ?? new Dictionary<string, string>();
		}

		public bool Equals(MetaGraphNode? other)
		{
			return other is MetaGraphNode m && m.ModelId == this.ModelId;
		}

		public string DotProperties => $"[label=\"{this.ModelId}{(string.Join(",", this.Units))} ({this.Count})\"]";

		private static int idGen = 1000;

		private int id = idGen++;

		public int Id => this.id;

		public bool IsPruned => false;

		public bool IsStartNode => false;

	}
}
