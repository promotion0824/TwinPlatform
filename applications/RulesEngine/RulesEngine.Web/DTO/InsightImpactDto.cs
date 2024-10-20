using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="InsightImpactDto" />
/// </summary>
public class InsightImpactDto
{
    /// <summary>
    /// Creates a <see cref="InsightImpactDto" /> from an <see cref="ImpactScore" />
    /// </summary>
    public InsightImpactDto(ImpactScore impactScore)
    {
        this.Name = impactScore.Name;
        this.FieldId = impactScore.FieldId;
        this.Unit = impactScore.Unit;
        this.Score = impactScore.Score;
        this.ExternalId = impactScore.ExternalId;
    }

    /// <summary>
    /// The name of the impact score
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The field id of the impact score
    /// </summary>
    public string FieldId { get; init; }

    /// <summary>
    /// The ADX external id of the impact score
    /// </summary>
    public string ExternalId { get; init; }

    /// <summary>
    /// The value of the impact score
    /// </summary>
    public double Score { get; init; }

    /// <summary>
    /// The unit of measure for the score
    /// </summary>
    public string Unit { get; init; }
}
