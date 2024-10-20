using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Extensions
{
    public static class IAlertNotificationChannelExtensions
    {
        public static bool IsEnabled(this IAlertNotificationChannel channel)
        {
            return AlertNotificationChannelSpec.IsChannelEnabled(channel);
        }
    }
}