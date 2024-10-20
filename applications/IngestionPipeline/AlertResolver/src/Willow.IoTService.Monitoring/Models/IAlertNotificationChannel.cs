using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.IoTService.Monitoring.Models
{
    public interface IAlertNotificationChannel
    {
        string ChannelName { get; }

        public bool FilterAlertsForChannel { get; }

        public TimeSpan ActiveAlertTTL { get; }

        Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Notify(IEnumerable<AlertNotification> alertNotifications);

        Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Resolve(IEnumerable<AlertNotification> alertNotifications);
    }
}