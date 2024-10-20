namespace ConnectorCore.Modules
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Entities.Validators;
    using ConnectorCore.Repositories;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class SchemaColumnsFeatureModule : IFeatureModule
    {
        public void Register(IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            services.AddTransient<ISchemaColumnsRepository, SchemaColumnsRepository>();
            services.AddTransient<ISchemaColumnsService, SchemaColumnsService>();
            services.AddTransient<IJsonSchemaValidator, JsonSchemaValidator>();
            services.AddTransient<IJsonSchemaDataGenerator, JsonSchemaDataGenerator>();
        }

        public void Startup(IServiceProvider serviceProvider, IConfiguration config)
        {
        }
    }
}
