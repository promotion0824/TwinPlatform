namespace Willow.DataQuality.Model.Validation;

public enum PropertyValidationResultType
{
    RequiredPropertyMissing,
    InvalidFormat,
    InvalidValue,
    InvalidRange,
    RequiredUnitMissing,
    InvalidValueAfterUnitConversion,
    UnitConversionError,
    ModelMissingAnnotatedUnitProperty
}

public record PropertyValidationResult(PropertyValidationResultType type, string propertyName, string? actualValue = null, string? expectedValue = null);

public record NumericRangeValidationResult(PropertyValidationResultType type, string propertyName, string? unit, double? min, double? max, string actual)
    : PropertyValidationResult(type, propertyName, actual, new string(string.Format($" min : {min}, max : {max} {unit}")));

public record NumericRangeValidationResultConvertedUnits(PropertyValidationResultType type, string propertyName,
    string? twinUnit, string? convertedRuleUnit, double? min, double? max, double? originalValue, double? convertedValue, string actual)
: PropertyValidationResult(type, propertyName, new string(string.Format($"{actual} {twinUnit}")), new string(string.Format($" {originalValue} {twinUnit} converted to {convertedValue} {convertedRuleUnit} min/max : {min}, {max} {convertedRuleUnit}")));

public record UnitsConversionValidationResult(PropertyValidationResultType type, string propertyName, string? expectedUnit, string actual)
: PropertyValidationResult(type, propertyName, actual, new string(string.Format($"{expectedUnit}")));

public record NumericListValidationResult(PropertyValidationResultType type, string propertyName, string? unit, string allowedValues, string? actual)
: PropertyValidationResult(type, propertyName, actual, new string(string.Format($"unit: {unit}, allowedValues: {allowedValues}")));

public record DateRangeValidationResult(PropertyValidationResultType type, string propertyName, DateTime min, DateTime max, string actual)
    : PropertyValidationResult(type, propertyName, actual, new string(string.Format($"Min: {min}, Max: {max}")));

public record StringEnumValidationResult(PropertyValidationResultType type, string propertyName, IEnumerable<string> allowedValues, string actual)
    : PropertyValidationResult(type, propertyName, actual, new string(string.Format($"{allowedValues}")));

public record NumericEnumValidationResult(PropertyValidationResultType type, string propertyName, IEnumerable<int> allowedValues, string actual)
    : PropertyValidationResult(type, propertyName, actual, new string(string.Format($"{allowedValues}")));

public record PatternValidationResult(PropertyValidationResultType type, string propertyName, string pattern, string actual)
    : PropertyValidationResult(type, propertyName, actual, new string(string.Format($"{pattern}")));

public record UnitsValidationResult(PropertyValidationResultType type, string propertyName, string? unit)
: PropertyValidationResult(type, propertyName, expectedValue: unit);
