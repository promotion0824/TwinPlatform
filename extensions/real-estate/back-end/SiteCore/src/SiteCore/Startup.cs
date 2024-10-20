using System;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SiteCore.Database;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Import;
using SiteCore.Import.Services;
using SiteCore.Options;
using SiteCore.Services;
using SiteCore.Services.Background;
using SiteCore.Services.DigitalTwinCore;
using SiteCore.Services.ImageHub;
using Willow.HealthChecks;
using Willow.Infrastructure;
using Willow.Infrastructure.Database;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace SiteCore
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
            services.AddMemoryCache();

            var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2COptions = azureB2CSection.Get<AzureADB2CConfiguration>();

            services.AddJwtAuthentication(
                Configuration["Auth0:Domain"],
                Configuration["Auth0:Audience"],
                azureB2COptions,
                _env
            );

            AddDbContexts(services);
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            services.AddHealthChecks().AddDbContextCheck<SiteDbContext>().AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.Configure<WidgetBackgroundJobOptions>(
                Configuration.GetSection(nameof(WidgetBackgroundJobOptions))
            );
            services.Configure<FloorModuleOptions>(
                Configuration.GetSection(nameof(FloorModuleOptions))
            );
            services.Configure<ForgeOptions>(Configuration.GetSection(nameof(ForgeOptions)));
            services.Configure<AzureADB2CConfiguration>(azureB2CSection);
            services.AddSingleton<IImagePathHelper, ImagePathHelper>();
            services.AddScoped<IImageHubService, ImageHubService>();
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IFloorService, FloorService>();
            services.AddScoped<ILayerGroupsService, LayerGroupsService>();
            services.AddScoped<IModulesService, ModulesService>();
            services.AddScoped<IModuleTypesService, ModuleTypesService>();
            services.AddScoped<IModuleGroupsService, ModuleGroupsService>();
            services.AddScoped<IWidgetService, WidgetService>();
            services.AddScoped<IMetricsService, MetricsService>();
            services.AddScoped<ISiteExtendService, SiteExtendService>();
            services.AddTransient<IAutodeskForgeTokenProvider, AutodeskForgeTokenProvider>();
            services.AddTransient<IImportService, ZonesImportService>();
            services.AddTransient<ISiteCoreDataImportService, SiteCoreDataImportService>();
            services.AddScoped<IDigitalTwinCoreApiService, DigitalTwinCoreApiService>();
            services.AddScoped<
                ISitePreferencesScopePopulateService,
                SitePreferencesScopePopulateService
            >();

            services.AddLazyCache();
        }

        private void AddDbContexts(IServiceCollection services)
        {
            services.AddDbContext<SiteDbContext>(SetOptions);
            return;

            void SetOptions(IServiceProvider sp, DbContextOptionsBuilder o)
            {
                var connectionString = Configuration.GetConnectionString("SiteDb");
                o.UseSqlServer(
                    connectionString,
                     opt =>
                         // Don't call EnableRetryLogic, the following call does the same job
                         opt.ExecutionStrategy(c => new CustomAzureSqlExecutionStrategy(c,
                             logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()))
                             .UseAzureSqlDefaults()
                             .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                );
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDbUpgradeChecker dbUpgradeChecker,
            ILogger<Startup> logger
        )
        {
            app.UseWillowHealthChecks(
                new HealthCheckResponse()
                {
                    HealthCheckDescription = "SiteCore app health.",
                    HealthCheckName = "SiteCore"
                }
            );

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWillowContext(Configuration);
            app.UseApiServices(Configuration, env);

        }

        private void ConfigureTelemetryService(IServiceCollection services)
        {
            if (
                string.IsNullOrWhiteSpace(
                    Configuration.GetValue<string>("ApplicationInsights:ConnectionString")
                )
            )
                return;

            services.AddWillowContext(Configuration);

            var meterName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
            var meterVersion =
                Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown";
            var meter = new Meter(meterName, meterVersion);

            services.AddSingleton(meter);

            var metricsAttributesHelper = new MetricsAttributesHelper(Configuration);
            services.AddSingleton(metricsAttributesHelper);
        }

        public static Task WriteHealthReportResponse(HttpContext context, HealthReport healthReport)
        {
            var healthCheckDto = new HealthCheckDto(
                Assembly.GetEntryAssembly().GetName().Name,
                "Health Report",
                healthReport
            );

            var healthCheckDtoJson = JsonSerializerExtensions.Serialize(healthCheckDto);

            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(healthCheckDtoJson);
        }
    }
}
