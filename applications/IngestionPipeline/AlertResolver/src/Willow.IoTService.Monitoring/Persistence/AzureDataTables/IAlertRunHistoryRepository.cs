using System;
using System.Threading.Tasks;

namespace Willow.IoTService.Monitoring.Persistence.AzureDataTables
{
    public interface IAlertRunHistoryRepository
    {
        Task<DateTime?> GetLastAlertRun(string alertType, Guid siteId, Guid connectorId);

        Task SaveAlertRun(string alertType, Guid siteId, Guid connectorId);
    }
}