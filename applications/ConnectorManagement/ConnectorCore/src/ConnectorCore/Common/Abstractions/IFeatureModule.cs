namespace ConnectorCore.Common.Abstractions
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal interface IFeatureModule
    {
        void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env);

        void Startup(IServiceProvider serviceProvider, IConfiguration config);
    }
}
