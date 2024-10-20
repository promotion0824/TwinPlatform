namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using Dapper;
    using Microsoft.Extensions.Options;

    internal class LogsRepository : ILogsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;
        private readonly HealthcheckOptions healthcheckOptions;

        public LogsRepository(IDbConnectionProvider connectionProvider, IOptions<HealthcheckOptions> options)
        {
            this.connectionProvider = connectionProvider;
            healthcheckOptions = options.Value;
        }

        public async Task<List<ConnectorStatusRecord>> GetConnectorStatusesAsync()
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var thresholdTimestamp = DateTime.UtcNow - TimeSpan.FromMinutes(healthcheckOptions.OfflineThresholdMinutes);
                var sourceDistinctTimestamp = DateTime.UtcNow - TimeSpan.FromDays(30);

                var excludeSources = healthcheckOptions.ExcludeSources.Select(s => s.ToLowerInvariant()).ToHashSet();

                var sql = $@"
                    with [ConnectorSources] as (
                        select distinct c.[Id] as [ConnectorId], l.[Source]
                        from [Connector] c
                        left join [Logs] l on l.[StartTime] >= @sourceDistinctTimestamp and l.[ConnectorId] = c.[Id]
                        where
                        c.[ConnectorTypeId] <> 'c9e2914a-2440-4af4-99fb-5235a1ef994b'
                        ),
                         [LatestLogRecords] as (
                         select row_query.* from (
                                        select l.*, row_number() over (partition by l.[ConnectorId] order by l.[EndTime] desc) as [RowNum]
                                        from [Logs] l
                                        where l.[EndTime] >= @thresholdTimestamp) as row_query
                         where row_query.[RowNum] = 1
                        )
                    select s.[ConnectorId], s.[Source], c.[Name] as [ConnectorName], c.[SiteId], c.[ClientId], c.[ErrorThreshold], sum(l.[ErrorCount]) as [ErrorCount], max(llr.PointCount) - max(llr.ErrorCount) as [LatestPointCount], c.[IsEnabled]
                    from [ConnectorSources] s
                        left join [Logs] l on (l.[Source] = s.[Source] or l.[Source] is null and s.[Source] is null) and l.[EndTime] >= @thresholdTimestamp and l.[ConnectorId] = s.[ConnectorId]
                        left join [Connector] c on s.[ConnectorId] = c.[Id]
                        left join [LatestLogRecords] llr on llr.[ConnectorId] = s.[ConnectorId]

                    group by s.[ConnectorId], s.[Source], c.[Name], c.[ErrorThreshold], c.[SiteId], c.[ClientId] , c.[IsEnabled]

                                                        ";

                var data = await conn.QueryAsync<ErrorCountStatsData>(sql, new { thresholdTimestamp, sourceDistinctTimestamp });

                return data.GroupBy(d => new { d.ConnectorId, d.ConnectorName, d.ErrorThreshold, d.SiteId, d.ClientId, d.IsEnabled })
                    .Select(connectorGroup => new ConnectorStatusRecord
                    {
                        ConnectorId = connectorGroup.Key.ConnectorId,
                        ConnectorName = connectorGroup.Key.ConnectorName,
                        SiteId = connectorGroup.Key.SiteId,
                        ClientId = connectorGroup.Key.ClientId,
                        IsEnabled = connectorGroup.Key.IsEnabled,
                        SourceStatuses = connectorGroup.Select(g => new ConnectorSourceStatusRecord
                        {
                            Source = g.Source,
                            ErrorCount = g.ErrorCount,
                            Status = g.ErrorCount.HasValue
                                    ? g.ErrorCount.Value > connectorGroup.Key.ErrorThreshold
                                        ? ConnectorStatus.OnlineWithErrors
                                        : ConnectorStatus.Online
                                    : ConnectorStatus.Offline,
                        })
                            .Where(sr => !excludeSources.Contains(sr.Source?.ToLowerInvariant()))
                            .ToList(),

                        PointsCount = connectorGroup.FirstOrDefault()?.LatestPointCount,
                    })
                    .ToList();
            }
        }

        public async Task<ConnectorLogError> GetConnectorLogError(Guid connectorId, long logId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                return await conn.QueryFirstAsync<ConnectorLogError>("select [Errors] from [dbo].[Logs] where [ConnectorId] = @connectorId and [Id] = @logId", new { connectorId, logId });
            }
        }

        public async Task<IEnumerable<LogRecordEntity>> GetLatestLogForConnectors(IEnumerable<Guid> connectorIds, string source, int count, bool includeErrors)
        {
            using var conn = await connectionProvider.GetConnection();

            var query = @$"SELECT i.[Id], i.[StartTime], i.[EndTime], i.[ConnectorId], i.[PointCount], i.[ErrorCount], i.[RetryCount], i.[Source], i.[CreatedAt] {(includeErrors ? ", i.[Errors]" : string.Empty)}
                           FROM ( SELECT
                                RowNumber = ROW_NUMBER() OVER (PARTITION BY l.ConnectorId ORDER BY l.StartTime),
                                l.[Id], l.[StartTime], l.[EndTime], l.[ConnectorId], l.[PointCount], l.[ErrorCount], l.[RetryCount], l.[Source], l.[CreatedAt] {(includeErrors ? ", l.[Errors]" : string.Empty)}
                                FROM [dbo].[Logs] l
                                WHERE l.[ConnectorId] IN @connectorIds
                                {(!string.IsNullOrWhiteSpace(source) ? " and l.[Source] = @source" : string.Empty)}
                           ) i
                           WHERE i.[RowNumber] <= @count
                           ORDER BY i.[ConnectorId], i.[StartTime] desc";

            var data = await conn.QueryAsync<LogRecordEntity>(query, new { count, connectorIds, source });
            return data;
        }

        public async Task<List<LogRecordEntity>> GetLatestLogForConnector(Guid connectorId, string source, int count, bool includeErrors)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var queryBuilder = new StringBuilder($"select top {count} ");
                queryBuilder.Append(
                    "[Id], [StartTime], [EndTime], [ConnectorId], [PointCount], [ErrorCount], [RetryCount], [Source], [CreatedAt]");
                if (includeErrors)
                {
                    queryBuilder.Append(", [Errors]");
                }

                queryBuilder.Append(" from [dbo].[Logs] where [ConnectorId] = @connectorId");
                if (!string.IsNullOrWhiteSpace(source))
                {
                    queryBuilder.Append(" and [Source] = @source");
                }

                queryBuilder.Append(" order by [StartTime] desc");

                var data = await conn.QueryAsync<LogRecordEntity>(queryBuilder.ToString(), new { connectorId, source });
                return data.ToList();
            }
        }

        public async Task<LogRecordEntity> CreateAsync(LogRecordEntity newItem)
        {
            newItem.CreatedAt = DateTime.UtcNow;
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "insert into [dbo].[Logs]([StartTime], [EndTime], [ConnectorId], [PointCount], [ErrorCount], [RetryCount], [Errors], [Source], [CreatedAt]) " +
                            "OUTPUT Inserted.[Id] " +
                            "values(@StartTime, @EndTime, @ConnectorId, @PointCount, @ErrorCount, @RetryCount, @Errors, @Source, @CreatedAt)";

                newItem.Id = await conn.QuerySingleAsync<long>(sql, newItem);
            }

            return newItem;
        }

        public async Task<List<LogRecordEntity>> GetLogsForConnectorAsync(Guid connectorId, DateTime start, DateTime? end, string source = null)
        {
            if (end == null)
            {
                end = DateTime.UtcNow;
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sourceFilter = string.IsNullOrEmpty(source)
                    ? "1 = 1"
                    : "[Source] = @source";

                var sql = $"SELECT * FROM [dbo].[Logs] WHERE [ConnectorId] = @connectorId AND [StartTime] >= @start AND [StartTime] <= @end AND {sourceFilter} ORDER BY [StartTime]";
                var data = await conn.QueryAsync<LogRecordEntity>(sql, new { connectorId, start, end = end.Value, source });
                return data.ToList();
            }
        }

        private class ErrorCountStatsData
        {
            public Guid ConnectorId { get; set; }

            public string ConnectorName { get; set; }

            public int? ErrorCount { get; set; }

            public int ErrorThreshold { get; set; }

            public string Source { get; set; }

            public int? LatestPointCount { get; set; }

            public Guid SiteId { get; set; }

            public Guid ClientId { get; set; }

            public bool IsEnabled { get; set; }
        }
    }
}
