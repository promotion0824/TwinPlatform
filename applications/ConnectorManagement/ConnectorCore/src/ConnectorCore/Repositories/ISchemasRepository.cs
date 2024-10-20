namespace ConnectorCore.Repositories
{
    using System;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISchemasRepository
    {
        Task<SchemaEntity> CreateAsync(SchemaEntity newItem);

        Task<bool> IsSchemaInUseAsync(Guid schemaId);
    }
}
