namespace Willow.DataQuality.Model.Rules;

public class RuleTemplate
{
    public string? Id { get; set; }

    public bool ExactModelOnly { get; set; }

    public string? TemplateId { get; set; }

    public string? PrimaryModelId { get; set; }

    public IDictionary<string, string> Name { get; set; } = new Dictionary<string, string>();

    public IDictionary<string, string> Description { get; set; } = new Dictionary<string, string>();

    public IEnumerable<RuleTemplateProperty>? Properties { get; set; }

    public IEnumerable<RuleTemplateExpression>? Expressions { get; set; }

    public IEnumerable<RuleTemplatePath>? Paths { get; set; }
}
