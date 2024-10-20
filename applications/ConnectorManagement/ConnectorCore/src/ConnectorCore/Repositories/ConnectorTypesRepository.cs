namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class ConnectorTypesRepository : IConnectorTypesRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public ConnectorTypesRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<ConnectorTypeEntity>> GetAllAsync()
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[ConnectorType]";
                var data = await conn.QueryAsync<ConnectorTypeEntity>(sql);
                return data.ToList();
            }
        }

        public async Task<ConnectorTypeEntity> GetItemAsync(Guid itemKey)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[ConnectorType] where [Id] = @itemKey";
                var data = await conn.QuerySingleOrDefaultAsync<ConnectorTypeEntity>(sql, new { itemKey });
                return data;
            }
        }

        public async Task<ConnectorTypeEntity> CreateAsync(ConnectorTypeEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO [dbo].[ConnectorType] ([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId])
                            VALUES (@Id, @Name, @ConnectorConfigurationSchemaId, @DeviceMetadataSchemaId, @PointMetadataSchemaId)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }
    }
}
