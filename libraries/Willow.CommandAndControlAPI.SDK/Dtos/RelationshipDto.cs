namespace Willow.CommandAndControlAPI.SDK.Dtos;

/// <summary>
/// Represents a relationship twin.
/// </summary>
public class RelationshipDto
{
    /// <summary>
    /// Gets or sets the ID of the twin at the other end of the relationship.
    /// </summary>
    public required string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the name of the twin at the other end of the relationship.
    /// </summary>
    public required string TwinName { get; set; }

    /// <summary>
    /// Gets or sets the model ID of the twin at the other end of the relationship.
    /// </summary>
    public required string ModelId { get; set; }

    /// <summary>
    /// Gets or sets the type of relationship between the two twins.
    /// </summary>
    /// <example>
    /// - isCapabilityOf
    /// - hostedBy
    /// - locatedIn.
    /// </example>
    public required string RelationshipType { get; set; }
}
