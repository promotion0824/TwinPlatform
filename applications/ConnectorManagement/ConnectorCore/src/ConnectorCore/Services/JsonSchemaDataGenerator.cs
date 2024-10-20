namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ConnectorCore.Entities;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal class JsonSchemaDataGenerator : IJsonSchemaDataGenerator
    {
        public string GenerateEmptyObject(ICollection<SchemaColumnEntity> columns)
        {
            var jObject = new JObject();
            foreach (var schemaColumnEntity in columns.Where(c => c.IsRequired))
            {
                var typeName = schemaColumnEntity.DataType;
                if (typeName.Equals("number", StringComparison.InvariantCultureIgnoreCase))
                {
                    jObject[schemaColumnEntity.Name] = 0;
                }

                if (typeName.Equals("boolean", StringComparison.InvariantCultureIgnoreCase))
                {
                    jObject[schemaColumnEntity.Name] = false;
                }

                if (typeName.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                {
                    jObject[schemaColumnEntity.Name] = string.Empty;
                }
            }

            return JsonConvert.SerializeObject(jObject);
        }
    }
}
