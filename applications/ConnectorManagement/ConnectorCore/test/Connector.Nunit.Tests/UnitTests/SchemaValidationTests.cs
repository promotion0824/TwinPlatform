namespace Connector.Nunit.Tests.UnitTests
{
    using System.Collections.Generic;
    using ConnectorCore.Entities;
    using ConnectorCore.Entities.Validators;
    using FluentAssertions;
    using NUnit.Framework;

    public class SchemaValidationTests
    {
        [Test]
        public void ValidateJson_ReturnsTrue()
        {
            var json = @"{
                      ""Name"": ""Foo"",
                      ""Value"": 1,
                      ""Bool"": false
                    }";
            var columns = new List<SchemaColumnEntity>
            {
                new SchemaColumnEntity { Name = "Name", DataType = "string" },
                new SchemaColumnEntity { Name = "Value", DataType = "number" },
                new SchemaColumnEntity { Name = "Bool", DataType = "boolean" },
            };

            var validator = new JsonSchemaValidator();
            var result = validator.IsValid(columns, json, out _);

            result.Should().BeTrue();
        }

        [Test]
        public void ValidateJson_WrongDataType_ReturnsFalse()
        {
            var json = @"{
                      ""Name"": ""Foo"",
                      ""Value"": ""2""
                    }";
            var columns = new List<SchemaColumnEntity>
            {
                new SchemaColumnEntity { Name = "Name", DataType = "string" },
                new SchemaColumnEntity { Name = "Value", DataType = "number" },
            };

            var validator = new JsonSchemaValidator();
            var result = validator.IsValid(columns, json, out _);

            result.Should().BeTrue();
        }

        [Test]
        public void ValidateJson_RequiredFieldAbsent_ReturnsFalse()
        {
            var json = @"{
                      ""Name"": ""Foo""
                    }";
            var columns = new List<SchemaColumnEntity>
            {
                new SchemaColumnEntity { Name = "Name", DataType = "string" },
                new SchemaColumnEntity { Name = "Value", DataType = "number", IsRequired = true },
            };

            var validator = new JsonSchemaValidator();
            var result = validator.IsValid(columns, json, out _);

            result.Should().BeFalse();
        }

        [Test]
        public void ValidateJson_OptionalFieldsNull_ReturnsTrue()
        {
            var json = @"{
                      ""Name"": null,
                      ""Value"": null,
                      ""Bool"": null
                    }";
            var columns = new List<SchemaColumnEntity>
            {
                new SchemaColumnEntity { Name = "Name", DataType = "string" },
                new SchemaColumnEntity { Name = "Value", DataType = "number" },
                new SchemaColumnEntity { Name = "Bool", DataType = "boolean" },
            };

            var validator = new JsonSchemaValidator();
            var result = validator.IsValid(columns, json, out _);

            result.Should().BeTrue();
        }

        [Test]
        public void ValidateJson_RequiredFieldNull_ReturnsFalse()
        {
            var json = @"{
                      ""Name"": ""Foo"",
                      ""Value"": null,
                      ""Bool"": true
                    }";
            var columns = new List<SchemaColumnEntity>
            {
                new SchemaColumnEntity { Name = "Name", DataType = "string" },
                new SchemaColumnEntity { Name = "Value", DataType = "number", IsRequired = true },
                new SchemaColumnEntity { Name = "Bool", DataType = "boolean" },
            };

            var validator = new JsonSchemaValidator();
            var result = validator.IsValid(columns, json, out _);

            result.Should().BeFalse();
        }
    }
}
