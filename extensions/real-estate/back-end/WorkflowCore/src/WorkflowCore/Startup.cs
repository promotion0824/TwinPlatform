using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using WorkflowCore.Database;
using WorkflowCore.Entities;
using WorkflowCore.Http;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services;
using WorkflowCore.Extensions.ServiceCollectionExtensions;
using Willow.Scheduler;
using WorkflowCore.Entities.Interceptors;
using WorkflowCore.Services.Background;
using Microsoft.Extensions.Logging;
using WorkflowCore.Services.MappedIntegration;
using Azure.Messaging.ServiceBus;
using System.Diagnostics.Metrics;
using System.Reflection;
using Willow.Common;
using Willow.Notifications;
using Willow.Telemetry.Web;
using Willow.Telemetry;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Willow.HealthChecks;
using WorkflowCore.Infrastructure.Json;
using System.Text.Json;
using Willow.Infrastructure.Database;

namespace WorkflowCore
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

			var appSettings = Configuration.Get<AppSettings>();

			var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2COptions = azureB2CSection.Get<AzureB2CConfiguration>();

            services.AddJwtAuthentication(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"], azureB2COptions, _env);

            var connectionString = Configuration.GetConnectionString("WorkflowDb");
            AddDbContexts(services, connectionString, appSettings);
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            services
                .AddHealthChecks()
                .AddDbContextCheck<WorkflowContext>()
                .AddDependencyService(Configuration, ApiServiceNames.ImageHub, HealthStatus.Degraded)
                .AddDependencyService(Configuration, ApiServiceNames.DirectoryCore, HealthStatus.Unhealthy);

            ConfigureTelemetryService(services);

            // Repositories
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
            services.AddScoped<ITicketTemplateRepository>( (sp)=> new TicketTemplateRepository(sp.GetRequiredService<WorkflowContext>(),
                                                                                               sp.GetRequiredService<IDateTimeService>(),
                                                                                               sp.GetRequiredService<ITicketStatusService>()));
            services.AddScoped<IInspectionRepository, InspectionRepository>();
            services.AddScoped<IUserInspectionRepository, UserInspectionRepository>();
            services.AddScoped<ISchedulerRepository, SchedulerRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
			services.AddScoped<IAuditTrailRepository, AuditTrailRepository>();


			services.AddSingleton<IImagePathHelper, ImagePathHelper>();
            services.AddCachedRestRepository<Guid, Site>(ApiServiceNames.SiteCore, (siteId)=> $"sites/{siteId}", null, 4);
            services.AddSingleton<ISiteService, SiteService>();
            services.AddScoped<IImageHubService, ImageHubService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IWorkflowSequenceNumberService, WorkflowSequenceNumberService>();
            services.AddScoped<IReportersService, ReportersService>();
            services.AddScoped<IAttachmentsServices, AttachmentsServices>();
            services.AddScoped<ICommentsService, CommentsService>();
            services.AddScoped<INotificationReceiverService, NotificationReceiverService>();
            services.AddScoped<IWorkflowNotificationService, WorkflowNotificationService>();
            services.AddScoped<IPushNotificationServer, PushNotificationService>();
            services.AddScoped<IWorkgroupService, WorkgroupService>();
            services.AddScoped<IInspectionService, InspectionService>();
            services.AddScoped<IInspectionRecordGenerator, InspectionRecordGenerator>();
            services.AddScoped<IUserInspectionService, UserInspectionService>();
            services.AddScoped<IInspectionUsageService, InspectionUsageService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddSingleton<IDirectoryApiService>(p => new DirectoryApiService(p.CreateRestApi(ApiServiceNames.DirectoryCore)));
            services.AddSingleton<IMarketPlaceApiService>(p => new MarketPlaceApiService(p.CreateRestApi(ApiServiceNames.MarketPlaceCore)));
            services.AddScoped<IDigitalTwinServiceApi>(p => new DigitalTwinServiceApi(p.CreateRestApi(ApiServiceNames.DigitalTwinCore)));
            services.AddScoped<IInsightServiceApi>(p => new InsightServiceApi(p.CreateRestApi(ApiServiceNames.InsightCore)));
            services.AddScoped<ISiteApiService, SiteApiService>();
            services.AddScoped<IInspectionReportService, InspectionReportService>();
            services.AddScoped<ISiteStatisticsService, SiteStatisticsService>();
			services.AddScoped<IInsightStatisticsService, InsightStatisticsService>();
			services.AddScoped<IAuditTrailService, AuditTrailService>();
			services.Configure<AzureB2CConfiguration>(azureB2CSection);
            services.AddScoped<ITicketStatusTransitionsService, TicketStatusTransitionsService>();
            services.AddScoped<ITicketStatusService, TicketStatusService>();
            services.AddScoped<ITicketSubStatusService, TicketSubStatusService>();
            services.AddScoped<IExternalProfileService, ExternalProfileService>();

            services.AddSingleton<IHttpRequestHeaders>(p => new HttpRequestHeaders());
            services.AddLazyCache();
            services.AddNotificationsService(opt =>
            {
                opt.QueueOrTopicName = appSettings.MessageQueue.CommServiceQueue;
                opt.ServiceBusConnectionString = appSettings.ServiceBusConnectionString;
            });

            services.AddScheduler(Configuration, connectionString, out int scheduleAdvance);

            services.AddScoped<ITicketTemplateService>( (sp)=> new TicketTemplateService( sp.GetRequiredService<IDateTimeService>(),
                                                                                          sp.GetRequiredService<ITicketTemplateRepository>(),
                                                                                          sp.GetRequiredService<IWorkflowService>(),
                                                                                          sp.GetRequiredService<IDigitalTwinServiceApi>(),
                                                                                          scheduleAdvance));
            services.AddQuartzJobs(Configuration.GetSection("BackgroundJobOptions"));
			services.AddScoped<ISessionService, SessionService>();
			services.AddHostedService<AddTwinIdToInspectionHostedService>();
            services.AddHostedService<AddTwinIdToTicketHostedService>();
            services.AddHostedService<AddTwinsToTicketTemplateHostedService>();

            if (appSettings.MappedIntegrationConfiguration?.IsEnabled ?? false)
            {
                services.AddMappedIntegration(Configuration);
            }

        }

        private static void AddDbContexts(IServiceCollection services, string connectionString, AppSettings appSettings)
        {
			services.AddDbContext<WorkflowContext>((sp, o) =>
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
                o.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
				o.AddInterceptors(new AuditTrailInterceptor(sp.GetRequiredService<ISessionService>()));
                // for now enable the ticket events interceptor only if the mapped integration is enabled
                if((appSettings.MappedIntegrationConfiguration?.IsEnabled ?? false)
                && !(appSettings.MappedIntegrationConfiguration?.IsReadOnly ?? false))
                {
                    o.AddInterceptors(new TicketEventsInterceptor(sp.GetRequiredService<ILogger<TicketEventsInterceptor>>(),
                                                                  sp.GetRequiredService<IConfiguration>(),
                                                                  sp.GetRequiredService<ServiceBusClient>(),
                                                                  sp.GetRequiredService<ISessionService>(),
                                                                  sp.GetRequiredService<ISiteApiService>()));
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbUpgradeChecker dbUpgradeChecker)
        {
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = "WorkflowCore app health.",
                HealthCheckName = "WorkflowCore"
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

        private class WebHealthCheckResponse : HealthCheckResponse
        {
            public override Task WriteHealthZResponse(HttpContext context, HealthReport healthReport)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                var slimResult = new HealthCheckDto("WorkflowCore", "App health", healthReport);
                string json = JsonSerializer.Serialize(slimResult, JsonSerializerExtensions.DefaultOptions);
                return context.Response.WriteAsync(json);
            }
        }
    }
}
