using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.IoTService.Monitoring.Persistence.AzureDataTables
{
    public interface IActiveAlertRepository
    {
        Task LogAlert(string alertKey);
        Task LogAlert(IEnumerable<string> alertKeys);
        Task DeleteAlert(string alertKey);
        Task DeleteAlert(IEnumerable<string> alertKeys);
        Task<bool> IsAlertActive(string alertKey);
        Task<IEnumerable<(string AlertKey, bool IsActive)>> IsAlertActive(IEnumerable<string> alertKeys);
        Task<DateTime?> AlertLastOccurence(string alertKey);
        Task<IEnumerable<(string AlertKey, DateTime? LastOccurence)>> AlertLastOccurence(IEnumerable<string> alertKeys);
    }
}