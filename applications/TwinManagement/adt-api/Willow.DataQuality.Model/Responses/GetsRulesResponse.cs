using Willow.DataQuality.Model.Rules;

namespace Willow.DataQuality.Model.Responses;

/// <summary>
/// A list of data quality rule templates
/// </summary>
public class GetRulesResponse
{
    public IEnumerable<RuleTemplate> Rules { get; set; } = Enumerable.Empty<RuleTemplate>();
}
