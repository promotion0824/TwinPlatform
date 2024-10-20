using Abodit.Mutable;

#nullable disable // DTO

namespace Willow.Rules.Model;

/// <summary>
/// Result from the metagraph call
/// </summary>
public class MetaGraphResult
{
	/// <summary>
	/// Gets the graph
	/// </summary>
	public Graph<MetaGraphNode, MetaGraphRelation> Graph { get; set; }

	public int TwinCount { get; set; }

	public int CapabilitiesCount { get; set; }

	public int RelationshipCount { get; set; }

}