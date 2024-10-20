using System;
using System.Collections.Generic;

namespace Willow.Rules.Model;

/// <summary>
/// A graph node that is a twin
/// </summary>
public struct TwinNodeDto : IEquatable<TwinNodeDto>
{
	/// <summary>
	/// A local id just for this sub graph
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Id of the twin
	/// </summary>
	public string TwinId { get; set; }

	/// <summary>
	/// Name of the twin
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Description of the twin
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Model Id like dtmi:...
	/// </summary>
	public string ModelId { get; set; }

	/// <summary>
	/// Interval that we expect this trend to follow
	/// </summary>
	public int? TrendInterval { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// Position (supply, return, ...)
	/// </summary>
	public string Position { get; set; }

	/// <summary>
	/// Is a selected node in the graph
	/// </summary>
	public bool IsSelected { get; set; }

	/// <summary>
	/// This is used client side, here just for convenience, all grouping is client-side now
	/// </summary>
	public bool IsCollapsed { get; set; }

	/// <summary>
	/// This is used client side, here just for convenience, all grouping is client-side now
	/// </summary>
	public bool IsExpanded { get; set; }

	/// <summary>
	/// When the graph is too dense nodes with the same group key can be collapsed to a node
	/// </summary>
	public string GroupKey { get; set; }

	/// <summary>
	/// When a group is too large we have a second group key
	/// </summary>
	public string GroupKey2 { get; set; }

	/// <summary>
	/// Used client-side during expand/collapse node groupings
	/// </summary>
	public string CollapseKey { get; set; }

	/// <summary>
	/// The Expression on a calculated point
	/// </summary>
	public string ValueExpression { get; set; }

	/// <summary>
	/// Additional properties of the twin
	/// </summary>
	public Dictionary<string, object> Contents { get; set; }

	/// <summary>
	/// Equality
	/// </summary>
	public bool Equals(TwinNodeDto other) => (this.Id, this.Name, this.ModelId, this.Unit, this.Position)
		.Equals((other.Id, other.Name, other.ModelId, other.Unit, other.Position));
}