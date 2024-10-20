using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A possible dependency that can be selected for a rule
/// </summary>
public class RuleDependencyListItemDto
{
    /// <summary>
    /// Constructor for <see cref="RuleDependencyListItemDto"/>
    /// </summary>
    public RuleDependencyListItemDto(
        Rule rule,
        RuleMetadata ruleMetadata,
        bool isEnabled,
        string relationship,
        string[] availableRelationships,
        bool circularDependency,
        int distance)
    {
        RuleId = rule.Id;
        RuleName = rule.Name;
        IsEnabled = isEnabled;
        RuleCategory = rule.Category;
        RulePrimaryModelId = rule.PrimaryModelId;
        Relationship = relationship;
        AvailableRelationships = availableRelationships;
        IsRelated = availableRelationships.Contains(RuleDependencyRelationships.RelatedTo);
        IsSibling = availableRelationships.Contains(RuleDependencyRelationships.Sibling);
        IsReferencedCapability = availableRelationships.Contains(RuleDependencyRelationships.ReferencedCapability);
        CircularDependency = circularDependency;
        RuleInstanceCount = ruleMetadata.RuleInstanceCount;
        ValidInstanceCount = ruleMetadata.ValidInstanceCount;
        Distance = distance;
    }

    /// <summary>
    /// The rule id
    /// </summary>
    public string RuleId { get; init; }

    /// <summary>
    /// Whether this dependency is enabled
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// The distance from the rule's model for related rules' models
    /// </summary>
    public int Distance { get; init; }

    /// <summary>
    /// The rule name
    /// </summary>
    public string RuleName { get; init; }

    /// <summary>
    /// The relationship to the rule
    /// </summary>
    public string Relationship { get; init; }

    /// <summary>
    /// The rule category
    /// </summary>
    public string RuleCategory { get; init; }

    /// <summary>
    /// The rule primary model id
    /// </summary>
    public string RulePrimaryModelId { get; init; }

    /// <summary>
    /// The available relationships to this rule
    /// </summary>
    public string[] AvailableRelationships { get; init; }

    /// <summary>
    /// Whether this rule has a fed by relationship to the rule
    /// </summary>
    public bool IsRelated { get; init; }

    /// <summary>
    /// Whether this rule has the same or inherited model id
    /// </summary>
    public bool IsSibling { get; init; }

    /// <summary>
    /// Whether this rule's primary model id is linked to a capability of the rule
    /// </summary>
    public bool IsReferencedCapability { get; init; }

    /// <summary>
    /// An error to indicate if the dependency will create a circular reference
    /// </summary>
    public bool CircularDependency { get; init; }

    /// <summary>
    /// The count of instances that the rule expanded to
    /// </summary>
    public int RuleInstanceCount { get; init; }

    /// <summary>
    /// The count of valid instances of this rule
    /// </summary>
    public int ValidInstanceCount { get; init; }

}
