#pragma warning disable CS8618 // Nullable fields in DTO

namespace WillowRules.DTO;

/// <summary>
/// A dto for relationships between models
/// </summary>
public class ModelSimpleRelationshipDto
{
	/// <summary>
	/// Id of the start node
	/// </summary>
	public int StartId { get; init; }

	/// <summary>
	/// Id of the end node
	/// </summary>
	public int EndId { get; init; }

	/// <summary>
	/// The relationship name
	/// </summary>
	public string Relationship { get; init; }

	/// <summary>
	/// The substance, water, air, chilled water, ...
	/// </summary>
	public string Substance { get; init; }
}

#pragma warning restore CS8618 // Nullable fields in DTO
