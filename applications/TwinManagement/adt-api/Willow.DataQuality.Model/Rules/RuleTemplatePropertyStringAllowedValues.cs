namespace Willow.DataQuality.Model.Rules;

public class RuleTemplatePropertyStringAllowedValues : RuleTemplateProperty
{
    public IEnumerable<string> AllowedValues { get; set; } = Array.Empty<string>();
}
