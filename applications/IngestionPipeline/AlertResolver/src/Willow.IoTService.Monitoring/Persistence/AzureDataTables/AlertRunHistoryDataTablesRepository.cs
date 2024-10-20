using System;
using System.Linq;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Entities;
using Willow.IoTService.Monitoring.Services.DataTable;

namespace Willow.IoTService.Monitoring.Persistence.AzureDataTables
{
    public class AlertRunHistoryDataTablesRepository : IAlertRunHistoryRepository
    {
        private readonly IDataTableService _dataTableService;
        private readonly string _alertRunHistoryTableName = "AlertRunHistory";

        public AlertRunHistoryDataTablesRepository(IDataTableService dataTableService)
        {
            _dataTableService = dataTableService;
        }

        public async Task<DateTime?> GetLastAlertRun(string alertType, Guid siteId, Guid connectorId)
        {
            var alertRuns = await _dataTableService.GetEntitiesList<AlertRunEntity>(_alertRunHistoryTableName,
                                                                                    (x) => x.AlertType == alertType &&
                                                                                           x.PartitionKey == GetPartitionKey(siteId) &&
                                                                                           x.RowKey == GetRowKey(connectorId, alertType));

            return alertRuns.OrderByDescending(x => x.Timestamp).FirstOrDefault()?.Timestamp?.UtcDateTime;
        }

        public async Task SaveAlertRun(string alertType, Guid siteId, Guid connectorId)
        {
            var alertRunEntity = new AlertRunEntity()
            {
                PartitionKey = GetPartitionKey(siteId),
                RowKey = GetRowKey(connectorId, alertType),
                Timestamp = DateTime.UtcNow,
                AlertType = alertType
            };

            await _dataTableService.SaveEntity<AlertRunEntity>(_alertRunHistoryTableName, alertRunEntity);
        }

        private static string GetPartitionKey(Guid siteId)
        {
            return $"site-{siteId}";
        }

        private static string GetRowKey(Guid connectorId, string alertType)
        {
            return $"connector-{connectorId}-{alertType}";
        }
    }
}