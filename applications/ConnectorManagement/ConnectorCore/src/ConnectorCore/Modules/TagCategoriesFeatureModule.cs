namespace ConnectorCore.Modules
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Repositories;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class TagCategoriesFeatureModule : IFeatureModule
    {
        public void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddTransient<ITagCategoriesRepository, TagCategoriesRepository>();
        }

        public void Startup(IServiceProvider serviceProvider, IConfiguration config)
        {
        }
    }
}
