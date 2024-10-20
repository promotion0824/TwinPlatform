using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A reference that may exist for a global
/// </summary>
public class RuleReferenceDto
{
    /// <summary>
    /// Constructor for <see cref="RuleReferenceDto"/>
    /// </summary>
    public RuleReferenceDto(GlobalVariable global)
    {
        ReferenceType = global.VariableType.ToString();
        Id = global.Id;
        Name = global.Name;
    }

    /// <summary>
    /// Constructor for <see cref="RuleReferenceDto"/>
    /// </summary>
    public RuleReferenceDto(Rule rule)
    {
        ReferenceType = "Rule";
        Id = rule.Id;
        Name = rule.Name;
    }

    /// <summary>
    /// Is this a rule reference
    /// </summary>
    public string ReferenceType { get; init; }

    /// <summary>
    /// The referenced object's id
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The referenced object's name
    /// </summary>
    public string Name { get; init; }
}
