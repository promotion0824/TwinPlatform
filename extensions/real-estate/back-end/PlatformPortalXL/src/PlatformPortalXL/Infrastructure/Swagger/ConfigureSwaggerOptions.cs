using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlatformPortalXL.Infrastructure.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions( IApiVersionDescriptionProvider provider ) =>
            _provider = provider;

        public void Configure( SwaggerGenOptions options )
        {
	        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

            foreach ( var description in _provider.ApiVersionDescriptions )
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = assemblyName,
                        Version = description.ApiVersion.ToString(),
                    } );
            }
        }
    }
}
