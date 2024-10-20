using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Extensions
{
    public static class AlertExtensions
    {
        public static bool IsEnabledForChannel(this IAlert alert, IAlertNotificationChannel channel)
        {
            return AlertNotificationChannelSpec.IsEnabledForChannel(alert, channel);
        }
    }
}