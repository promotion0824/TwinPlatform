namespace Willow.Api.Swagger;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// A class for swagger extensions.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Add the swagger services to the service collection.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="configuration">The configuration for the application.</param>
    /// <param name="setupAction">A function to call to perform the setup.</param>
    public static void AddSwagger(this IServiceCollection services, IConfiguration configuration, Action<SwaggerGenOptions>? setupAction = null)
    {
        services.AddEndpointsApiExplorer();
        var options = configuration.GetSwaggerOptions();
        if (options is not null && options.IsSwaggerEnabled())
        {
            services.AddSwaggerGen(c =>
            {
                var info = new OpenApiInfo { Title = options.DocumentTitle, Version = options.DocumentVersion };
                c.SwaggerDoc(options.DocumentVersion, info);
                setupAction?.Invoke(c);
            });
        }
    }

    /// <summary>
    /// Use swagger in the application.
    /// </summary>
    /// <param name="app">The Application Builder instance.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void UseSwagger(this IApplicationBuilder app, IConfiguration configuration)
    {
        var options = configuration.GetSwaggerOptions();
        if (options is not null && options.IsSwaggerEnabled())
        {
            app.UseSwagger(c =>
            {
                if (!string.IsNullOrWhiteSpace(options.RoutePrefix))
                {
                    c.PreSerializeFilters.Add((swaggerDoc, _) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = $"/{options.RoutePrefix}" } };
                    });
                }
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{(string.IsNullOrEmpty(options.RoutePrefix) ? string.Empty : "/")}{options.RoutePrefix}/swagger/{options.DocumentVersion}/swagger.json", options.DocumentTitle);
            });
        }
    }

    private static SwaggerOptions? GetSwaggerOptions(this IConfiguration configuration)
    {
        var options = configuration.GetSection("SwaggerOptions")?.Get<SwaggerOptions>();
        return options;
    }
}
