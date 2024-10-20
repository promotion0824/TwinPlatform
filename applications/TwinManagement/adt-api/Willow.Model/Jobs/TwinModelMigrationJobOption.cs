namespace Willow.Model.Jobs;

/// <summary>
/// Twin Model Migration Job Option
/// </summary>
public record TwinModelMigrationJobOption : JobBaseOption
{
    /// <summary>
    /// Model Migration Rule Dictionary.
    /// </summary>
    public Dictionary<string, Dictionary<string, ModelRule[]>> MigrationRules { get; set; } = [];
}

/// <summary>
/// Create Relationship Action.
/// </summary>
/// <param name="TargetModelId">Type of target model sibling twin to create the relationship with.</param>
/// <param name="RelationshipName">Name of the relationship.</param>
/// <param name="RelationshipDirection">Direction of the relationship.<see cref="RelationshipDirection"/>.</param>
public record CreateRelationshipAction(string TargetModelId, string RelationshipName, RelationshipDirection RelationshipDirection);

/// <summary>
/// Model Rule Record.
/// </summary>
/// <param name="OldModelId">Id of the old model.</param>
/// <param name="NewModelId">Id of the new model.</param>
/// <param name="CreateRels">List of relationship to create for the new model.</param>
public record ModelRule(string OldModelId, string NewModelId, List<CreateRelationshipAction> CreateRels);

/// <summary>
/// Relationship Direction Enum
/// </summary>
public enum RelationshipDirection
{
    Incoming,
    Outgoing,
}
