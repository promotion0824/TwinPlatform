using System;
using Abodit.Graph;

namespace WillowRules.DTO;

/// <summary>
/// Serializable edge for use in SerializableGraph
/// </summary>
public struct SerializableEdge<E>
	where E : notnull, IEquatable<E>, IRelation
{
	/// <summary>
	/// Id of start node
	/// </summary>
	public string StartId { get; set; }

	/// <summary>
	/// Id of end node
	/// </summary>
	public string EndId { get; set; }

	/// <summary>
	/// Edge
	/// </summary>
	public E Edge { get; set; }
}
