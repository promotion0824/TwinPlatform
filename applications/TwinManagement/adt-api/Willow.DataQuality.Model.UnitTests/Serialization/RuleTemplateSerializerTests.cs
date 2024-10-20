namespace Willow.DataQuality.Model.UnitTests.Serialization
{
    using Willow.DataQuality.Model.Rules;
    using Willow.DataQuality.Model.Serialization;
    using Xunit;

    public class RuleTemplateSerializerTests
    {
        private readonly RuleTemplateSerializer ruleTemplateSerializer;

        public RuleTemplateSerializerTests()
        {
            ruleTemplateSerializer = new RuleTemplateSerializer();
        }

        [Fact(Skip = "TODO Fix: de-serialize unrecognized type discriminator")]
        public void Deserialize_WithIntRangeProperty_ShouldContainIntRangePropertyObject()
        {
            var ruleId = "test";
            var propertyName = "test-prop";
            var propertyMinValue = 2;
            var propertyMaxValue = 9;
            var value = $"{{\"id\":\"{ruleId}\",\"exactModelOnly\":false,\"properties\":[{{\"type\":\"intrange\",\"minValue\":{propertyMinValue},\"maxValue\":{propertyMaxValue},\"name\":\"{propertyName}\",\"required\":false}}]}}";
            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var property = rule?.Properties?.Single() as RuleTemplatePropertyNumericRange;

            Assert.NotNull(property);
            Assert.Equal(propertyName, property?.Name);
            Assert.Equal(propertyMinValue, property?.MinValue);
            Assert.Equal(propertyMaxValue, property?.MaxValue);
        }

        [Fact]
        public void Serialize_WithIntRangeProperty_ShouldContainIntRangeProperties()
        {
            var properties = new List<RuleTemplateProperty>
            {
                new RuleTemplatePropertyNumericRange { Name = "test-prop-range", MinValue = 2, MaxValue = 10 },
            };

            var rule = new RuleTemplate { Id = "test", Properties = properties };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains("test-prop-range", serialized);
            Assert.Contains("minValue", serialized);
            Assert.Contains("maxValue", serialized);
        }

        [Fact]
        public void Deserialize_WithPatternProperty_ShouldContainPatternPropertyObject()
        {
            var ruleId = "test";
            var propertyName = "test-prop";
            var propertyPattern = "(wewe)-[s232]";
            var value = $"{{\"id\":\"{ruleId}\",\"exactModelOnly\":false,\"properties\":[{{\"type\":\"pattern\",\"pattern\":\"{propertyPattern}\",\"name\":\"{propertyName}\",\"required\":false}}]}}";

            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var property = rule?.Properties?.Single() as RuleTemplatePropertyPattern;

            Assert.NotNull(property);
            Assert.Equal(propertyName, property?.Name);
            Assert.Equal(propertyPattern, property?.Pattern);
        }

        [Fact]
        public void Serialize_WithPatternProperty_ShouldContainPatternProperties()
        {
            var propertyName = "test-prop";
            var propertyPattern = "(wewe)-[s232]";
            var properties = new List<RuleTemplateProperty>
            {
                new RuleTemplatePropertyPattern { Name = propertyName, Pattern = propertyPattern },
            };

            var rule = new RuleTemplate { Id = "test", Properties = properties };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains(propertyName, serialized);
            Assert.Contains("pattern", serialized);
            Assert.Contains(propertyPattern, serialized);
        }

        [Fact(Skip = "TODO Fix: de-serialize unrecognized type discriminator")]
        public void Deserialize_WithNumericAllowedValuesProperty_ShouldContainIntAllowedValuesPropertyObject()
        {
            var ruleId = "test";
            var propertyName = "test-prop";
            var propertyUnit = "britishThermalUnitPerHour";
            var allowedValues = new List<double> { 123, 343, 677 };
            var value = $"{{\"id\":\"{ruleId}\",\"exactModelOnly\":false,\"properties\":[{{\"type\":\"intallowedvalues\",\"allowedValues\":[{string.Join(",", allowedValues.ToArray())}],\"unit\":\"{propertyUnit}\",\"name\":\"{propertyName}\",\"required\":false}}]}}";

            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var property = rule?.Properties?.Single() as RuleTemplatePropertyNumericAllowedValues;

            Assert.NotNull(property);
            Assert.Equal(propertyName, property?.Name);
            Assert.Equal(propertyUnit, property?.Unit);
            Assert.True(property?.AllowedValues.All(x => allowedValues.Contains(x)));
        }

        [Fact]
        public void Serialize_WithNumericAllowedValues_ShouldContainNumericAllowedValuesProperties()
        {
            var propertyName = "test-prop";
            var propertyUnit = "britishThermalUnitPerHour";
            var allowedValues = new List<double> { 123, 343, 677 };
            var properties = new List<RuleTemplateProperty>
            {
                new RuleTemplatePropertyNumericAllowedValues { Name = propertyName, Unit = propertyUnit, AllowedValues = allowedValues },
            };

            var rule = new RuleTemplate { Id = "test", Properties = properties };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains(propertyName, serialized);
            Assert.Contains("allowedValues", serialized);
            Assert.Contains(propertyUnit, serialized);
            Assert.True(allowedValues.All(x => serialized.Contains(x.ToString())));
        }

        [Fact]
        public void Deserialize_WithStringAllowedValuesProperty_ShouldContainStringAllowedValuesPropertyObject()
        {
            var ruleId = "test";
            var propertyName = "test-prop";
            var allowedValues = new List<string> { "123", "343", "677" };
            var value = $"{{\"id\":\"{ruleId}\",\"exactModelOnly\":false,\"properties\":[{{\"type\":\"stringallowedvalues\",\"allowedValues\":[{string.Join(",", allowedValues.Select(x => $"\"{x}\"").ToArray())}],\"name\":\"{propertyName}\",\"required\":false}}]}}";

            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var property = rule?.Properties?.Single() as RuleTemplatePropertyStringAllowedValues;

            Assert.NotNull(property);
            Assert.Equal(propertyName, property?.Name);
            Assert.True(property?.AllowedValues.All(x => allowedValues.Contains(x)));
        }

        [Fact]
        public void Serialize_WithStringAllowedValues_ShouldContainStringAllowedValuesProperties()
        {
            var propertyName = "test-prop";
            var allowedValues = new List<string> { "123", "343", "677" };
            var properties = new List<RuleTemplateProperty>
            {
                new RuleTemplatePropertyStringAllowedValues { Name = propertyName, AllowedValues = allowedValues },
            };

            var rule = new RuleTemplate { Id = "test", Properties = properties };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains(propertyName, serialized);
            Assert.Contains("allowedValues", serialized);
            Assert.True(allowedValues.All(x => serialized.Contains(x)));
        }

        [Fact]
        public void Deserialize_WithPath_ShouldContainPathObject()
        {
            var ruleId = "test";
            var pathName = "test-path";
            var pathMatch = "([this])-[:hasDocument]->([dtmi:com:willowinc:Warranty])";
            var value = $"{{\"id\":\"test\",\"exactModelOnly\":false,\"paths\":[{{\"name\":\"{pathName}\",\"match\":\"{pathMatch}\"}}]}}";

            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var path = rule?.Paths?.Single();

            Assert.NotNull(path);
            Assert.Equal(pathName, path?.Name);
            Assert.Equal(pathMatch, path?.Match);
        }

        [Fact]
        public void Serialize_WithPath_ShouldContainPathProperties()
        {
            var pathName = "test-path";
            var pathMatch = "([this])-[:hasDocument]-([dtmi:com:willowinc:Warranty])";
            var paths = new List<RuleTemplatePath>
            {
                new RuleTemplatePath { Name = pathName, Match = pathMatch },
            };

            var rule = new RuleTemplate { Id = "test", Paths = paths };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains(pathName, serialized);
            Assert.Contains(pathMatch, serialized);
        }

        [Fact]
        public void Serialize_WithExpression_ShouldContainExpressionProperties()
        {
            var expressionName = "test-expression";
            var expression = "EXISTS([nominalHeatingCapacity]) OR EXISTS([netSensibleHeatingCapacity])";
            var expressions = new List<RuleTemplateExpression>
            {
                new RuleTemplateExpression { Name = expressionName, Expression = expression },
            };

            var rule = new RuleTemplate { Id = "test", Expressions = expressions };

            var serialized = ruleTemplateSerializer.Serialize(rule);

            Assert.Contains(expressionName, serialized);
            Assert.Contains(expression, serialized);
        }

        [Fact]
        public void Deserialize_WithExpression_ShouldContainExpressionObject()
        {
            var ruleId = "test";
            var expressionName = "test-expression";
            var expression = "EXISTS([nominalHeatingCapacity]) OR EXISTS([netSensibleHeatingCapacity])";
            var value = $"{{\"id\":\"{ruleId}\",\"exactModelOnly\":false,\"expressions\":[{{\"name\":\"{expressionName}\",\"expression\":\"{expression}\"}}]}}";

            var rule = ruleTemplateSerializer.Deserialize(value);

            Assert.NotNull(rule);
            Assert.Equal(ruleId, rule?.Id);

            var expressionObject = rule?.Expressions?.Single();

            Assert.NotNull(expressionObject);
            Assert.Equal(expressionName, expressionObject?.Name);
            Assert.Equal(expression, expressionObject?.Expression);
        }
    }
}
