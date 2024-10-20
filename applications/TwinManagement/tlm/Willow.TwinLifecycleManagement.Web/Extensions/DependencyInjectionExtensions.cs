using Authorization.TwinPlatform.Common.Authorization.Handlers;
using Authorization.TwinPlatform.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Willow.Api.Authentication;
using Willow.Api.Common.Extensions;
using Willow.AppContext;
using Willow.AzureDigitalTwins.SDK.Extensions;
using Willow.AzureDigitalTwins.SDK.Option;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Auth.Policies;
using Willow.TwinLifecycleManagement.Web.Helpers.Adapters;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Options;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Extensions;

/// <summary>
/// Dependency Injection Extension.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Register Internal TLM App Services.
    /// </summary>
    /// <param name="services">IServiceCollection Implementation.</param>
    public static void RegisterTLMServices(this IServiceCollection services)
    {
        services.AddScoped<IFileImporterService, FileImporterService>();
        services.AddScoped<IFileExporterService, FileExporterService>();
        services.AddScoped<IGitImporterService, GitImporterService>();
        services.AddScoped<IJobStatusService, JobStatusService>();
        services.AddScoped<IDeletionService, DeletionService>();
        services.AddScoped<IModelsService, ModelsService>();
        services.AddScoped<ITwinsService, TwinsService>();
        services.AddScoped<IEnvService, EnvService>();
        services.AddScoped<IGraphService, GraphService>();
        services.AddScoped<IMappingService, MappingService>();
        services.AddScoped<IUnifiedJobsService, UnifiedJobsService>();
        services.AddScoped<IMtiService, MtiService>();
    }

    /// <summary>
    /// Register Data Quality API Services.
    /// </summary>
    /// <param name="services">IServiceCollection Implementation.</param>
    public static void RegisterDataQualityApiServices(this IServiceCollection services)
    {
        services.AddScoped<IDataQualityService, DataQualityService>();
        services.AddScoped<IDQRuleService, DQRuleService>();
    }

    /// <summary>
    /// Register External Services.
    /// </summary>
    /// <param name="services">IServiceCollection Implementation.</param>
    /// <param name="configuration">IConfiguration instance.</param>
    public static void RegisterDependentServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add ADT API
        services.AddAdtApiClients(configuration.GetSection("TwinsApi").Get<AdtApiClientOption>());

        // Add MTI API
        services.AddHttpClient("MTIAPI", (serviceProvider, httpClient) =>
        {
            httpClient.BaseAddress = new Uri(configuration.GetValue<string>("MTIAPI:BaseAddress"));
            httpClient.Timeout = configuration.GetSection("MTIAPI").GetValue<TimeSpan>("Timeout");
        });
    }

    /// <summary>
    /// Register Authorization related services.
    /// </summary>
    /// <param name="services">IServiceCollection implementation.</param>
    /// <param name="configurationSection">IConfigurationSection instance.</param>
    public static void RegisterAuthorizationServices(this IServiceCollection services, IConfigurationSection configurationSection, IWebHostEnvironment webHostEnvironment)
    {
        // Add TLM Policy Provider
        services.AddSingleton<IAuthorizationPolicyProvider, TLMPolicyProvider>();

        // Register all of the requirements
        services.AddScoped<IAuthorizationHandler, AuthorizePermissionPolicyHandler>();

        // Configure the custom authorization service
        services.AddScoped<IAuthorizationService, TLMAuthorizationService>();
        services.AddUserManagementCoreServices(configurationSection,webHostEnvironment.IsDevelopment());
    }

    /// <summary>
    /// Register Tools.
    /// </summary>
    /// <param name="services">IServiceCollection Implementation.</param>
    public static void RegisterTools(this IServiceCollection services)
    {
        services.AddScoped<IBaseRequestAdapter<UpgradeModelsRepoRequest, GitRepoRequest>, GitRequestAdapter>();
    }

    /// <summary>
    /// Add Options.
    /// </summary>
    /// <param name="services">IServiceCollection.</param>
    /// <param name="configuration">IConfiguration instance.</param>
    public static void AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<AzureAppOptions>().BindConfiguration(AzureAppOptions.Config);
        services.AddOptions<GraphApplicationOptions>().BindConfiguration("GraphAPI");
        services.AddOptions<MtiOptions>().BindConfiguration("MtiOptions");

        services.AddOptions<ApplicationInsightsDto>().BindConfiguration("ApplicationInsights");

        var azureAdSection = configuration.GetSection("AzureAD", AzureADOptions.PopulateDefaults);
        services.Configure<AzureADOptions>(azureAdSection);

        services.Configure<WillowContextOptions>(configuration.GetSection("WillowContext"));

        services.Configure<SpeechServiceOption>(configuration.GetSection("SpeechService"));
    }

    /// <summary>
    /// Configure Swagger Endpoint.
    /// </summary>
    /// <param name="services">IServiceCollection implementation.</param>
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "TwinLifecycleManagement", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    },
            });
        });
    }

    private const string ImportFileFormat = "usermanagement.import.{0}.json";

    /// <summary>
    /// Adds Environment Specific (based on AuthorizationAPI:Import:InstanceType) configuration json file to configuration builder.
    /// </summary>
    /// <param name="manager">Instance of Configuration Manager.</param>
    /// <remarks>
    /// Methods assumes the configuration file names in format "usermanagement.import.{0}.json", 0 => AuthorizationAPI:Import:InstanceType.
    /// </remarks>
    public static void AddUMEnvironmentSpecificConfigSource(this ConfigurationManager manager)
    {
        var envName = manager.GetValue<string>("AuthorizationAPI:InstanceType");
        if (string.IsNullOrWhiteSpace(envName))
        {
            return;
        }

        string importFileName = string.Format(ImportFileFormat, envName.ToLowerInvariant());

        if (File.Exists(importFileName))
        {
            manager.AddJsonFile(importFileName, optional: false);
        }
    }
}
