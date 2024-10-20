using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace RulesEngine.Web;

/// <summary>
/// Rule extensions
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates a parameter
    /// </summary>
    public static IList<ValidationReponseElementDto> ValidateRuleParameter(this RuleParameterDto source, string field, bool unitsRequired = false)
    {
        if (source is null)
        {
            List<ValidationReponseElementDto> validations = new()
            {
                new ValidationReponseElementDto(field, "An expression is required")
            };

            return validations;
        }

        return new RuleParameterDto[]
        {
            source
        }.ValidateRuleParameters(true, field, unitsRequired);
    }

    /// <summary>
    /// Validates parameters
    /// </summary>
    public static IList<ValidationReponseElementDto> ValidateRuleParameters(this IEnumerable<RuleParameterDto> source, bool required, string field, bool unitsRequired = false, bool requiresResultField = false)
    {
        List<ValidationReponseElementDto> validations = new();

        if (source.Count() == 0 && required)
        {
            validations.Add(new ValidationReponseElementDto(field, "At least one expression is required"));
        }

        if (requiresResultField && !source.Any(v => v.FieldId == Fields.Result.Id))
        {
            validations.Add(new ValidationReponseElementDto(field, "The 'result' expression is missing. Please add a 'result' field for the rule"));
        }

        foreach (var parameter in source)
        {
            try
            {
                if (source.Any(v => v.FieldId == parameter.FieldId && v != parameter))
                {
                    validations.Add(new ValidationReponseElementDto(parameter.FieldId, $"Field Id '{parameter.FieldId}' must be unique"));
                }

                if (unitsRequired && string.IsNullOrWhiteSpace(parameter.Units))
                {
                    validations.Add(new ValidationReponseElementDto($"{parameter.FieldId}_units", $"A unit is required for {parameter.FieldId}"));
                }

                if (string.IsNullOrWhiteSpace(parameter.PointExpression))
                {
                    validations.Add(new ValidationReponseElementDto(parameter.FieldId, "An expression is required"));
                }
                else
                {
                    Willow.ExpressionParser.Parser.Deserialize(parameter.PointExpression);
                }
            }
            catch (Willow.ExpressionParser.ParserException pex)
            {
                validations.Add(new ValidationReponseElementDto(parameter.FieldId, pex.Message));
            }
        }

        return validations.WithParentField(field);
    }

    /// <summary>
    /// Appends a field prefix to validations in a list
    /// </summary>
    public static IList<ValidationReponseElementDto> AppendPrefix(this IEnumerable<ValidationReponseElementDto> validations, string prefix)
    {
        return validations.Select(v => new ValidationReponseElementDto($"{prefix}_{v.field}", v.message)).ToList();
    }

    /// <summary>
    /// Appends a parent field to validations in a list
    /// </summary>
    public static IList<ValidationReponseElementDto> WithParentField(this IEnumerable<ValidationReponseElementDto> validations, string parentField)
    {
        return validations.Select(v => new ValidationReponseElementDto(v.field, v.message, parentField: parentField)).ToList();
    }

    /// <summary>
    /// Validates rule triggers
    /// </summary>
    public static IList<ValidationReponseElementDto> ValidateRuleTriggers(this IEnumerable<RuleTriggerDto> source, bool required, string field)
    {
        List<ValidationReponseElementDto> validations = new();

        if (source.Count() == 0 && required)
        {
            validations.Add(new ValidationReponseElementDto(field, "At least one expression is required"));
        }

        foreach (var trigger in source)
        {
            if (string.IsNullOrWhiteSpace(trigger.Name))
            {
                validations.Add(new ValidationReponseElementDto(trigger.Name, "Name is required"));
                continue;
            }

            if (source.Any(v => string.Equals(v.Name, trigger.Name, System.StringComparison.OrdinalIgnoreCase) && v != trigger))
            {
                validations.Add(new ValidationReponseElementDto(trigger.Name, $"Trigger Name '{trigger.Name}' must be unique"));
            }

            var expressionValidations = new List<ValidationReponseElementDto>();

            expressionValidations.AddRange(trigger.Condition.ValidateRuleParameter(nameof(RuleTriggerDto.Condition)));

            if (trigger.TriggerType == RuleTriggerType.TriggerCommand)
            {
                expressionValidations.AddRange(trigger.Value.ValidateRuleParameter(nameof(RuleTriggerDto.Value), unitsRequired: true));
                expressionValidations.AddRange(trigger.Point.ValidateRuleParameter(nameof(RuleTriggerDto.Point)));
            }

            validations.AddRange(expressionValidations.AppendPrefix(trigger.Name).WithParentField(field));
        }

        return validations;
    }
}
