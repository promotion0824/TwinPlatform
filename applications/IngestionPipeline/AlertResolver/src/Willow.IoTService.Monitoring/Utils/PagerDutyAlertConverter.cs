using System;
using System.Runtime.CompilerServices;
using Willow.IoTService.Monitoring.Dtos.PagerDuty;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Models;

[assembly: InternalsVisibleTo("Willow.IoTService.Monitoring.UnitTests")]
namespace Willow.IoTService.Monitoring.Utils
{
    public static class PagerDutyAlertConverter
    {
        public static PagerDutyAlert FromAlertNotification(AlertNotification alertNotification, string routingKey)
        {
            return new PagerDutyAlert
            {
                RoutingKey = routingKey,
                Action = alertNotification.AutoResolve ? EventAction.Resolve : EventAction.Trigger,
                DedupKey = alertNotification.AlertKey,
                EventPayload = new Payload
                {
                    Summary = $"[{alertNotification.AlertName}] {alertNotification.Message}",
                    Source = alertNotification.Source,
                    Severity = FromAlertSeverity(alertNotification.Severity),
                    Timestamp = alertNotification.Timestamp,
                    CustomDetails = alertNotification.Data
                }
            };
        }

        internal static PagerDutySeverity FromAlertSeverity(AlertSeverity alertSeverity)
        {
            if (Enum.TryParse<PagerDutySeverity>(alertSeverity.ToString(), out var severity))
            {
                return severity;
            }

            throw new NotSupportedException($"AlertSeverity '{alertSeverity}' not supported");
        }
    }
}