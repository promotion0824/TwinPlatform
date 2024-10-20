namespace ConnectorCore.Repositories
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class SchemasRepository : ISchemasRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public SchemasRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<SchemaEntity> CreateAsync(SchemaEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sql =
                    "insert into [dbo].[Schema]([Id], [Name], [ClientId], [Type]) values(@Id, @Name, @ClientId, @Type)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        /// <summary>
        /// Checks if there are connector type entities referencing provided schema id.
        /// </summary>
        /// <param name="schemaId">Id of the schema to check.</param>
        /// <returns>True if exists any connector type referencing the schema with provided id, otherwise False.</returns>
        public async Task<bool> IsSchemaInUseAsync(Guid schemaId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select 'ConectorType.PointMetadataSchemaId' as 'Reference'
                            from [ConnectorType] where [PointMetadataSchemaId] = @schemaId
                            union
                            select 'ConectorType.DeviceMetadataSchemaId' as 'Reference'
                            from [ConnectorType] where [DeviceMetadataSchemaId] = @schemaId
                            union
                            select 'ConectorType.ConnectorConfigurationSchemaId' as 'Reference'
                            from [ConnectorType] where [ConnectorConfigurationSchemaId] = @schemaId";

                var references = (await conn.QueryAsync<string>(sql, new { schemaId })).ToList();

                return references.Any();
            }
        }
    }
}
