using Willow.DataQuality.Model.Validation;

namespace Willow.DataQuality.Model.Responses;

public class TwinValidationResultResponse
{
    public string? TwinId { get; set; }

    public IEnumerable<ValidationRuleResult> Results { get; set; } = Array.Empty<ValidationRuleResult>();
}

public class ValidationRuleResult
{
    public string? RuleId { get; set; }

    public IDictionary<string, IEnumerable<PropertyValidationResultType>> PropertyErrors { get; set; } = new Dictionary<string, IEnumerable<PropertyValidationResultType>>();
}
