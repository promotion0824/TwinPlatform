using Willow.DataQuality.Model.Rules;
using Willow.Model.Adt;

namespace Willow.DataQuality.Model.Validation;

public class RuleTemplateValidationResult
{
    public RuleTemplateValidationResult(TwinWithRelationships twinWithRelationship, RuleTemplate ruleTemplate)
    {
        TwinWithRelationship = twinWithRelationship;
        RuleTemplate = ruleTemplate;
        PropertyValidationResults = new List<PropertyValidationResult>();
        ExpressionValidationResults = new List<ExpressionValidationResult>();
        PathValidationResults = new List<PathValidationResult>();
    }

    public TwinWithRelationships TwinWithRelationship { get; init; }
    public RuleTemplate RuleTemplate { get; init; }
    public IEnumerable<PropertyValidationResult> PropertyValidationResults { get; set; }
    public IEnumerable<PathValidationResult> PathValidationResults { get; set; }
    public IEnumerable<ExpressionValidationResult> ExpressionValidationResults { get; set; }
    public bool IsApplicableModel { get; set; }
    public bool IsValid { get; set; }
}
