using Newtonsoft.Json;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A possible dependency that can be selected for a rule
/// </summary>
public class RuleDependencyDto
{
    /// <summary>
    /// Constructor for <see cref="RuleDependencyDto"/>
    /// </summary>
    public RuleDependencyDto(RuleDependency ruleDependency)
    {
        Relationship = ruleDependency.Relationship;
        RuleId = ruleDependency.RuleId;
    }

    /// <summary>
    /// Constructor json
    /// </summary>
    [JsonConstructor]
    private RuleDependencyDto()
    {
    }

    /// <summary>
    /// The relationship to the referenced rule
    /// </summary>
    public string Relationship { get; init; }

    /// <summary>
    /// The rule id of the referenced rule
    /// </summary>
    public string RuleId { get; init; }
}
