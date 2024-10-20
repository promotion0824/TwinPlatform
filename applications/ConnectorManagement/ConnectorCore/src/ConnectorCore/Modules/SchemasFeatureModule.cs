namespace ConnectorCore.Modules
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Repositories;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    // ReSharper disable once UnusedMember.Global
    internal class SchemasFeatureModule : IFeatureModule
    {
        public void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddTransient<ISchemasRepository, SchemasRepository>();
        }

        public void Startup(IServiceProvider serviceProvider, IConfiguration config)
        {
        }
    }
}
