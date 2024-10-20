using System;
using System.Text;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Utils
{
    public static class AlertUtils
    {
        public static string CreateAlertKey(this IAlert alert)
        {
            return alert.AlertType;
        }

        public static string CreateAlertKey(this IAlert alert, Guid siteId)
        {
            return $"{alert.AlertType}:{siteId}";
        }

        public static string CreateAlertKey(this IAlert alert, Guid siteId, string suffixId)
        {
            return CreateAlertKey(alert.AlertType, siteId, suffixId);
        }

        public static string CreateAlertKey(string alertType, Guid siteId, string suffixId)
        {
            return $"{alertType}:{siteId}:{suffixId}";
        }

        public static string GetDurationString(TimeSpan timespan)
        {
            string timeString = "";
            if (timespan.Hours != 0)
            {
                timeString = timespan.ToString("%h");
                timeString += timespan.Hours > 0 ? " hours " : " hour ";
            }
            if (timespan.Minutes != 0)
            {
                timeString += timespan.ToString("%m");
                timeString += " minutes";
            }

            return timeString;
        }
    }
}