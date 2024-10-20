namespace Willow.Rules.Model;

// EF
#nullable disable

/// <summary>
/// A related node
/// </summary>
public class MetaGraphRelatedNode
{
	/// <summary>
	/// Relationship
	/// </summary>
	public string Relationship { get; set; }

	/// <summary>
	/// Node
	/// </summary>
	public MetaGraphNode Node { get; set; }
}
