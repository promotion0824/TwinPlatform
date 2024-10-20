using Azure.DigitalTwins.Core;
using System.Text.RegularExpressions;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.DataQuality.Execution.Converters;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;

namespace Willow.DataQuality.Execution.Checkers;

public class PropertiesChecker : IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>
{

    public Task<IEnumerable<PropertyValidationResult>> Check(TwinWithRelationships twinWithRelationships, IEnumerable<RuleTemplateProperty> propertyRules, IEnumerable<UnitInfo>? unitInfo = null)
    {
        var validationResults = new List<PropertyValidationResult>();
        var twin = twinWithRelationships.Twin;

        foreach (var propertyRule in propertyRules)
        {
            validationResults.Add(CheckBaseRuleTemplateProperty(twin, propertyRule));

            PropertyValidationResult? result = null;

            // TODO: Why are we doing a manual dispatch?
            switch (propertyRule)
            {
                case RuleTemplatePropertyNumericRange rule:
                    result = CheckNumericRangeProperty(twin, rule, unitInfo);
                    break;
                case RuleTemplatePropertyDateRange rule:
                    result = CheckDateRangeProperty(twin, rule);
                    break;
                case RuleTemplatePropertyPattern rule:
                    result = CheckPatternProperty(twin, rule);
                    break;
                case RuleTemplatePropertyStringAllowedValues rule:
                    result = CheckStringAllowedValuesProperty(twin, rule);
                    break;
                case RuleTemplatePropertyNumericAllowedValues rule:
                    result = CheckNumericListProperty(twin, rule, unitInfo);
                    break;
            }

            if (result != null)
                validationResults.Add(result);
        }

        // TODO: why are we adding null results at all?
        return Task.FromResult(validationResults.Where(x => x != null));
    }

    private static PropertyValidationResult? CheckBaseRuleTemplateProperty(BasicDigitalTwin twin, RuleTemplateProperty ruleTemplateProperty)
    {
        if (ruleTemplateProperty.Required && !twin.Contents.ContainsKey(ruleTemplateProperty.Name))
            return new PropertyValidationResult(PropertyValidationResultType.RequiredPropertyMissing, ruleTemplateProperty.Name);
        return null;
    }

    private static PropertyValidationResult? CheckNumericRangeProperty(BasicDigitalTwin twin, RuleTemplatePropertyNumericRange ruleTemplateProperty, IEnumerable<UnitInfo>? unitInfo)
    {
        if (!twin.Contents.ContainsKey(ruleTemplateProperty.Name))
            return null;

        var converted = false;
        var twinPropertyValue = twin.Contents[ruleTemplateProperty.Name].ToString();
        var twinPropertyValueParsedSuccess = double.TryParse(twinPropertyValue, out double twinPropertyDoubleValue);
        double convertedValue = 0;
        var unitPropertyFromUnitInfo = unitInfo?.FirstOrDefault(x => x.AnnotatedProperty == ruleTemplateProperty.Name)?.UnitProperty;
        if (unitPropertyFromUnitInfo == null)
        {
            // Models UnitInfo cache missing annotated Unit property
            return new UnitsValidationResult(PropertyValidationResultType.ModelMissingAnnotatedUnitProperty, ruleTemplateProperty.Name, null);
        }
        //Get Twins Unit
        var unitPropertyValueFromTwin = twin.Contents.ContainsKey(unitPropertyFromUnitInfo) ? twin.Contents[unitPropertyFromUnitInfo] : null;
        if (unitPropertyValueFromTwin == null)
        {
            return new UnitsValidationResult(PropertyValidationResultType.RequiredUnitMissing, unitPropertyFromUnitInfo, ruleTemplateProperty.Unit);
        }
        if (unitPropertyValueFromTwin.ToString() != ruleTemplateProperty.Unit)
        {
            converted = UnitConverter.ConvertToUnit(unitPropertyValueFromTwin.ToString(), ruleTemplateProperty.Unit, twinPropertyDoubleValue, out convertedValue);
            if (!converted)
            {
                return new UnitsConversionValidationResult(PropertyValidationResultType.UnitConversionError,
                    ruleTemplateProperty.Name,
                    ruleTemplateProperty.Unit, unitPropertyValueFromTwin.ToString());
            }
        }

        bool ValidateRange(double value)
        {
            if (ruleTemplateProperty.MinValue.HasValue && value <= ruleTemplateProperty.MinValue.Value ||
                ruleTemplateProperty.MaxValue.HasValue && value >= ruleTemplateProperty.MaxValue.Value)
            {
                return false;
            }
            return true;
        }

        if (converted)
        {
            if (twinPropertyValueParsedSuccess && !ValidateRange(convertedValue))
            {
                return new NumericRangeValidationResultConvertedUnits(PropertyValidationResultType.InvalidValueAfterUnitConversion, ruleTemplateProperty.Name,
                    unitPropertyValueFromTwin.ToString(), ruleTemplateProperty.Unit, ruleTemplateProperty.MinValue, ruleTemplateProperty.MaxValue, twinPropertyDoubleValue, convertedValue,
                    twinPropertyDoubleValue.ToString());
            }
        }
        else if (!twinPropertyValueParsedSuccess || !ValidateRange(twinPropertyDoubleValue))
        {
            return new NumericRangeValidationResult(PropertyValidationResultType.InvalidValue, ruleTemplateProperty.Name,
                ruleTemplateProperty.Unit, ruleTemplateProperty.MinValue, ruleTemplateProperty.MaxValue, twinPropertyDoubleValue.ToString());
        }
        return null;
    }

    private static PropertyValidationResult? CheckNumericListProperty(BasicDigitalTwin twin, RuleTemplatePropertyNumericAllowedValues ruleTemplateProperty, IEnumerable<UnitInfo>? unitInfo)
    {
        if (!twin.Contents.ContainsKey(ruleTemplateProperty.Name))
            return null;

        var twinPropertyValue = twin.Contents[ruleTemplateProperty.Name].ToString();
        var twinPropertyValueParsedSuccess = double.TryParse(twinPropertyValue, out double twinPropertyDoubleValue);
        var unitPropertyFromUnitInfo = unitInfo?.FirstOrDefault(x => x.AnnotatedProperty == ruleTemplateProperty.Name).UnitProperty;
        if (unitPropertyFromUnitInfo == null)
        {
            // Models UnitInfo cache missing annotated Unit property
            return new UnitsValidationResult(PropertyValidationResultType.ModelMissingAnnotatedUnitProperty, ruleTemplateProperty.Name, null);
        }
        //Get Twins Unit
        var unitPropertyValueFromTwin = twin.Contents.ContainsKey(unitPropertyFromUnitInfo) ? twin.Contents[unitPropertyFromUnitInfo] : null;
        if (unitPropertyValueFromTwin == null)
        {
            return new UnitsValidationResult(PropertyValidationResultType.RequiredUnitMissing, unitPropertyFromUnitInfo, ruleTemplateProperty.Unit);
        }
        if (unitPropertyValueFromTwin.ToString() != ruleTemplateProperty.Unit)
        {
            var converted = UnitConverter.ConvertToUnit(unitPropertyValueFromTwin.ToString(), ruleTemplateProperty.Unit, twinPropertyDoubleValue, out var convertedValue);
            if (!converted)
            {
                return new UnitsConversionValidationResult(PropertyValidationResultType.UnitConversionError,
                    ruleTemplateProperty.Name,
                    ruleTemplateProperty.Unit, unitPropertyValueFromTwin.ToString());
            }
            else if (!ruleTemplateProperty.AllowedValues.Contains(convertedValue))
            {
                return new NumericListValidationResult(PropertyValidationResultType.InvalidValueAfterUnitConversion, ruleTemplateProperty.Name,
                    ruleTemplateProperty.Unit, string.Join(",", ruleTemplateProperty.AllowedValues),
                    string.Format($"{convertedValue} {ruleTemplateProperty.Unit} converted from {twinPropertyDoubleValue.ToString()} {unitPropertyValueFromTwin}"));
            }
        }
        else if (!twinPropertyValueParsedSuccess || !ruleTemplateProperty.AllowedValues.Contains(twinPropertyDoubleValue))
        {
            return new NumericListValidationResult(PropertyValidationResultType.InvalidValue, ruleTemplateProperty.Name,
                ruleTemplateProperty.Unit, string.Join(",", ruleTemplateProperty.AllowedValues), twinPropertyDoubleValue.ToString());
        }
        return null;
    }

    private static PropertyValidationResult? CheckDateRangeProperty(BasicDigitalTwin twin, RuleTemplatePropertyDateRange ruleTemplateProperty)
    {
        if (twin.Contents.ContainsKey(ruleTemplateProperty.Name))
        {
            var twinPropertyValue = twin.Contents[ruleTemplateProperty.Name].ToString();
            var isInvalidDateRangeValue = !DateTime.TryParse(twinPropertyValue, out DateTime twinPropertyDateValue) ||
                ruleTemplateProperty.MinValue.HasValue && twinPropertyDateValue.Date <= ruleTemplateProperty.MinValue.Value.Date ||
                ruleTemplateProperty.MaxValue.HasValue && twinPropertyDateValue.Date >= ruleTemplateProperty.MaxValue.Value.Date;
            if (isInvalidDateRangeValue)
            {
                return new DateRangeValidationResult(PropertyValidationResultType.InvalidValue, ruleTemplateProperty.Name,
                    ruleTemplateProperty.MinValue.Value.Date, ruleTemplateProperty.MaxValue.Value.Date, twinPropertyDateValue.ToString());
            }
        }
        return null;
    }

    private static PropertyValidationResult? CheckPatternProperty(BasicDigitalTwin twin, RuleTemplatePropertyPattern ruleTemplateProperty)
    {
        if (twin.Contents.ContainsKey(ruleTemplateProperty.Name))
        {
            var twinPropertyValue = twin.Contents[ruleTemplateProperty.Name].ToString();
            var match = Regex.Match(twinPropertyValue, ruleTemplateProperty.Pattern, RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return new PatternValidationResult(PropertyValidationResultType.InvalidFormat, ruleTemplateProperty.Name, ruleTemplateProperty.Pattern, twinPropertyValue);
            }
        }
        return null;
    }

    private static PropertyValidationResult? CheckStringAllowedValuesProperty(BasicDigitalTwin twin, RuleTemplatePropertyStringAllowedValues ruleTemplateProperty)
    {
        if (twin.Contents.ContainsKey(ruleTemplateProperty.Name))
        {
            var twinPropertyValue = twin.Contents[ruleTemplateProperty.Name].ToString();
            if (!ruleTemplateProperty.AllowedValues.Contains(twinPropertyValue))
            {
                return new StringEnumValidationResult(PropertyValidationResultType.InvalidValue, ruleTemplateProperty.Name, ruleTemplateProperty.AllowedValues, twinPropertyValue);
            }
        }
        return null;
    }
}
