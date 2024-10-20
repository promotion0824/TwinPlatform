using Newtonsoft.Json;
using RulesEngine.Web.DTO;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// A possible dependency that may exist for an insight
/// </summary>
public class InsightDependencyDto
{
    /// <summary>
    /// Constructor for <see cref="InsightDependencyDto"/>
    /// </summary>
    public InsightDependencyDto(InsightDependency insightDependency)
    {
        Relationship = insightDependency.Relationship;
        InsightId = insightDependency.InsightId;
    }

    /// <summary>
    /// Constructor for <see cref="InsightDependencyDto"/>
    /// </summary>
    public InsightDependencyDto(InsightDependency insightDependency, Insight insight)
        : this(insightDependency)
    {
        Insight = new InsightDto(insight);
    }

    /// <summary>
    /// Constructor json
    /// </summary>
    [JsonConstructor]
    private InsightDependencyDto()
    {
    }

    /// <summary>
    /// The relationship to the referenced insight
    /// </summary>
    public string Relationship { get; init; }

    /// <summary>
    /// The referenced insight
    /// </summary>
    public string InsightId { get; init; }

    /// <summary>
    /// The related insight
    /// </summary>
    public InsightDto Insight { get; init; }
}
