namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ConnectorCore.Common.Extensions;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class ScansRepository : IScansRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public ScansRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<List<ScanEntity>> GetByConnectorIdAsync(Guid connectorId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Scan] where [ConnectorId] = @connectorId";

                var data = await conn.QueryAsync<ScanEntity>(sql, new { connectorId });
                return data.ToList();
            }
        }

        public async Task<ScanEntity> CreateAsync(ScanEntity newScanEntity)
        {
            if (newScanEntity.Id == Guid.Empty)
            {
                newScanEntity.Id = Guid.NewGuid();
            }

            newScanEntity.CreatedAt = DateTime.UtcNow;
            newScanEntity.Status = ScanStatus.New;

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO
                                        [dbo].[Scan]
                                            ([Id], [ConnectorId], [Status], [Message], [CreatedBy], [CreatedAt], [StartTime], [EndTime], [DevicesToScan], [Configuration])
                                        VALUES
                                            (@Id, @ConnectorId, @Status, @Message, @CreatedBy, @CreatedAt, @StartTime, @EndTime, @DevicesToScan, @Configuration)";
                await conn.ExecuteAsync(sql, newScanEntity);
            }

            return newScanEntity;
        }

        public async Task StopAsync(Guid connectorId, Guid scanId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var now = DateTime.UtcNow;
                var status = ScanStatus.Finished;

                const string sql = @"UPDATE [dbo].[Scan] set EndTime = @now, Status = @status, ErrorCount = 1,
                                        ErrorMessage='Scan was stopped by user.'
                                        WHERE Id=@scanId and ConnectorId=@connectorId";
                await conn.ExecuteAsync(sql, new { now, status, scanId, connectorId });
            }
        }

        public async Task<ScanEntity> GetById(Guid scanId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Scan] where [Id] = @scanId";

                return await conn.QueryFirstAsync<ScanEntity>(sql, new { scanId });
            }
        }

        public async Task PatchAsync(
            Guid connectorId,
            Guid scanId,
            ScanStatus? status,
            string errorMessage,
            int? errorCount,
            DateTime? startTime,
            DateTime? endTime)
        {
            var fieldsBuilder = new StringBuilder()
                    .AppendIfTrue(",Status=@status ", status != null)
                    .AppendIfTrue(",ErrorMessage=@errorMessage ", errorMessage != null)
                    .AppendIfTrue(",ErrorCount=@errorCount ", errorCount != null)
                    .AppendIfTrue(",StartTime=@startTime ", startTime != null)
                    .AppendIfTrue(",EndTime=@endTime ", endTime != null);

            var sqlBuilder = new StringBuilder("UPDATE [dbo].[Scan] set ")
                .Append(fieldsBuilder.ToString().TrimStart(','))
                .Append("WHERE id=@scanId and ConnectorId=@connectorId");
            using (var conn = await connectionProvider.GetConnection())
            {
                await conn.ExecuteAsync(sqlBuilder.ToString(),
                    new { status, errorMessage, errorCount, startTime, endTime, connectorId, scanId });
            }
        }
    }
}
