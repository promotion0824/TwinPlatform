using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;

namespace Willow.DataQuality.Execution.Checkers;

public interface IRuleTemplateChecker
{
    Task<RuleTemplateValidationResult> Check(TwinWithRelationships twin, RuleTemplate ruleTemplate, List<UnitInfo>? unitInfo = null);
}

public class RuleTemplateChecker : IRuleTemplateChecker
{
    private readonly IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult> _propertiesChecker;
    private readonly IRuleBodyChecker<RuleTemplatePath, PathValidationResult> _pathsChecker;
    private readonly IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult> _expressionsChecker;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;

    public RuleTemplateChecker(IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult> propertiesChecker,
        IRuleBodyChecker<RuleTemplatePath, PathValidationResult> pathsChecker,
        IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult> expressionsChecker,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser)
    {
        _propertiesChecker = propertiesChecker;
        _pathsChecker = pathsChecker;
        _expressionsChecker = expressionsChecker;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
    }

    public async Task<RuleTemplateValidationResult> Check(TwinWithRelationships twinWithRelationship, RuleTemplate ruleTemplate, List<UnitInfo>? unitInfo = null)
    {
        if (twinWithRelationship == null)
            throw new NullReferenceException("Rule template checker - Null Twin");

        if (ruleTemplate is null ||
                        false == ruleTemplate.Properties?.Any() &&
                        false == ruleTemplate.Expressions?.Any() &&
                        false == ruleTemplate.Paths?.Any())
        {
            throw new InvalidDataException($"Rule template checker - Rule has nothing to check - ruleId: {ruleTemplate?.Id ?? "???"}");
        }

        var result = new RuleTemplateValidationResult(twinWithRelationship, ruleTemplate);

        if (ruleTemplate.ExactModelOnly && twinWithRelationship.Twin.Metadata.ModelId != ruleTemplate.PrimaryModelId)
        {
            result.IsApplicableModel = false;
            return result;
        }

        if (!ruleTemplate.ExactModelOnly && ruleTemplate.PrimaryModelId != twinWithRelationship.Twin.Metadata.ModelId)
        {
            // TODO: We should cache descendant info, and this interface should be on our model cache
            var descendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { ruleTemplate.PrimaryModelId });
            var foundDescendant = descendants.Any(x => x.Key == twinWithRelationship.Twin.Metadata.ModelId);
            if (!foundDescendant)
            {
                result.IsApplicableModel = false;
                return result;
            }
        }

        result.IsApplicableModel = true;

        // TODO: re-factor to return results rather than assign as side-effect
        // Property and expression checker are simple operations w/no I/O - execute 

        var propertiesCheck = async () => result.PropertyValidationResults =
                                await _propertiesChecker.Check(twinWithRelationship, ruleTemplate.Properties, unitInfo);

        var pathCheck = async () => result.PathValidationResults =
                                await _pathsChecker.Check(twinWithRelationship, ruleTemplate.Paths);

        var expressionCheck = async () => result.ExpressionValidationResults =
                                await _expressionsChecker.Check(twinWithRelationship, ruleTemplate.Expressions);

        await Task.WhenAll(propertiesCheck(), pathCheck(), expressionCheck());

        result.IsValid =
            !result.PropertyValidationResults.Any() &&
            !result.PathValidationResults.Any() &&
            !result.ExpressionValidationResults.Any();

        return result;
    }
}
