#pragma warning disable CS8618 // Nullable fields in DTO

namespace WillowRules.DTO;

/// <summary>
/// Simplified graph to hand back to UI
/// </summary>
public class ModelSimpleGraphDto
{
	/// <summary>
	/// Nodes
	/// </summary>
	public ModelSimpleDto[] Nodes { get; set; }

	/// <summary>
	/// Edges
	/// </summary>/
	public ModelSimpleRelationshipDto[] Relationships { get; set; }
}

#pragma warning restore CS8618 // Nullable fields in DTO
