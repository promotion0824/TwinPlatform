namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class SchemaColumnsRepository : ISchemaColumnsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public SchemaColumnsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<SchemaColumnEntity>> GetBySchema(Guid schemaId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[SchemaColumn] where [SchemaId] = @schemaId";
                var data = await conn.QueryAsync<SchemaColumnEntity>(sql, new { schemaId });
                return data.ToList();
            }
        }

        public async Task<IList<SchemaColumnEntity>> GetBySchemas(params Guid[] schemaIds)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[SchemaColumn] where [SchemaId] in @schemaIds";
                var data = await conn.QueryAsync<SchemaColumnEntity>(sql, new { schemaIds });
                return data.ToList();
            }
        }

        public async Task<SchemaColumnEntity> GetItemAsync(Guid itemKey)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[SchemaColumn] where [Id] = @itemKey";
                var data = await conn.QuerySingleOrDefaultAsync<SchemaColumnEntity>(sql, new { itemKey });
                return data;
            }
        }

        public async Task<SchemaColumnEntity> CreateAsync(SchemaColumnEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId]) " +
                            "values(@Id, @Name, @IsRequired, @DataType, @SchemaId)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }
    }
}
