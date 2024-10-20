namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISchemaColumnsRepository
    {
        Task<SchemaColumnEntity> GetItemAsync(Guid itemKey);

        Task<SchemaColumnEntity> CreateAsync(SchemaColumnEntity newItem);

        Task<IList<SchemaColumnEntity>> GetBySchema(Guid schemaId);

        Task<IList<SchemaColumnEntity>> GetBySchemas(params Guid[] schemaIds);
    }
}
