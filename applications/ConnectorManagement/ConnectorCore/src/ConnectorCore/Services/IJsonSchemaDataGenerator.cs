namespace ConnectorCore.Services;

using ConnectorCore.Entities;

internal interface IJsonSchemaDataGenerator
{
    string GenerateEmptyObject(ICollection<SchemaColumnEntity> columns);
}
