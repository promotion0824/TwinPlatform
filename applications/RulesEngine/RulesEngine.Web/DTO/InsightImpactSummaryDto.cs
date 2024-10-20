namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="InsightImpactDto" />
/// </summary>
public class InsightImpactSummaryDto
{
    /// <summary>
    /// Creates a <see cref="InsightImpactSummaryDto" />
    /// </summary>
    public InsightImpactSummaryDto(string name, string fieldId, string[] units, int count)
    {
        this.Name = name;
        this.FieldId = fieldId;
        this.Units = units;
        this.Count = count;
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
    /// The units of measure used by all the impact scores having this id
    /// </summary>
    public string[] Units { get; init; }

    /// <summary>
    /// How many times this impact score id is used
    /// </summary>
    public int Count { get; init; }
}
