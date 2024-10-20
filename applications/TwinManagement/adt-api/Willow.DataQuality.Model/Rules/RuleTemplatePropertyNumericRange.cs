namespace Willow.DataQuality.Model.Rules;

public class RuleTemplatePropertyNumericRange : RuleTemplateProperty
{
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string? Unit { get; set; }
}
