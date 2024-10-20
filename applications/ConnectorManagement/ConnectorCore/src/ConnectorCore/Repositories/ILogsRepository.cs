namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;

    internal interface ILogsRepository
    {
        Task<LogRecordEntity> CreateAsync(LogRecordEntity newItem);

        Task<IEnumerable<LogRecordEntity>> GetLatestLogForConnectors(IEnumerable<Guid> connectorIds, string source, int count, bool includeErrors);

        Task<List<LogRecordEntity>> GetLatestLogForConnector(Guid connectorId, string source, int count, bool includeErrors);

        Task<List<LogRecordEntity>> GetLogsForConnectorAsync(Guid connectorId, DateTime start, DateTime? end, string source = null);

        Task<List<ConnectorStatusRecord>> GetConnectorStatusesAsync();

        Task<ConnectorLogError> GetConnectorLogError(Guid connectorId, long logId);
    }
}
