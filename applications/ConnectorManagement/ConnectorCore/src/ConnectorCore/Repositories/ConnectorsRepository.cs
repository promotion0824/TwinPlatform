namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using Dapper;
    using Willow.Infrastructure.Exceptions;

    internal class ConnectorsRepository : IConnectorsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public ConnectorsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<ConnectorEntity> GetByTypeAsync(Guid siteId, Guid connectorTypeId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Connector] where [SiteId] = @siteId and [ConnectorTypeId] = @connectorTypeId";
                var data = await conn.QuerySingleOrDefaultAsync<ConnectorEntity>(sql, new { siteId, connectorTypeId });
                return data;
            }
        }

        public async Task<IList<ConnectorEntity>> GetBySiteIdAsync(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"
select [Connector].[id]
      ,[Connector].[Name]
      ,[ClientId]
      ,[SiteId]
      ,[Configuration]
      ,[ConnectorTypeId]
      ,[ErrorThreshold]
      ,[IsEnabled]
      ,[IsLoggingEnabled]
      ,[RegistrationId]
      ,[RegistrationKey]
      ,[LastImport]
      ,[LastUpdatedAt]
      ,[ConnectionType]
      ,[IsArchived] from Connector
join [dbo].[ConnectorType] on [ConnectorTypeId] = [ConnectorType].[Id]
where [Connector].[SiteId] = @siteId
order by [ConnectorType].[Name]
                ";
                var data = await conn.QueryAsync<ConnectorEntity>(sql, new { siteId });
                return data.ToList();
            }
        }

        public async Task<IList<ConnectorEntity>> GetByCustomerIdAsync(Guid customerId)
        {
            using var conn = await connectionProvider.GetConnection();
            const string sql = @"
                select [Connector].[id]
                      ,[Connector].[Name]
                      ,[ClientId]
                      ,[SiteId]
                      ,[Configuration]
                      ,[ConnectorTypeId]
                      ,[ErrorThreshold]
                      ,[IsEnabled]
                      ,[IsLoggingEnabled]
                      ,[RegistrationId]
                      ,[RegistrationKey]
                      ,[LastImport]
                      ,[LastUpdatedAt]
                      ,[ConnectionType]
                      ,[IsArchived] from Connector
                where [Connector].[ClientId] = @customerId
                ";
            var data = await conn.QueryAsync<ConnectorEntity>(sql, new { customerId });
            return data.ToList();
        }

        public async Task<IList<ConnectorEntity>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            if (!ids.Any())
            {
                return new List<ConnectorEntity>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var idsString = string.Join(", ", ids.Select(q => $"'{q}'"));

                var sql = $"select * from [dbo].[Connector] where [Id] in ({idsString})";
                var data = await conn.QueryAsync<ConnectorEntity>(sql);
                return data.ToList();
            }
        }

        public async Task<ConnectorEntity> GetItemAsync(Guid itemKey)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Connector] where [Id] = @itemKey";
                var data = await conn.QuerySingleOrDefaultAsync<ConnectorEntity>(sql, new { itemKey });
                return data;
            }
        }

        public async Task<ConnectorEntity> CreateAsync(ConnectorEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            newItem.ConnectionType = newItem.ConnectionType ?? string.Empty;

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO [dbo].[Connector] ([Id], [Name], [ClientId], [SiteId], [Configuration], [ConnectorTypeId], [ErrorThreshold], [IsEnabled], 
                                        [IsLoggingEnabled], [RegistrationId], [RegistrationKey], [ConnectionType])
                                        VALUES (@Id, @Name, @ClientId, @SiteId, @Configuration, @ConnectorTypeId, @ErrorThreshold, @IsEnabled, 
                                        @IsLoggingEnabled, @RegistrationId, @RegistrationKey, @ConnectionType)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        public async Task<ConnectorEntity> UpdateAsync(ConnectorEntity updateItem)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"UPDATE [dbo].[Connector]
                            SET [Name] = @Name, 
                                [Configuration] = @Configuration, 
                                [ErrorThreshold] = @ErrorThreshold, 
                                [IsEnabled] = @IsEnabled, 
                                [IsLoggingEnabled] = @IsLoggingEnabled,
                                [LastUpdatedAt] = SYSUTCDATETIME(),
                                [IsArchived] = @IsArchived
                            WHERE [Id] = @Id";
                var rowsAffected = await conn.ExecuteAsync(sql, updateItem);
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(
                        $"[{typeof(ConnectorEntity).Name}] with key [{updateItem.Id}] was not found");
                }
            }

            return await GetItemAsync(updateItem.Id);
        }

        public async Task SetEnabled(Guid connectorId, bool enabled)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"UPDATE [dbo].[Connector] 
                            SET [IsEnabled] = @enabled, 
                                [LastUpdatedAt] = SYSUTCDATETIME() 
                            WHERE [Id] = @connectorId";
                var rowsAffected = await conn.ExecuteAsync(sql, new { connectorId, enabled });
                if (rowsAffected == 0)
                {
                    throw new ResourceNotFoundException(nameof(ConnectorEntity), connectorId);
                }
            }
        }

        public async Task SetArchivedAndDisableConnector(Guid connectorId, bool archived)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "update [dbo].[Connector] set [IsArchived] = @archived, [IsEnabled] = 0 where [Id] = @connectorId";
                var rowsAffected = await conn.ExecuteAsync(sql, new { connectorId, archived });
                if (rowsAffected == 0)
                {
                    throw new ResourceNotFoundException(typeof(ConnectorEntity).Name, connectorId);
                }
            }
        }

        public async Task<DateTime?> GetLastImportBySiteAsync(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select max([LastImport]) from [dbo].[Connector] where [SiteId] = @siteId";
                var lastImport = await conn.QuerySingleOrDefaultAsync<DateTime?>(sql, new { siteId });
                return lastImport;
            }
        }

        public async Task<ConnectorDataForValidation> GetConnectorDataForValidation(Guid connectorId, Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select c.SiteId, ct.DeviceMetadataSchemaId, ct.PointMetadataSchemaId 
                            from [dbo].[Connector] c
                            inner join [dbo].[ConnectorType] ct on c.ConnectorTypeId = ct.Id
                            where c.[Id] = @connectorId and c.[SiteId]=siteId";
                var data = await conn.QuerySingleOrDefaultAsync<ConnectorDataForValidation>(sql, new { connectorId, siteId });
                return data;
            }
        }

        public async Task<Dictionary<Guid, int>> GetPointsCountPerConnectorId(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select count(p.EntityId) as PointsCount, d.ConnectorId
                from [dbo].[Point] p 
                inner join [dbo].[Device] d on d.Id = p.DeviceId
                where p.SiteId = @siteId
                group by d.ConnectorId";
                var data = await conn.QueryAsync<ConnectorPointsCount>(sql, new { siteId });
                return data.ToDictionary(x => x.ConnectorId, x => x.PointsCount);
            }
        }

        private class ConnectorPointsCount
        {
            public int PointsCount { get; set; }

            public Guid ConnectorId { get; set; }
        }
    }
}
