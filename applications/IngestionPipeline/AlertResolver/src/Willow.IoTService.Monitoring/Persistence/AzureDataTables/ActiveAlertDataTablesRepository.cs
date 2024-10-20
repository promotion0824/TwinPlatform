using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Entities;
using Willow.IoTService.Monitoring.Services.DataTable;

namespace Willow.IoTService.Monitoring.Persistence.AzureDataTables
{
    public class ActiveAlertDataTablesRepository : IActiveAlertRepository
    {
        private readonly IDataTableService _dataTableService;
        private readonly string _activeAlertTableName = "ActiveAlerts";

        public ActiveAlertDataTablesRepository(IDataTableService dataTableService)
        {
            _dataTableService = dataTableService;
        }

        public async Task<DateTime?> AlertLastOccurence(string alertKey)
        {
            var result = await AlertLastOccurence(new[] { alertKey });

            return result.First().LastOccurence;
        }

        public async Task<IEnumerable<(string AlertKey, DateTime? LastOccurence)>> AlertLastOccurence(IEnumerable<string> alertKeys)
        {
            var alerts = await _dataTableService.GetEntitiesList<ActiveAlertEntity>(_activeAlertTableName);

            return alertKeys.Select(key => (key, alerts.FirstOrDefault(alert => alert.AlertKey == key)?.LatestOccurence));
        }

        public async Task DeleteAlert(string alertKey)
        {
            await DeleteAlert(new[] { alertKey });
        }

        public async Task DeleteAlert(IEnumerable<string> alertKeys)
        {
            var alerts = await _dataTableService.GetEntitiesList<ActiveAlertEntity>(_activeAlertTableName);

            alerts = alerts.Where(a => alertKeys.Contains(a.AlertKey)).ToList();

            var groupedAlerts = alerts.GroupBy(x => x.PartitionKey).ToList();

            foreach (var groupedAlert in groupedAlerts)
            {
                await _dataTableService.DeleteEntities<ActiveAlertEntity>(_activeAlertTableName, groupedAlert.ToList());
            }
        }

        public async Task<bool> IsAlertActive(string alertKey)
        {
            var result = await IsAlertActive(new[] { alertKey });

            return result.First().IsActive;
        }

        public async Task<IEnumerable<(string AlertKey, bool IsActive)>> IsAlertActive(IEnumerable<string> alertKeys)
        {
            var alerts = await _dataTableService.GetEntitiesList<ActiveAlertEntity>(_activeAlertTableName);

            return alertKeys.Select(key => (key, alerts.Any(alert => alert.AlertKey == key)));
        }

        public async Task LogAlert(string alertKey)
        {
            await LogAlert(new[] { alertKey });
        }

        public async Task LogAlert(IEnumerable<string> alertKeys)
        {
            var alerts = await _dataTableService.GetEntitiesList<ActiveAlertEntity>(_activeAlertTableName);

            var dateTimeNow = DateTime.UtcNow;

            var insert = alertKeys
              .Where(k => !alerts.Any(a => a.AlertKey == k))
              .Select(k => new ActiveAlertEntity
              {
                  AlertKey = k,
                  AlertRaised = dateTimeNow,
                  LatestOccurence = dateTimeNow,
                  AlertCount = 1,
                  PartitionKey = GetPartitionKey(k),
                  RowKey = GetRowKey(k),
                  Timestamp = dateTimeNow
                  
              }).ToList();

            var update = alerts.Where(a => alertKeys.Contains(a.AlertKey)).ToList();
            update.ForEach(a =>
            {
                a.AlertCount++;
                a.LatestOccurence = dateTimeNow;
            });

            insert.AddRange(update);

            var grouped = insert.GroupBy(x => x.PartitionKey).ToList();

            foreach (var group in grouped)
            {
                await _dataTableService.SaveEntities<ActiveAlertEntity>(_activeAlertTableName, group.ToList());
            }
        }

        private static string GetPartitionKey(string alertKey)
        {
            var splitValues = alertKey.Split(':');

            return $"{splitValues[0]}-{splitValues[1]}";
        }

        private static string GetRowKey(string alertKey)
        {
            var splitValues = alertKey.Split(':');

            return $"{splitValues[2]}-{splitValues[3]}";
        }
    }
}
