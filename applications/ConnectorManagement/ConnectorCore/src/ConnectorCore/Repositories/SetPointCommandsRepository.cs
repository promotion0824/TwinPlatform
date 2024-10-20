namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class SetPointCommandsRepository : ISetPointCommandsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public SetPointCommandsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<SetPointCommandEntity>> GetByConnectorIdAsync(Guid connectorId)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @"SELECT *
                            FROM [dbo].[SetPointCommand] 
                            WHERE ConnectorId = @connectorId
                                AND Status < 4
                            ORDER BY CreatedAt";

            var data = await conn.QueryAsync<SetPointCommandEntity>(sql, new { connectorId });
            return data.ToList();
        }

        public async Task<IList<SetPointCommandEntity>> GetBySiteIdAsync(Guid siteId)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @"SELECT *
                            FROM [dbo].[SetPointCommand] 
                            WHERE SiteId = @siteId
                            ORDER BY CreatedAt";

            var data = await conn.QueryAsync<SetPointCommandEntity>(sql, new { siteId });
            return data.ToList();
        }

        public async Task<SetPointCommandEntity> GetItemAsync(Guid entityId)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @"SELECT *
                            FROM [dbo].[SetPointCommand] 
                            WHERE Id = @entityId";

            return await conn.QuerySingleOrDefaultAsync<SetPointCommandEntity>(sql, new { entityId });
        }

        public async Task UpdateAsync(SetPointCommandEntity entity)
        {
            using var conn = await connectionProvider.GetConnection();
            const string sql = @"UPDATE [dbo].[SetPointCommand]
                                SET [ConnectorId] = @ConnectorId,
	                                [EquipmentId] = @EquipmentId,
                                    [InsightId] = @InsightId,
                                    [PointId] = @PointId,
                                    [SetPointId] = @SetPointId,
                                    [OriginalValue] = @OriginalValue,
                                    [DesiredValue] = @DesiredValue,
                                    [DesiredDurationMinutes] = @DesiredDurationMinutes,
                                    [Status] = @Status,
                                    [LastUpdatedAt] = @LastUpdatedAt,
                                    [ErrorDescription] = @ErrorDescription,
                                    [Unit] = @Unit,
                                    [Type] = @Type,
                                    [CurrentReading] = @CurrentReading
                                WHERE Id = @Id";
            var rowsAffected = await conn.ExecuteAsync(sql, entity);
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException(
                    $"[{nameof(SetPointCommandEntity)}] with key [{entity.Id}] was not found");
            }
        }

        public async Task InsertAsync(SetPointCommandEntity entity)
        {
            using var conn = await connectionProvider.GetConnection();
            const string sql = @"INSERT INTO [dbo].[SetPointCommand]
                                    ([Id], [SiteId], [ConnectorId], [EquipmentId], [InsightId], [PointId], [SetPointId], [OriginalValue], [DesiredValue], [DesiredDurationMinutes], [Status], [CreatedAt], [LastUpdatedAt], [CreatedBy], [Unit], [Type], [CurrentReading])
                                VALUES
                                   (@Id, @SiteId, @ConnectorId, @EquipmentId, @InsightId, @PointId, @SetPointId, @OriginalValue, @DesiredValue, @DesiredDurationMinutes, @Status, @CreatedAt, @LastUpdatedAt, @CreatedBy, @Unit, @Type, @CurrentReading)";

            var rowsAffected = await conn.ExecuteAsync(sql, entity);
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException(
                    $"[{nameof(SetPointCommandEntity)}] with key [{entity.Id}] was not found");
            }
        }

        public async Task DeleteAsync(Guid siteId, Guid entityId)
        {
            using var conn = await connectionProvider.GetConnection();

            const string sql = "UPDATE [dbo].[SetPointCommand] SET [Status] = @status WHERE [Id] = @entityId AND [SiteId] = @siteId";

            var rowsAffected = await conn.ExecuteAsync(sql, new { entityId, siteId, status = (int)SetPointCommandStatus.Deleted });
            if (rowsAffected == 0)
            {
                throw new KeyNotFoundException(
                    $"[{nameof(SetPointCommandEntity)}] with key [{entityId}] was not found");
            }
        }
    }
}
