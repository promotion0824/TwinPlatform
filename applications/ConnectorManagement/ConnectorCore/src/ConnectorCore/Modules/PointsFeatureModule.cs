namespace ConnectorCore.Modules
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Repositories;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class PointsFeatureModule : IFeatureModule
    {
        public void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddTransient<IPointsRepository, PointsRepository>();
            services.AddTransient<IPointsService, PointsService>();
        }

        public void Startup(IServiceProvider serviceProvider, IConfiguration config)
        {
        }
    }
}
