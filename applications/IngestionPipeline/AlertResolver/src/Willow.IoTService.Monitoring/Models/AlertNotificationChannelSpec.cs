using System;
using System.Collections.Generic;
using System.Linq;
using Willow.IoTService.Monitoring.Services;

namespace Willow.IoTService.Monitoring.Models
{
    public class AlertNotificationChannelSpec
    {
        internal static readonly List<AlertNotificationChannelSpec> Registry = new List<AlertNotificationChannelSpec>();

        private AlertNotificationChannelSpec(Type channelType)
        {
            AlertNotificationChannelType = channelType;
        }

        public static AlertNotificationChannelSpec For<T>(bool isEnabled = true) where T : IAlertNotificationChannel
        {
            return new AlertNotificationChannelSpec(typeof(T)) { ChannelIsEnabled = isEnabled };
        }

        public Type AlertNotificationChannelType { get; }

        public bool ChannelIsEnabled { get; set; }

        public List<Type> AlertTypes { get; } = new List<Type>();

        public void ForAllAlerts()
        {
            var alertTypes = AlertsFactory.ScanForAlertTypes();

            foreach (var type in alertTypes)
            {
                ForAlertType(type);
            }
        }

        private AlertNotificationChannelSpec ForAlertType(Type alertType)
        {
            var existingConfig = Registry.FirstOrDefault(i => i.AlertNotificationChannelType == AlertNotificationChannelType);

            if (existingConfig != null)
            {
                foreach (var t in existingConfig.AlertTypes)
                {
                    if (!AlertTypes.Contains(t))
                    {
                        AlertTypes.Add(t);
                    }
                }

                Registry.Remove(existingConfig);
            }

            if (!AlertTypes.Contains(alertType))
            {
                AlertTypes.Add(alertType);
            }

            Registry.Add(this);

            return this;
        }

        public AlertNotificationChannelSpec ForAlert<TAlert>() where TAlert : IAlert
        {
            return ForAlertType(typeof(TAlert));
        }

        public static bool IsEnabledForChannel(IAlert alert, IAlertNotificationChannel channel)
        {
            var config = Registry.FirstOrDefault(i => i.AlertNotificationChannelType == channel.GetType());

            if (config == null)
            {
                return false;
            }

            return config.AlertTypes.Any(i => i == alert.GetType());
        }

        public static bool IsChannelEnabled(IAlertNotificationChannel channel)
        {
            var config = Registry.FirstOrDefault(i => i.AlertNotificationChannelType == channel.GetType());

            if (config == null)
            {
                return false;
            }

            return config.ChannelIsEnabled;
        }
    }
}