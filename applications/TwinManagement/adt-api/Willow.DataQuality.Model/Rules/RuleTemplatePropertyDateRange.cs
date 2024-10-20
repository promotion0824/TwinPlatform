namespace Willow.DataQuality.Model.Rules;

public class RuleTemplatePropertyDateRange : RuleTemplateProperty
{
    public DateTime? MinValue { get; set; }
    public DateTime? MaxValue { get; set; }
}
