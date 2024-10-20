using System.Text.Json.Serialization;

namespace Willow.DataQuality.Model.Rules;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(RuleTemplatePropertyNumericRange), typeDiscriminator: "numericrange")]
[JsonDerivedType(typeof(RuleTemplatePropertyDateRange), typeDiscriminator: "daterange")]
[JsonDerivedType(typeof(RuleTemplatePropertyPattern), typeDiscriminator: "pattern")]
[JsonDerivedType(typeof(RuleTemplatePropertyNumericAllowedValues), typeDiscriminator: "numericallowedvalues")]
[JsonDerivedType(typeof(RuleTemplatePropertyStringAllowedValues), typeDiscriminator: "stringallowedvalues")]
public class RuleTemplateProperty
{
    public string? Name { get; set; }

    public bool Required { get; set; }
}
