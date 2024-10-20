using System;
using System.Text.Json.Serialization;
using Abodit.Graph;

namespace Willow.Rules.Model;

/// <summary>
/// A graph edge between twins
/// </summary>
public struct TwinRelationshipDto : IEquatable<TwinRelationshipDto>, IRelation
{
	public string Id => $"{StartId}-{Name}-{EndId}-{Substance}";

	public int StartId { get; set; }

	public int EndId { get; set; }

	public string Name { get; set; }

	public string Substance { get; set; }

	public string StartTwinId { get; set; }

	public string EndTwinId { get; set; }

	[JsonIgnore]
	public bool IsReflexive => false;

	public bool Equals(TwinRelationshipDto other) =>
		(this.StartId, this.EndId, this.Name, this.Substance).Equals((other.StartId, other.EndId, other.Name, other.Substance));
}
