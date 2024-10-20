using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;

namespace Willow.DataQuality.Execution.Checkers;

public class ExpressionsChecker : IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>
{
    public Task<IEnumerable<ExpressionValidationResult>> Check(TwinWithRelationships twinWithRelationships, IEnumerable<RuleTemplateExpression> expressionRules, IEnumerable<UnitInfo>? unitInfo = null)
    {
        return Task.FromResult(Enumerable.Empty<ExpressionValidationResult>());
    }
}
