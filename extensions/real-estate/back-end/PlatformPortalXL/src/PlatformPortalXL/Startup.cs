using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using PlatformPortalXL.Http;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.Services.LiveDataApi;
using PlatformPortalXL.Services.MarketPlaceApi;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.Services.Forge;
using PlatformPortalXL.Services.PowerBI;
using PlatformPortalXL.Services.Assets;
using PlatformPortalXL.Services.LiveData;
using PlatformPortalXL.Helpers;
using Willow.Api.Authorization;
using Willow.Api.AzureStorage;
using Willow.Data;
using Willow.Data.Rest;
using Willow.Management;
using Willow.PlatformPortalXL;
using Willow.Platform.Localization;
using Willow.Platform.Users;
using Willow.Workflow;
using PlatformPortalXL.Services.ArcGis;
using PlatformPortalXL.Services.Sigma;
using Willow.Proxy;
using PlatformPortalXL.Services.Twins;
using PlatformPortalXL.ServicesApi.InsightApi;
using Microsoft.AspNetCore.StaticFiles;
using Willow.MessageDispatch;
using PlatformPortalXL.Services.GeometryViewer;
using PlatformPortalXL.ServicesApi.GeometryViewerApi;
using System.Threading;
using Willow.TimedDispatch;
using PlatformPortalXL.Features.Insights;
using PlatformPortalXL.Filters.ArcGis;
using PlatformPortalXL.Services.CognitiveSearch;
using Authorization.TwinPlatform.Common.Extensions;
using Willow.Common.Middlewares;
using PlatformPortalXL.ServicesApi.ZendeskApi;
using PlatformPortalXL.Infrastructure.SingleTenant;
using PlatformPortalXL.Services.ContactUs;
using PlatformPortalXL.Infrastructure.AppSettingOptions;
using System.Diagnostics.Metrics;
using System.Reflection;
using Willow.Telemetry.Web;
using Willow.Telemetry;
using Willow.HealthChecks;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Azure.Services.AppAuthentication;
using Willow.Infrastructure;
using PlatformPortalXL.Auth.Extensions;
using PlatformPortalXL.Infrastructure;
using PlatformPortalXL.Services.CognitiveSearch.Extensions;
using PlatformPortalXL.ServicesApi.WeatherbitApi;
using Willow.Notifications;
using Willow.Notifications.Interfaces;
using Willow.Copilot.ProxyAPI.Extensions;
using Willow.Copilot.ProxyAPI;
using PlatformPortalXL.Services.Copilot;
using PlatformPortalXL.Configs;
using PlatformPortalXL.Infrastructure.Security;
using PlatformPortalXL.Services.SiteWeatherCache;
using PlatformPortalXL.ServicesApi.NotificationTriggerApi;
using Willow.Data.Configs;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.ServicesApi.NotificationApi;

namespace PlatformPortalXL
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiServices(Configuration, _env)
                    .AddRetryPipelines()
                    .AddMemoryCache();

            ConfigureDataProtectionService(services);
            ConfigureTelemetryService(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            services.Configure<AppSettings>(Configuration);
            string auth0domain = $"https://{Configuration["Auth0:Domain"]}/";
            string auth0audience = Configuration["Auth0:Audience"];
            var b2cConfig = new B2CConfig
            {
                Authority = Configuration["B2C:Authority"],
                ClientId  = Configuration["B2C:ClientId"],
                TenantId  = Configuration["B2C:TenantId"]
            };

            services
                .AddWillowAuthentication(b2cConfig)
                .AddJwtBearer(options =>
                {
                    options.Authority = auth0domain;
                    options.Audience = auth0audience;
                    options.RequireHttpsMetadata = false;
                });

            services.AddWillowAuthorization(options =>
            {
                options.InvokeHandlersAfterFailure = false; // Required to enforce ANDs on policies
                options.AddAuthorizationRequirements();
            });

            services.AddAuthorizationRequirements()
                    .AddAuthorizationHandlers()
                    .AddAccessControlServices()
                    .AddAncestralTwinLookupServices()
                    .AddTwinSearchServices(Configuration)
                    .AddSupportForLegacyAuthPermissionsAgainstUserManagement();

            services.AddScoped<ITwinTreeAuthEvaluator, TwinTreeAuthEvaluator>();

            // Registers the IUserAuthorizationService and IImportService services.
            services.AddUserManagementCoreServices(Configuration.GetSection("AuthorizationAPI"));

            services.Configure<ArcGisOptions>(Configuration.GetSection(nameof(ArcGisOptions)));
            services.Configure<ZendeskOptions>(Configuration.GetSection(nameof(ZendeskOptions)));
            services.Configure<ForgeOptions>(Configuration.GetSection(nameof(ForgeOptions)));
            services.Configure<PowerBIOptions>(Configuration.GetSection(nameof(PowerBIOptions)));
            services.Configure<BlobStorageConfig>(Configuration.GetSection("Azure:BlobStorage"));
            services.Configure<SingleTenantOptions>(Configuration.GetSection(nameof(SingleTenantOptions)));
            services.Configure<AzureCognitiveSearchOptions>(Configuration.GetSection(nameof(AzureCognitiveSearchOptions)));
            services.Configure<DirectoryApiServiceOptions>(Configuration.GetSection(nameof(DirectoryApiServiceOptions)));
            services.Configure<CustomerInstanceConfigurationOptions>(Configuration.GetSection("WillowContext:CustomerInstanceConfiguration"));
            services.Configure<StaleCacheOptions>(Configuration.GetSection(nameof(StaleCacheOptions)));

            services.AddScoped<IAssetLocalizerFactory>( p=>
            {
                var config = new BlobStorageConfig { AccountName   = Configuration["ContentStorage:AccountName"],
                                                     ContainerName = Configuration["ContentStorage:ContainerName"],
                                                     AccountKey    = Configuration["ContentStorage:AccountKey"] }; // Key should only be used for local development

                return new AssetLocalizerFactory(p.GetRequiredService<IMemoryCache>(),
                                                 p.CreateBlobStore<AssetLocalizerFactory>(config, "assets", true),
                                                 p.GetRequiredService<ILogger<AssetLocalizerFactory>>());
            });

            services.AddStatisticsService();
            services.AddNotificationsService(opt =>
            {
                opt.QueueOrTopicName = Configuration["CommServiceQueue"];
                opt.ServiceBusConnectionString = Configuration["ServiceBusConnectionString"];
            });

            services.AddMessageService<IGeometryViewerMessagingService>(Configuration, "GeometryViewerQueue",
                (messageQueue, p) => new GeometryViewerMessagingService(messageQueue));

            services.AddScoped<IGeometryViewerApiService>(p => new GeometryViewerApiService(p.CreateRestApi(ApiServiceNames.DigitalTwinCore)));
            services.AddScoped<IGeometryViewerService, ForgeService>();
            services.AddScoped<IMessageDispatchHandler, GeometryViewerDispatchHandler>();
            services.AddScoped<ITimedDispatchHandler, GeometryViewerDispatchHandler>();

            services.AddSingleton<IHostedMessageDispatch>(p =>
            {
                return new ServiceBusMessageDispatch(
                    Configuration["ServiceBusConnectionString"],
                    Configuration["GeometryViewerQueue"],
                    p.GetRequiredService<IMessageDispatchHandler>());
            });
            services.AddHostedService(p => p.GetRequiredService<IHostedMessageDispatch>());
            services.AddHostedService(p => new TimedDispatch(p.GetRequiredService<ITimedDispatchHandler>(), TimeSpan.Zero, Timeout.InfiniteTimeSpan));
            services.AddHostedService<SiteWeatherCacheHostedService>();

            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddSingleton<ICopilotService, CopilotService>();
            services.AddSingleton<IImageUrlHelper, ImageUrlHelper>();
            services.AddSingleton<IManagementAccessService>( p=> new ManagementAccessService(p.GetRequiredService<IDirectoryApiService>(), p.GetUserRepository()));
            services.AddScoped<IForgeApi, ForgeApi>();
            services.AddUserService();
            services.AddScoped<IAccessControlService, AccessControlService>();
            services.AddSingleton<IMarketPlaceApiService>( p=> new MarketPlaceApiService(p.CreateRestApi(ApiServiceNames.MarketPlaceCore)));
            services.AddScoped<IAppManagementService, AppManagementService>();
            services.AddScoped<IDirectoryApiService, DirectoryApiService>();
            services.AddScoped<IAccountRepository>( p=> p.GetRequiredService<IDirectoryApiService>());
            services.AddScoped<ISiteApiService, SiteApiService>();
            services.AddWorkflowService();
            services.AddSingleton<IInsightApiService>( p=> new InsightApiService(p.CreateRestApi(ApiServiceNames.InsightCore)));
            services.AddScoped<IFloorsApiService, FloorsApiService>();
            services.AddScoped<IConnectorApiService, ConnectorApiService>();
            services.AddScoped<ILiveDataApiService, LiveDataApiService>();
            services.AddScoped<ILayerGroupsApiService, LayerGroupsApiService>();
            services.AddScoped<IModuleTypesService, ModuleTypesService>();
            services.AddScoped<IModuleGroupsService, ModuleGroupsService>();
            services.AddScoped<IConnectorPointsService, ConnectorPointsService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IContentTypeProvider, FileExtensionContentTypeProvider>();
            services.AddScoped<IFloorManagementService, FloorManagementService>();
            services.AddScoped<IAssetApiService, AssetApiService>();
            services.AddScoped<ITwinStatisticsService, TwinStatisticsService>();
            services.AddSingleton<IWidgetApiService>(p => new WidgetApiService(p.CreateRestApi(ApiServiceNames.SiteCore)));
            services.AddScoped<IFloorsService, FloorsService>();
            services.AddScoped<IPowerBIService, PowerBIService>();
            services.AddSingleton<IPowerBIClientFactory, PowerBIClientFactory>();
            services.AddScoped<IForgeService, ForgeService>();
            services.AddScoped<IArcGisService, ArcGisService>();
            services.AddScoped<ITwinService, TwinService>();
            services.AddScoped<IPersonManagementService, PersonManagementService>();
            services.AddScoped<IAuth0Service, Auth0Service>();
            services.AddScoped<INotificationTriggerService, NotificationTriggerService>();
            services.AddScoped<INotificationsService, NotificationsService>();
            services.AddSingleton<INotificationTriggerApiService>(p => new NotificationTriggerApiService(p.CreateRestApi(ApiServiceNames.NotificationCore)));
            services.AddScoped<IAuthFeatureFlagService, AuthFeatureFlagService>();
            services.AddSingleton<INotificationApiService>(p => new NotificationApiService(p.CreateRestApi(ApiServiceNames.NotificationCore)));
            services.AddScoped<ConnectorLiveDataService>();
            services.AddScoped<IDigitalTwinAssetService, DigitalTwinAssetService>();
            services.AddScoped<DigitalTwinLiveDataService>();
            services.AddSingleton<IManagedUserRequestValidator, ManagedUserRequestValidator>();
            services.AddScoped<IInspectionService, InspectionService>();
            services.AddScoped<IPortfolioDashboardService, PortfolioDashboardService>();
            services.AddSingleton<IWeatherbitApiService>(p =>
                new WeatherbitApiService(p.CreateRestApi(ApiServiceNames.Weatherbit), Configuration["WeatherbitApiUrl"], Configuration["WeatherbitApiKey"], p.GetRequiredService<IResiliencePipelineService>()));
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<SiteService>();
            services.AddScoped<ISiteService, SiteServiceWithAuthFiltering>();
            services.AddScoped<IManagementService>( p=>
            {
                return new ManagementService(p.GetRequiredService<IManagementAccessService>(),
                                             p.GetRequiredService<IDirectoryApiService>(),
                                             p.GetRequiredService<ISiteApiService>(),
                                             p.GetRequiredService<IManagedUserRequestValidator>(),
                                             p.GetRequiredService<IAuthFeatureFlagService>(),
                                             p.GetRequiredService<INotificationService>(),
                                             this.Configuration["CommandPortalBaseUrl"],
                                             p.GetRequiredService<IImageUrlHelper>(),
                                             p.GetRequiredService<IAccessControlService>());
            });
            services.AddScoped<IConnectorService, ConnectorService>();
            services.AddScoped<IScanValidationService, ScanValidationService>();
            services.AddScoped<ISigmaService, SigmaService>();
            services.AddSingleton<IHttpRequestHeaders, HttpRequestHeaders>();
            services.AddKPIService(this.Configuration);
            services.AddScoped<IControllerHelper, ControllerHelper>();
            services.AddDigitalApiService(this.Configuration);
            services.AddScoped<IDigitalTwinService, DigitalTwinService>();
            services.AddHttpProxy();
			services.AddScoped<IInsightService, InsightService>();
			services.AddScoped<CustomerValidationFilter>();
			services.AddScoped<ArcGisValidationFilter>();
            services.AddScoped<IZendeskApiService, ZendeskApiService>();
            services.AddScoped<IContactUsService, ContactUsService>();

            services.ConfigureCopilotClients(Configuration.GetSection("Copilot").Get<CopilotSettings>());
            services.AddHealthChecks()
                .AddDependencyService(Configuration, ApiServiceNames.DirectoryCore, HealthStatus.Unhealthy)
                .AddDependencyService(Configuration, ApiServiceNames.InsightCore, HealthStatus.Unhealthy)
                .AddDependencyService(Configuration, ApiServiceNames.WorkflowCore, HealthStatus.Unhealthy)
                .AddDependencyService(Configuration, ApiServiceNames.DigitalTwinCore, HealthStatus.Unhealthy)
                .AddDependencyService(Configuration, ApiServiceNames.SiteCore, HealthStatus.Unhealthy)
                .AddAssemblyVersion()
                .AddSiteWeatherCache();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider provider)
        {
            app.ConfigureExceptionHandler();

            app.UseWillowHealthChecks(new HealthCheckResponse
            {
                HealthCheckDescription = "PortalXL app health",
                HealthCheckName = "PPXL"
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWillowContext(Configuration);
            app.UseMiddleware<UserEmailLoggingMiddleware>();
            app.UseApiServices(Configuration, env, provider);
        }

        private void ConfigureDataProtectionService(IServiceCollection services)
        {
            var keyVaultName = Configuration.GetValue<string>("DataProtection:KeyVaultName");
            if (string.IsNullOrEmpty(keyVaultName))
            {
                return;
            }

            // Note that dbup is not used in this project, however the library is required as it transitively includes AzureServiceTokenProvider.
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

            var keyBundle = keyVaultClient.GetKeyAsync($"https://{keyVaultName}.vault.azure.net/", "DataProtectionKey")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            var keyIdentifier = keyBundle.KeyIdentifier.Identifier;

            var storageCredentials = new StorageCredentials(
                Configuration.GetValue<string>("DataProtection:StorageAccountName"),
                Configuration.GetValue<string>("DataProtection:StorageKey"));
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("dataprotectionkeys");

            services.AddDataProtection(options => { options.ApplicationDiscriminator = "willow"; })
                .PersistKeysToAzureBlobStorage(cloudBlobContainer, "willowplatform")
                .SetApplicationName("WillowPlatform")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
                .ProtectKeysWithAzureKeyVault(keyVaultClient, keyIdentifier);
        }

        public static Task WriteHealthReportResponse(HttpContext context, HealthReport healthReport)
        {
            var healthCheckDto = new HealthCheckDto(Assembly.GetEntryAssembly().GetName().Name, "Health Report", healthReport);

            var healthCheckDtoJson = JsonSerializer.Serialize(healthCheckDto, JsonSerializerExtensions.DefaultOptions);

            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(healthCheckDtoJson);
        }

        private void ConfigureTelemetryService(IServiceCollection services)
        {
            if (string.IsNullOrWhiteSpace(Configuration.GetValue<string>("ApplicationInsights:ConnectionString")))
                return;

            services.AddWillowContext(Configuration);

            var meterName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
            var meterVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown";
            var meter = new Meter(meterName, meterVersion);

            services.AddSingleton(meter);

            var metricsAttributesHelper = new MetricsAttributesHelper(Configuration);
            services.AddSingleton(metricsAttributesHelper);
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUserService(this IServiceCollection services)
        {
            services.AddScoped<IUserService>( (p)=>
            {
                var directoryApi = p.CreateRestApi(ApiServiceNames.DirectoryCore);
                var workflowApi  = p.CreateRestApi(ApiServiceNames.WorkflowCore);
                var cache        = p.GetRequiredService<IMemoryCache>();

                return new UserService(new CachedRepository<Guid, User>(GetUserRepository(p), cache, TimeSpan.FromHours(1), "User_"),
                                       new CachedRepository<SiteObjectIdentifier, Workgroup>( new RestRepositoryReader<SiteObjectIdentifier, Workgroup>
                                                                                              (
                                                                                                workflowApi,
                                                                                                (id)=> $"sites/{id.SiteId}/workgroups/{id.Id}",
                                                                                                null
                                                                                              ),
                                                                                              cache,
                                                                                              TimeSpan.FromHours(1),
                                                                                              "Workgroup_"),
                                       directoryApi);
            });

            return services;
        }

        public static IReadRepository<Guid, User> GetUserRepository(this IServiceProvider p)
        {
            var directoryApi = p.CreateRestApi(ApiServiceNames.DirectoryCore);

            return new RestRepositoryReader<Guid, User>(directoryApi, (id)=> $"users/{id}", null);
        }

        public static IServiceCollection AddWorkflowService(this IServiceCollection services)
        {
            services.AddSingleton<IWorkflowApiService>( (p)=>
            {
                return new WorkflowApiService(p.CreateRestApi(ApiServiceNames.WorkflowCore));
            });

            return services;
        }
    }
}
