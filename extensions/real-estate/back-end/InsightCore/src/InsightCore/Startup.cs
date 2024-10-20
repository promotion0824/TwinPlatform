using InsightCore.Constants;
using InsightCore.Database;
using InsightCore.Entities;
using InsightCore.Infrastructure.Configuration;
using InsightCore.Services;
using InsightCore.Services.Background;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Reflection;
using Willow.Api.Client;
using Willow.HealthChecks;
using Willow.Infrastructure.Database;
using Willow.Notifications;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace InsightCore
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
            services.Configure<AppSettings>(Configuration);

            var appSettings = Configuration.Get<AppSettings>();

            var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2COptions = azureB2CSection.Get<AzureB2CConfiguration>();
            services.Configure<InspectionOptions>(Configuration.GetSection(nameof(InspectionOptions)));
            services.AddJwtAuthentication(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"], azureB2COptions, _env);

            var connectionString = Configuration.GetConnectionString("InsightDb");
            AddDbContexts(services, connectionString);
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            services
                .AddHealthChecks()
                .AddDbContextCheck<InsightDbContext>()
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.AddScoped<IInsightService, InsightService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IInsightRepository, InsightRepository>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<IInsightStatisticsService, InsightStatisticsService>();
            services.AddScoped<IDigitalTwinServiceApi>(provider => new DigitalTwinServiceApi(new RestApi(provider.GetRequiredService<IHttpClientFactory>(), ApiServiceNames.DigitalTwinCore)));
            services.AddScoped<IWorkflowServiceApi>(provider => new WorkflowServiceApi(new RestApi(provider.GetRequiredService<IHttpClientFactory>(), ApiServiceNames.WorkflowCore)));
            services.Configure<AzureB2CConfiguration>(azureB2CSection);
            services.AddHostedService<AddTwinNameToInsightHostedService>();
            services.AddNotificationsService(opt => {
                opt.QueueOrTopicName = appSettings.ServiceBusOptions?.NotificationTopicName;
                opt.ServiceBusConnectionString = appSettings.ServiceBusOptions?.ConnectionString;
            });
        }

        private static void AddDbContexts(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<InsightDbContext>((sp, o) =>
            {
                o.UseSqlServer(
                   connectionString,
                   opt =>
                       // Don't call EnableRetryLogic, the following call does the same job
                       opt.ExecutionStrategy(c => new CustomAzureSqlExecutionStrategy(c,
                           logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()))
                           .UseAzureSqlDefaults()
                           .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                   );
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbUpgradeChecker dbUpgradeChecker)
        {
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = "InsightCore app health.",
                HealthCheckName = "InsightCore"
            });

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWillowContext(Configuration);
            app.UseApiServices(Configuration, env);

            dbUpgradeChecker.EnsureDatabaseUpToDate(env);
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
}
