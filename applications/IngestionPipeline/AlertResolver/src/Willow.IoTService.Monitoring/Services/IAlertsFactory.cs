using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Services
{
    public interface IAlertsFactory
    {
        IEnumerable<IAlert> CreateAlerts(IList<ConnectorConfigInfo> connectorConfigInfos);
    }

    public sealed class AlertsFactory : IAlertsFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly List<Type> _alertTypes = new();

        public static IEnumerable<Type> AlertTypes => _alertTypes;

        static AlertsFactory()
        {
            _alertTypes.AddRange(ScanForAlertTypes());
        }

        public AlertsFactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public static IEnumerable<Type> ScanForAlertTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(p => !p.IsAbstract && typeof(IAlert).IsAssignableFrom(p))
                .Distinct();
        }

        private static bool AlertTypeIsRegistered(Type alertType)
        {
            return AlertNotificationChannelSpec.Registry.Any(x => x.AlertTypes.Contains(alertType));
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t is not null)!;
            }
        }

        public IEnumerable<IAlert> CreateAlerts(IList<ConnectorConfigInfo> connectorConfigInfos)
        {
            var alerts = new List<IAlert>();

            foreach (var alertType in _alertTypes.Where(AlertTypeIsRegistered))
            {
                foreach (var connectorConfigInfo in connectorConfigInfos)
                {
                    var scope = _serviceProvider.CreateScope();

                    //NoInputEventsReceived is common for both ADX and non-ADX customers - applicable for SAJs
                    if (scope.ServiceProvider.GetService(alertType) is IAlert alert &&
                        alert.ConnectorConnectionTypes.Contains(connectorConfigInfo.ConnectionType) &&
                        (alert.AlertType == "NoInputEventsReceived" || !(connectorConfigInfo.IsADXEnabled ^ alert.IsADXEnabled)))
                    {
                        alert.ConnectorConfigInfo = connectorConfigInfo;
                        alerts.Add(alert);
                    }
                }
            }

            return alerts;
        }
    }
}