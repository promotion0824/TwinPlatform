//-----------------------------------------------------------------------
// <copyright file="ConfigureSwaggerOptions.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.SmartPlaces.Facilities.MSMConnectorApi.Modules
{
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Reflection;

    public class ConfigureSwaggerOptions
    : IConfigureNamedOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Configure each API discovered for Swagger Documentation
        /// </summary>
        /// <param name="options"></param>
        public void Configure(SwaggerGenOptions options)
        {
            // add swagger document for every API version discovered
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    CreateVersionInfo(description));
            }
        }

        /// <summary>
        /// Configure Swagger Options. Inherited from the Interface
        /// </summary>
        /// <param name="name"></param>
        /// <param name="options"></param>
        public void Configure(string name, SwaggerGenOptions options)
        {
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFileDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            options.IncludeXmlComments(Path.Combine(xmlFileDir, xmlFileName));

            options.UseInlineDefinitionsForEnums();
            options.CustomOperationIds(operation =>
            {
                if (operation.ActionDescriptor is ControllerActionDescriptor descriptor)
                {
                    return descriptor.ActionName;
                }

                return operation.ActionDescriptor.Id;
            });

            Configure(options);
        }

        /// <summary>
        /// Create information about the version of the API
        /// </summary>
        /// <param name="description"></param>
        /// <returns>Information about the API</returns>
        private OpenApiInfo CreateVersionInfo(
                ApiVersionDescription desc)
        {
            var info = new OpenApiInfo()
            {
                Title = "MSM Connector Api",
                Version = desc.ApiVersion.ToString()
            };

            if (desc.IsDeprecated)
            {
                info.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
            }

            return info;
        }
    }
}
