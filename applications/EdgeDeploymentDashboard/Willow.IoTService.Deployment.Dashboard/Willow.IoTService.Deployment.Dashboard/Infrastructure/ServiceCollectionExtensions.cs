namespace Willow.IoTService.Deployment.Dashboard.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Willow.IoTService.Deployment.Dashboard.Application.Queries.DownloadManifests;

public static class ServiceCollectionExtensions
{
    private const string ImportFileFormat = "usermanagement.import.{0}.json";

    public static void AddVersionedControllersWithValidation(this IServiceCollection services)
    {
        services.AddControllers(
                                c =>
                                {
                                    // use json format
                                    c.OutputFormatters.Clear();
                                    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
                                    {
                                        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                                    };
                                    options.Converters.Add(new JsonStringEnumConverter());
                                    c.OutputFormatters.Add(new SystemTextJsonOutputFormatter(options));

                                    // use general error handling
                                    c.Filters.AddErrorHandlingFilters();
                                    c.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status401Unauthorized));
                                    c.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status403Forbidden));
                                    c.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status500InternalServerError));
                                })
                .AddJsonOptions(
                                c =>
                                {
                                    c.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                                    c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                                    c.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                                });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining(typeof(DownloadManifestsValidator));
        services.AddFluentValidationRulesToSwagger();

        services.AddApiVersioning(
                                  c =>
                                  {
                                      c.DefaultApiVersion = new ApiVersion(1, 0);
                                      c.AssumeDefaultVersionWhenUnspecified = true;
                                      c.ReportApiVersions = true;
                                      c.UseApiBehavior = false;
                                  });
        services.AddVersionedApiExplorer(
                                         c =>
                                         {
                                             c.GroupNameFormat = "'v'VVV";
                                             c.SubstituteApiVersionInUrl = true;
                                         });
    }

    public static void AddSwaggerGenCustom(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        var swaggerOptions = configuration.GetSection("AzureAdB2C").Get<AadOptions>();
        var authUrl = new Uri($"{configuration["AzureAdB2C:Authority"]}/oauth2/v2.0/authorize");
        var tokenUrl = new Uri($"{configuration["AzureAdB2C:Authority"]}/oauth2/v2.0/token");
        var scopes = swaggerOptions?.B2CScopes;

        services.AddSwaggerGen(
                               c =>
                               {
                                   c.DescribeAllParametersInCamelCase();
                                   var paths = Directory.GetFiles(
                                                                  System.AppContext.BaseDirectory,
                                                                  "*.xml",
                                                                  SearchOption.TopDirectoryOnly);
                                   foreach (var path in paths)
                                   {
                                       c.IncludeXmlComments(path);
                                   }

                                   c.AddSecurityDefinition(
                                                           "Bearer",
                                                           new OpenApiSecurityScheme
                                                           {
                                                               Type = SecuritySchemeType.OAuth2,
                                                               Flows = new OpenApiOAuthFlows
                                                               {
                                                                   AuthorizationCode = new OpenApiOAuthFlow
                                                                   {
                                                                       Scopes = (scopes ?? Array.Empty<string>()).ToDictionary(x => $"{x}", x => x),
                                                                       AuthorizationUrl = authUrl,
                                                                       TokenUrl = tokenUrl,
                                                                   },
                                                               },
                                                               Scheme = "Bearer",
                                                               In = ParameterLocation.Query,
                                                               BearerFormat = "JWT",
                                                           });
                                   c.AddSecurityRequirement(
                                                            new OpenApiSecurityRequirement
                                                            {
                                                                {
                                                                    new OpenApiSecurityScheme
                                                                    {
                                                                        Reference = new OpenApiReference
                                                                        {
                                                                            Type = ReferenceType.SecurityScheme,
                                                                            Id = "Bearer",
                                                                        },
                                                                        Scheme = "oauth2",
                                                                        Name = "Bearer",
                                                                        In = ParameterLocation.Header,
                                                                    },
                                                                    scopes?.ToArray()
                                                                },
                                                            });
                               });

        services.ConfigureOptions<ConfigureSwaggerOptions>();
    }

    /// <summary>
    ///     Adds Environment Specific (based on AuthorizationAPI:Import:InstanceType) configuration json file to configuration
    ///     builder.
    /// </summary>
    /// <param name="manager">Instance of Configuration Manager.</param>
    /// <remarks>
    ///     Methods assumes the configuration file names in format "usermanagement.import.{0}.json", 0 =>
    ///     AuthorizationAPI:Import:InstanceType.
    /// </remarks>
    public static void AddUserManagementEnvironmentSpecificConfigSource(this ConfigurationManager manager)
    {
        var envName = manager.GetValue<string>("AuthorizationAPI:InstanceType");
        if (string.IsNullOrWhiteSpace(envName))
        {
            return;
        }

        var importFileName = string.Format(ImportFileFormat, envName.ToLowerInvariant());

        if (File.Exists(importFileName))
        {
            manager.AddJsonFile(importFileName, false);
        }
    }
}
