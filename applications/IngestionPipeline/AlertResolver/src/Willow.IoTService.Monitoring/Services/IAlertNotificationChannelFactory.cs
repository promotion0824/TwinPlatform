using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Willow.IoTService.Monitoring.Models;

namespace Willow.IoTService.Monitoring.Services
{
    public interface IAlertNotificationChannelFactory
    {
        IEnumerable<IAlertNotificationChannel> CreateChannels();
    }

    public sealed class AlertNotificationChannelFactory : IAlertNotificationChannelFactory
    {
        private readonly IServiceProvider serviceProvider;

        public AlertNotificationChannelFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<IAlertNotificationChannel> CreateChannels()
        {
            using var scope = serviceProvider.CreateScope();

            return scope.ServiceProvider.GetServices<IAlertNotificationChannel>();
        }
    }
}