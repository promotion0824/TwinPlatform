using Authorization.Migrator;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Permission.Api.Constants;
using Authorization.TwinPlatform.Permission.Api.Extensions;
using Authorization.TwinPlatform.Permission.Api.Handlers;
using Authorization.TwinPlatform.Permission.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.Metrics;
using Willow.HealthChecks;

namespace Authorization.TwinPlatform.Permission.Api;

/// <summary>
/// Startup class contains method for App Startup
/// </summary>
public static class Startup
{
    private static readonly CancellationTokenSource readinessCancellationTokenSource = new();
    /// <summary>
    /// Method to configure application logging
    /// </summary>
    /// <param name="builder">Instance of WebApplicationBuilder</param>
    public static void ConfigureLogging(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        if (!builder.Environment.IsDevelopment())
        {
            builder.Logging.AddConsole();
        }

        // Add Telemetry
        builder.Services.AddTelemetry(builder.Configuration);
    }

    /// <summary>
    /// Method to inject services in to the DI Container
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configuration">The app configuration.</param>
    /// <param name="env">Web Host Environment</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Add Memory Cache
        services.AddMemoryCache();

        //Keeping this temporary. Will remove if none of the scoped services has dependency on HttpContext
        services.AddHttpContextAccessor();

        //Add Authentication
        services.AddAdAuthentication(configuration);

        //Add Authorization
        services.AddAuthorization((authOptions) =>
        {
            authOptions.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build();
        });
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

        //Configure Options
        services.ConfigureOptions(configuration);

        //Add TwinPlatform Services
        services.AddTwinPlatformPermissionService(configuration, env);

        services.AddManagerServices();

        services.AddMicrosoftGraphAPIWithHostedServices(configuration.GetSection("GraphAPIApps"),
            configuration.GetSection("HostedServices"));

        //Adds Problem details services
        services.AddProblemDetails();

        services.AddCors();

        // Add Authorization Health Checks
        services.AddAuthorizationHealthChecks((healthBuilder) =>
        {
            healthBuilder.AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
            .AddCheck("readyz", () =>
            {
                readinessCancellationTokenSource.Token.ThrowIfCancellationRequested();
                return HealthCheckResult.Healthy("System is ready.");
            }, tags: ["readyz"]);
        });

    }

    /// <summary>
    /// Configure Options class
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configuration">IConfiguration instance</param>
    /// <exception cref="NullReferenceException">If missing required configuration </exception>
    private static void ConfigureOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var adminOptionSection = configuration.GetRequiredSection(AdminOption.OptionName);

        services.Configure<AdminOption>(adminOptionSection);
        services.Configure<PermissionCacheOption>(configuration.GetRequiredSection(PermissionCacheOption.OptionName));
    }

    /// <summary>
    /// Method to configure application request pipeline
    /// </summary>
    /// <param name="app">Web application</param>
    public static void Configure(WebApplication app)
    {

        app.UseSwagger();
        app.UseSwaggerUI();


        app.UseHttpsRedirection();

        app.UseRouting();

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthentication();
        app.UseAuthorization();

        app.Lifetime.ApplicationStopping.Register(readinessCancellationTokenSource.Cancel);
        app.Lifetime.ApplicationStopped.Register(readinessCancellationTokenSource.Cancel);
        var applicationName = app.Configuration.GetValue<string>("ApplicationInsights:CloudRoleName");
        app.UseWillowHealthChecks(new HealthCheckResponse()
        {
            HealthCheckDescription = $"{applicationName} health.",
            HealthCheckName = applicationName!,
        });

        app.MapControllers();
    }

    /// <summary>
    /// Method to migrate database used by the Permission API
    /// </summary>
    public static void MigrateDatabase(IConfiguration config, IServiceProvider serviceProvider)
    {
        // Initialize Metrics
        var meter = serviceProvider.GetRequiredService<Meter>();

        var migrateDbTask = DatabaseMigrator.MigrateAsync(config);
        migrateDbTask.Wait();

        // True if Migration succeeded; else false
        if (migrateDbTask.Result)
        {
            var dbMigrationSuccessCounter = meter.CreateCounter<long>(TelemetryMeterConstants.DatabaseMigrationSucceded);
            dbMigrationSuccessCounter.Add(1);
        }
        else
        {
            var dbMigrationFailedCounter = meter.CreateCounter<long>(TelemetryMeterConstants.DatabaseMigrationFailed);
            dbMigrationFailedCounter.Add(1);
        }

    }
}

