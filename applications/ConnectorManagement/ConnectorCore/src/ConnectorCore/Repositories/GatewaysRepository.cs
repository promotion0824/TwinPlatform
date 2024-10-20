namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class GatewaysRepository : IGatewaysRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public GatewaysRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<GatewayEntity>> GetByConnectorIdAsync(Guid connectorId, bool? isEnabled = null)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @$"SELECT g.*, c.*
                            FROM [dbo].[Gateway] AS g
                            INNER JOIN [dbo].[GatewayConnector] AS gc ON gc.GatewayId = g.Id
                            INNER JOIN [dbo].[Connector] AS c ON c.Id = gc.ConnectorId
                            WHERE g.Id = (
                                SELECT GatewayId FROM [dbo].[GatewayConnector] WHERE ConnectorId = @connectorId
                            ) {(isEnabled == null ? string.Empty : $"AND IsEnabled = ${isEnabled}")}
                            ORDER BY g.Name, c.Name";

            var data = await ExecuteQueryAsync(conn, sql, new { connectorId });

            return MergeConnectors(data);
        }

        public async Task<IList<GatewayEntity>> GetBySiteIdsAsync(IEnumerable<Guid> siteIds, bool? isEnabled = null)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @$"SELECT g.*, c.* from [dbo].[Gateway] AS g
                            LEFT OUTER JOIN [dbo].[GatewayConnector] AS gc ON gc.GatewayId = g.Id
                            LEFT OUTER JOIN [dbo].[Connector] AS c ON c.Id = gc.ConnectorId
                            WHERE g.[SiteId] in @siteIds
                            {(isEnabled == null ? string.Empty : $"AND IsEnabled = ${isEnabled}")}";

            var data = await ExecuteQueryAsync(conn, sql, new { siteIds });
            return MergeConnectors(data);
        }

        public async Task<GatewayEntity> GetItemAsync(Guid itemKey)
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @"SELECT g.*, c.* from [dbo].[Gateway] AS g
                            LEFT OUTER JOIN [dbo].[GatewayConnector] AS gc ON gc.GatewayId = g.Id
                            LEFT OUTER JOIN [dbo].[Connector] AS c ON c.Id = gc.ConnectorId
                            WHERE g.Id = @itemKey
                            ORDER BY g.Name, c.Name";

            var data = await ExecuteQueryAsync(conn, sql, new { itemKey });
            return MergeConnectors(data).FirstOrDefault();
        }

        private static async Task<IEnumerable<GatewayEntity>> ExecuteQueryAsync(System.Data.IDbConnection conn, string sql, object parameters)
        {
            return await conn.QueryAsync<GatewayEntity, ConnectorEntity, GatewayEntity>(sql,
                (gateway, connector) =>
                {
                    gateway.Connectors = connector == null
                        ? null
                        : new List<ConnectorEntity>
                        {
                            connector,
                        };
                    return gateway;
                },
                parameters);
        }

        private static IList<GatewayEntity> MergeConnectors(IEnumerable<GatewayEntity> data)
        {
            return data.GroupBy(g => g.Id).Select(g =>
            {
                var gateway = g.First();
                gateway.Connectors = g.Where(i => (i.Connectors?.Any()).GetValueOrDefault()).Select(c => c.Connectors?.SingleOrDefault()).ToList();
                return gateway;
            }).ToList();
        }

        public async Task UpdateAsync(GatewayEntity gateway)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"UPDATE [dbo].[Gateway]
                            SET [Name] = @Name,
                                [CustomerId] = @CustomerId,
                                [SiteId] = @SiteId,
                                [Host] = @Host,
                                [IsEnabled] = @IsEnabled,
                                [LastHeartbeatTime] = @LastHeartbeatTime
                            WHERE [Id] = @Id";
                var rowsAffected = await conn.ExecuteAsync(sql, gateway);
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException(
                        $"[{nameof(GatewayEntity)}] with key [{gateway.Id}] was not found");
                }
            }
        }

        public async Task<GatewayEntity> CreateAsync(GatewayEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using var conn = await connectionProvider.GetConnection();
            var sql =
                @"insert into [dbo].[Gateway]([Id], [Name], [CustomerId], [SiteId], [Host], [IsEnabled], [LastHeartbeatTime])
                                                values(@Id, @Name, @CustomerId, @SiteId, @Host, @IsEnabled, @LastHeartbeatTime)";
            await conn.ExecuteAsync(sql, newItem);

            foreach (var connector in newItem.Connectors ?? new List<ConnectorEntity>())
            {
                var dp = new DynamicParameters();
                dp.Add("GatewayId", newItem.Id);
                dp.Add("ConnectorId", connector.Id);
                var sqlConnector = @"insert into [dbo].[GatewayConnector]([GatewayId], [ConnectorId])
                                                values(@GatewayId, @ConnectorId)";
                await conn.ExecuteAsync(sqlConnector, dp);
            }

            return newItem;
        }
    }
}
