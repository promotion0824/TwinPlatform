namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISchemaColumnsService
    {
        Task<IList<SchemaColumnEntity>> GetBySchema(Guid schemaId);

        Task<SchemaColumnEntity> CreateAsync(SchemaColumnEntity newItem);

        Task<SchemaColumnEntity> GetItemAsync(Guid itemKey);
    }
}
