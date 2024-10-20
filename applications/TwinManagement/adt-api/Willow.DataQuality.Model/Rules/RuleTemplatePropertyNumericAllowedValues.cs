namespace Willow.DataQuality.Model.Rules;

public class RuleTemplatePropertyNumericAllowedValues : RuleTemplateProperty
{
    public IEnumerable<double> AllowedValues { get; set; } = Array.Empty<double>();

    public string? Unit { get; set; }
}
