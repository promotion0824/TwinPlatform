namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class DevicesRepository : IDevicesRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public DevicesRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<DeviceEntity> GetByExternalPointId(Guid siteId, string externalPointId)
        {
            var sql = @"select d.*
                        from [dbo].[Device] d, [dbo].[Point] p
                        where p.[DeviceId] = d.[Id]
                        and d.[SiteId] = @siteId and p.[SiteId] = @siteId
                        and p.[ExternalPointId] = @externalPointId";

            using (var conn = await connectionProvider.GetConnection())
            {
                var data = await conn.QuerySingleOrDefaultAsync<DeviceEntity>(sql, new { siteId, externalPointId });
                return data;
            }
        }

        public async Task<IList<Guid>> GetAllIdsForConnectorIdAsync(Guid connectorId)
        {
            var sql = @"select d.Id from [dbo].[Device] d where d.[ConnectorId] = @connectorId;";

            using (var conn = await connectionProvider.GetConnection())
            {
                var data = await conn.QueryAsync<Guid>(sql, new { connectorId });
                return data.ToList();
            }
        }

        public async Task<IList<DeviceEntity>> GetByConnectorIdAsync(Guid connectorId, bool? isEnabled = null)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Device] where [ConnectorId] = @connectorId";
                if (isEnabled.HasValue)
                {
                    sql += " and [IsEnabled] = @isEnabled";
                }

                var data = await conn.QueryAsync<DeviceEntity>(sql, new { connectorId, isEnabled });
                return data.ToList();
            }
        }

        public async Task<IList<DeviceEntity>> GetBySiteIdAsync(Guid siteId, bool? isEnabled = null)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Device] where [SiteId] = @siteId";
                if (isEnabled.HasValue)
                {
                    sql += " and [IsEnabled] = @isEnabled";
                }

                var data = await conn.QueryAsync<DeviceEntity>(sql, new { siteId, isEnabled });
                return data.ToList();
            }
        }

        public async Task<DeviceEntity> UpdateAsync(DeviceEntity updateItem)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"update [dbo].[Device] 
                                set [Name]=@Name, 
                                    [ExternalDeviceId]=@ExternalDeviceId, 
                                    [RegistrationId]=@RegistrationId, 
							        [RegistrationKey]=@RegistrationKey, 
                                    [Metadata]=@Metadata,
                                    [IsDetected]=@IsDetected,
                                    [IsEnabled]=@IsEnabled
                              where [Id]=@Id";
                var rowsAffected = await conn.ExecuteAsync(sql, updateItem);
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(
                        $"[{typeof(DeviceEntity).Name}] with key [{updateItem.Id}] was not found");
                }
            }

            return await GetItemAsync(updateItem.Id);
        }

        public async Task<DeviceEntity> GetItemAsync(Guid itemKey)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Device] where [Id] = @itemKey";
                var data = await conn.QuerySingleOrDefaultAsync<DeviceEntity>(sql, new { itemKey });
                return data;
            }
        }

        public async Task<DeviceEntity> GetDeviceByExternalId(Guid connectorId, string externalDeviceId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Device] where [ConnectorId] = @connectorId and [ExternalDeviceId] = @externalDeviceId";
                var data = await conn.QuerySingleOrDefaultAsync<DeviceEntity>(sql, new { connectorId, externalDeviceId });
                return data;
            }
        }

        public async Task<DeviceEntity> CreateAsync(DeviceEntity newItem)
        {
            if (!string.IsNullOrEmpty(newItem.ExternalDeviceId))
            {
                //if the device with the same external id already exists for connector - ignore the insert
                var existingDevice = await GetDeviceByExternalId(newItem.ConnectorId, newItem.ExternalDeviceId);
                if (existingDevice != null)
                {
                    return existingDevice;
                }
            }

            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO [dbo].[Device] ([Id], [Name], [ClientId], [SiteId], [ExternalDeviceId], [RegistrationId], 
							        [RegistrationKey], [Metadata], [IsDetected], [ConnectorId], [IsEnabled])
                            VALUES (@Id, @Name, @ClientId, @SiteId, @ExternalDeviceId, @RegistrationId, 
                                    @RegistrationKey, @Metadata, @IsDetected, @ConnectorId, @IsEnabled)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }
    }
}
