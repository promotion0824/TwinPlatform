using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApiServices(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseHsts();

            if (configuration.GetValue<bool>("EnableSwagger"))
            {
                var routePrefix = string.Empty;
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{routePrefix}" } };
                    });
                });
                app.UseSwaggerUI(options =>
                {
                    var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                    foreach (var groupName in provider.ApiVersionDescriptions.Select(x => x.GroupName))
                    {
                        options.SwaggerEndpoint($"{routePrefix}/swagger/{groupName}/swagger.json", $"{assemblyName} API {groupName.ToUpperInvariant()}");
                        
                    }

                    options.DocumentTitle = $"{assemblyName} - {env.EnvironmentName}";

                    if (configuration.GetValue<string>("Auth0:ClientId") != null)
                    {
                        var clientId = configuration.GetValue<string>("Auth0:ClientId");
                        options.OAuthClientId(clientId);
                        var audience = configuration.GetValue<string>("Auth0:Audience");
                        options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "audience", audience } });
                    }
                });
            }

            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
