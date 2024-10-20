using System;
using System.Diagnostics.Metrics;
using System.Reflection;
using Authorization.TwinPlatform.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationCore.Database;
using NotificationCore.Entities;
using NotificationCore.Infrastructure.Configuration;
using NotificationCore.Infrastructure.Extensions;
using NotificationCore.MessageHandlers;
using NotificationCore.Repositories;
using NotificationCore.Services;
using NotificationCore.TriggerFilterRules;
using Willow.HealthChecks;
using Willow.ServiceBus;
using Willow.ServiceBus.HostedServices;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace NotificationCore;

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
        var azureB2COptions = azureB2CSection.Get<AzureB2CConfiguration>();
        services.AddJwtAuthentication(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"], azureB2COptions, _env);
        services.AddServiceBus(Configuration.GetSection("ServiceBus"));
        var connectionString = Configuration.GetConnectionString("NotificationCoreDb");
        services.Configure<NotificationTopic>(
            Configuration.GetSection("ServiceBus:Topics:NotificationTopic"));
        AddDbContexts(services, connectionString);
        services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();
        services
            .AddHealthChecks()
            .AddDbContextCheck<NotificationDbContext>()
            .AddAssemblyVersion();

        ConfigureTelemetryService(services);

        services.AddScoped<ITopicMessageHandler, NotificationMessageHandler>();
        services.AddScoped<INotificationTriggerService, NotificationTriggerService>();
        services.AddScoped<INotificationTriggerRepository, NotificationTriggerRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationsRepository, NotificationsRepository>();

        services.Configure<AzureB2CConfiguration>(azureB2CSection);
        services.AddHostedService<MessageListenerBackgroundService>();


        // Add the trigger filter rules
        services.AddScoped<ITriggerFilterRule, TwinFilter>();
        services.AddScoped<ITriggerFilterRule, TwinCategoryFilter>();
        services.AddScoped<ITriggerFilterRule, SkillFilter>();
        services.AddScoped<ITriggerFilterRule, SkillCategoryFilter>();

        // Add the user management core services
        services.AddUserManagementCoreServices(Configuration.GetSection("AuthorizationAPI"));


    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbUpgradeChecker dbUpgradeChecker)
    {
        app.UseWillowHealthChecks(new HealthCheckResponse()
        {
            HealthCheckDescription = "NotificationCore app health.",
            HealthCheckName = "NotificationCore"
        });

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWillowContext(Configuration);
        app.UseApiServices(Configuration, env);
        dbUpgradeChecker.EnsureDatabaseUpToDate(env);
    }

    private void AddDbContexts(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NotificationDbContext>(SetOptions);
        return;

        void SetOptions(DbContextOptionsBuilder o)
        {
            const int couldNotConnect = 40;
            o.UseSqlServer(connectionString, builder => builder.EnableRetryOnFailure(6, TimeSpan.FromSeconds(30), [couldNotConnect]));
        }
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
