namespace ConnectorCore.Entities.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    internal class JsonSchemaValidator : IJsonSchemaValidator
    {
        public bool IsValid(ICollection<SchemaColumnEntity> columns, string jsonValue, out IList<string> errorMessages)
        {
            errorMessages = new List<string>();

            if (!columns.Any())
            {
                return true;
            }

            JObject jsonObject;
            try
            {
                jsonObject = JObject.Parse(jsonValue);
            }
            catch
            {
                errorMessages.Add("String provided is not a valid JSON");
                return false;
            }

            var numberColumns = columns.Where(x => "Number".Equals(x.DataType, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => x.Name).ToHashSet();

            var booleanColumns = columns.Where(x => x.DataType.Equals("Boolean", StringComparison.InvariantCultureIgnoreCase))
                                        .Select(x => x.Name)
                                        .ToHashSet();

            foreach (var prop in jsonObject.Properties())
            {
                if (numberColumns.Contains(prop.Name) && decimal.TryParse(prop.Value.ToString(), out var numberValue))
                {
                    prop.Value = numberValue;
                }
                else if (booleanColumns.Contains(prop.Name) && bool.TryParse(prop.Value.ToString(), out var booleanValue))
                {
                    prop.Value = booleanValue;
                }
            }

            var jSchema = new JSchema { Type = JSchemaType.Object };
            foreach (var schemaColumnEntity in columns)
            {
                JSchema prop;
                var schemaType = GetSchemaType(schemaColumnEntity.DataType);

                if (schemaColumnEntity.IsRequired)
                {
                    prop = new JSchema { Type = schemaType };
                    jSchema.Required.Add(schemaColumnEntity.Name);
                }
                else
                {
                    // Allows non-required properties to be null.
                    prop = JSchema.Parse($"{{\"type\": [ \"{schemaType.ToString().ToLower()}\", \"null\" ]}}");
                }

                jSchema.Properties[schemaColumnEntity.Name] = prop;
            }

            return jsonObject.IsValid(jSchema, out errorMessages);
        }

        private JSchemaType GetSchemaType(string typeName)
        {
            if (typeName.Equals("number", StringComparison.InvariantCultureIgnoreCase))
            {
                return JSchemaType.Number;
            }

            if (typeName.Equals("boolean", StringComparison.InvariantCultureIgnoreCase))
            {
                return JSchemaType.Boolean;
            }

            if (typeName.Equals("string", StringComparison.InvariantCultureIgnoreCase))
            {
                return JSchemaType.String;
            }

            throw new ArgumentException($"Data type [{typeName}] is not supported");
        }
    }
}
