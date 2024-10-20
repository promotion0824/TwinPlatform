using System;
using System.Runtime.CompilerServices;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Configs;
using DigitalTwinCore.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DigitalTwinCore.Database;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Features.DirectoryCore;
using DigitalTwinCore.Features.RelationshipMap.Extensions;
using DigitalTwinCore.Features.SiteAdmin;
using DigitalTwinCore.Infrastructure.Configuration;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DigitalTwinCore.Services.Cacheless;
using DigitalTwinCore.Services.Adx;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Willow.Api.AzureStorage;
using Willow.Common;
using DigitalTwinCore.Features.TwinsSearch.Services;
using Microsoft.AspNetCore.Http.Features;
using DigitalTwinCore.Features.GeometryViewer;
using DigitalTwinCore.Repositories;
using Willow.Telemetry.Web;
using System.Reflection;
using System.Diagnostics.Metrics;
using Willow.Telemetry;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using System.Text.Json;
using Willow.Infrastructure;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Willow.Infrastructure.Database;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("DigitalTwinCore.Test")]
namespace DigitalTwinCore
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
            services.AddApiServices(Configuration, _env);
            services.AddRetryPipelines();

            var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2COptions = azureB2CSection.Get<AzureB2CConfiguration>();

            var hasScopeTreeUpdater =
                Configuration.GetValue<bool>("SingleTenantOptions:IsSingleTenant")
                && !Configuration.GetValue<bool>("DisableScopeTreeUpdater");

            if (hasScopeTreeUpdater)
            {
                services.AddSingleton<ScopeTreeUpdaterService>();
                services.AddHostedService<ScopeTreeUpdaterService>(p => p.GetRequiredService<ScopeTreeUpdaterService>());
            }

            services.AddJwtAuthentication(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"], azureB2COptions, _env);

            var connectionString = Configuration.GetConnectionString("DigitalTwinDb");
            AddDbContexts(services, connectionString);
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();
            services.AddScoped<IAdxDatabaseInitializer, AdxDatabaseInitializer>();

            var healthChecks = services.AddHealthChecks();

            if (hasScopeTreeUpdater)
            {
                healthChecks = healthChecks.AddCheck<ScopeTreeUpdaterHealthCheck>("ScopeTreeUpdater");
            }

            healthChecks
                .AddDbContextCheck<DigitalTwinDbContext>()
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.Configure<SingleTenantOptions>(Configuration.GetSection("SingleTenantOptions"));
            services.Configure<AzureDigitalTwinsSettings>(Configuration.GetSection("AzureDigitalTwins"));
            services.Configure<AzureB2CConfiguration>(azureB2CSection);
            services.Configure<BlobStorageConfig>(Configuration.GetSection("Azure:BlobStorage"));
            services.Configure<AzureDataExplorerSettings>(Configuration.GetSection("ADX"));

            services.AddHttpContextAccessor();

            services.AddAzureBlobStorage<DocumentsController>(this.Configuration);

            services.TryAddScoped<IDigitalTwinServiceProvider, DigitalTwinServiceProvider>();
            services.AddScoped<ISiteAdtSettingsProvider, SiteAdtSettingsProvider>();
            services.AddScoped<ITokenService>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<AzureDigitalTwinsSettings>>();
                var cache = serviceProvider.GetRequiredService<IMemoryCache>();
                return config.Value.UsesAzureIdentity ? new AzureIdentityTokenService(cache) : new TokenService(cache);
            });
            services.AddScoped<IAdtApiService, AdtApiService>();
            services.AddScoped<IDigitalTwinService, CachelessAdtService>();
            services.AddScoped<IAssetService, CachelessAssetService>();
            services.AddScoped<ITagMapperService, TagMapperService>();
            services.AddScoped<IDocumentsService>(p =>
            {
                var config = p.GetRequiredService<IOptions<BlobStorageConfig>>();

                return new DocumentsService
                (
                    p.GetService<IContentTypeProvider>(),
                    p.GetService<IHashCreator>(),
                    p.GetService<IBlobStore>(),
                    p.GetService<IDigitalTwinServiceProvider>(),
                    p.GetService<IGuidWrapper>(),
                    $"https://{config.Value.AccountName}.blob.core.windows.net/{config.Value.ContainerName}"
                );
            });

            services.TryAddScoped<IHashCreator, Md5HashCreator>();
            services.TryAddScoped<IContentTypeProvider, FileExtensionContentTypeProvider>();
            services.TryAddScoped<IGuidWrapper, GuidWrapper>();
            services.AddScoped<IAdxHelper, AdxHelper>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IGeometryViewerService, GeometryViewerService>();
            services.AddScoped<ISiteSettingsService, SiteSettingsService>();
            services.AddScoped<IDirectoryCoreClient>(provider => new DirectoryCoreClient(provider.CreateRestApi(ApiServiceNames.DirectoryCore)));
            services.AddScoped<IRelationshipMapService, RelationshipMapService>();
            services.AddScoped<ITenantsService, TenantsService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.SetupRelationshipMapFeature(Configuration);

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
            });
        }

        private static void AddDbContexts(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<DigitalTwinDbContext>(SetOptions, ServiceLifetime.Transient);
            return;

            void SetOptions(IServiceProvider sp, DbContextOptionsBuilder o)
            {
                o.UseSqlServer(connectionString,
                    opt =>
                        // Don't call EnableRetryLogic, the following call does the same job
                        opt.ExecutionStrategy(c => new CustomAzureSqlExecutionStrategy(c,
                            logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()))
                            .UseAzureSqlDefaults()
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    );
            }
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDbUpgradeChecker dbUpgradeChecker,
            IAdxDatabaseInitializer adxDatabaseInitializer,
            IApiVersionDescriptionProvider provider)
        {
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = "DigitalTwinCore app health.",
                HealthCheckName = "DigitalTwinCore"
            });
            app.UseWillowContext(Configuration);
            app.UseApiServices(Configuration, env, provider);
            dbUpgradeChecker.EnsureDatabaseUpToDate(env);
            adxDatabaseInitializer.EnsureDatabaseObjectsExist();
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

        public static Task WriteHealthReportResponse(HttpContext context, HealthReport healthReport)
        {
            var healthCheckDto = new HealthCheckDto(Assembly.GetEntryAssembly().GetName().Name, "Health Report", healthReport);

            var healthCheckDtoJson = JsonSerializer.Serialize(healthCheckDto, JsonSerializerExtensions.DefaultOptions);

            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(healthCheckDtoJson);
        }
    }
}
