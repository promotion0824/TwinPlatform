using System.Collections.Generic;
using Newtonsoft.Json;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Dtos.MSTeams;

namespace Willow.IoTService.Monitoring.Utils
{
    public static class TeamsMessageConverter
    {
        public static TeamsMessage FromAlertNotification(AlertNotification alertNotification)
        {
            var msg = new TeamsMessage
            {
                Title = GetMessageTitle(alertNotification),
                Text = alertNotification.Message,
                ThemeColor = GetMessageThemeColor(alertNotification)
            };

            var details = new MessageSection
            {
                ActivityTitle = "Details",
                Facts = GetMessageFacts(alertNotification)
            };

            msg.Sections.Add(details);

            return msg;
        }

        private static string GetMessageTitle(AlertNotification alertNotification)
        {
            if (alertNotification.AutoResolve)
            {
                return $"Resolved: {alertNotification.AlertName}";
            }

            return $"{alertNotification.Severity}: {alertNotification.AlertName}";
        }

        public static IEnumerable<MessageFact> GetMessageFacts(AlertNotification alertNotification)
        {
            var facts = new List<MessageFact>
            {
                new MessageFact
                {
                    Name = nameof(alertNotification.Source),
                    Value = alertNotification.Source
                },

                new MessageFact
                {
                    Name = nameof(alertNotification.Severity),
                    Value = alertNotification.Severity.ToString()
                },

                new MessageFact
                {
                    Name = $"{nameof(alertNotification.Timestamp)} (Utc)",
                    Value = alertNotification.Timestamp.ToString()
                },
            };

            if (alertNotification.Data != null)
            {
                foreach (var item in alertNotification.Data)
                {
                    facts.Add(new MessageFact
                    {
                        Name = item.Key,
                        Value = item.Value?.ToString()
                    });
                }
            }

            return facts;
        }

        public static string GetMessageThemeColor(AlertNotification alertNotification)
        {
            if (alertNotification.AutoResolve)
            {
                return HexColorCodes.Green;
            }

            if (alertNotification.Severity == AlertSeverity.Critical || alertNotification.Severity == AlertSeverity.Error)
            {
                return HexColorCodes.Red;
            }

            if (alertNotification.Severity == AlertSeverity.Warning)
            {
                return HexColorCodes.Orange;
            }

            return HexColorCodes.Blue;
        }

        public static string ToJson(this TeamsMessage teamsMessage)
        {
            return JsonConvert.SerializeObject(teamsMessage);
        }

        public static class HexColorCodes
        {
            public const string Red = "FC2D3B";

            public const string Orange = "FFA500";

            public const string Green = "72C600";

            public const string Blue = "0078d7";
        }
    }
}