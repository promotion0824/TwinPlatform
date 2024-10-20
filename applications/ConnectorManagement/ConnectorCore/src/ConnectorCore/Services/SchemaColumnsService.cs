namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Repositories;

    internal class SchemaColumnsService : ISchemaColumnsService
    {
        private readonly ISchemaColumnsRepository schemaColumnsRepository;
        private readonly ISchemasRepository schemasRepository;

        public SchemaColumnsService(ISchemaColumnsRepository schemaColumnsRepository, ISchemasRepository schemasRepository)
        {
            this.schemaColumnsRepository = schemaColumnsRepository;
            this.schemasRepository = schemasRepository;
        }

        public async Task<IList<SchemaColumnEntity>> GetBySchema(Guid schemaId)
        {
            return await schemaColumnsRepository.GetBySchema(schemaId);
        }

        public async Task<SchemaColumnEntity> CreateAsync(SchemaColumnEntity newItem)
        {
            // ReSharper disable once PossibleInvalidOperationException
            if (await schemasRepository.IsSchemaInUseAsync(newItem.SchemaId))
            {
                if (newItem.IsRequired)
                {
                    throw new ArgumentException("Only fields that are not required allowed to be added to schemas being in use");
                }
            }

            return await schemaColumnsRepository.CreateAsync(newItem);
        }

        public async Task<SchemaColumnEntity> GetItemAsync(Guid itemKey)
        {
            return await schemaColumnsRepository.GetItemAsync(itemKey);
        }
    }
}
