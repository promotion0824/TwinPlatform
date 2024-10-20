using Willow.Rules.Model;

#nullable disable  // just a poco

namespace RulesEngine.Web;

/// <summary>
/// Additionaltwin relationship information sent to command
/// </summary>
public class RuleTriggerBoundRelationshipDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public RuleTriggerBoundRelationshipDto(RuleTriggerBoundRelationship v)
    {
        TwinId = v.TwinId;
        TwinName = v.TwinName;
        ModelId = v.ModelId;
        RelationshipType = v.RelationshipType; 
    }

    /// <summary>
    /// Gets or sets the ID of the twin at the other end of the relationship.
    /// </summary>
    public string TwinId { get; init; }

    /// <summary>
    /// Gets or sets the name of the twin at the other end of the relationship.
    /// </summary>
    public string TwinName { get; init; }

    /// <summary>
    /// Gets or sets the model ID of the twin at the other end of the relationship.
    /// </summary>
    public string ModelId { get; init; }

    /// <summary>
    /// Gets or sets the type of relationship between the two twins.
    /// </summary>
    /// <example>
    /// - isCapabilityOf
    /// - hostedBy
    /// - locatedIn.
    /// </example>
    public string RelationshipType { get; init; }
}
