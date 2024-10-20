namespace ConnectorCore.Modules
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Repositories;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class ConnectorsFeatureModule : IFeatureModule
    {
        public void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddTransient<IConnectorsRepository, ConnectorsRepository>();
            services.AddTransient<IConnectorsService, ConnectorsService>();
            services.AddTransient<IIotRegistrationService, IotRegistrationService>();
            services.AddTransient<IEventNotificationService, EventNotificationService>();
        }

        public void Startup(IServiceProvider serviceProvider, IConfiguration config)
        {
        }
    }
}
