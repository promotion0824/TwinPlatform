namespace Willow.IoTService.Deployment.Dashboard;

using Authorization.TwinPlatform.Common.Extensions;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.IoTService.Deployment.Dashboard.Application;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;
using Willow.IoTService.Deployment.Dashboard.Endpoints;
using Willow.IoTService.Deployment.Dashboard.Infrastructure;
using Willow.IoTService.Deployment.Dashboard.Options;
using Willow.IoTService.Deployment.DataAccess.Db;
using Willow.IoTService.Deployment.DataAccess.DependencyInjection;
using Willow.IoTService.Deployment.ManifestStorage.Hosting;

public class Program
{
    private static List<HealthCheckConfig> dependentHealthChecks = [];

    public static int Main(string[] args)
    {
        return WebApplicationStart.Run(args,
            "Willow.IoTService.Deployment.Dashboard",
            Configure,
            ConfigureApp,
            ConfigureHealthChecks);
    }

    private static void Configure(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

        builder.Services.AddAuthentication()
            .AddMicrosoftIdentityWebApi(options =>
                {
                    options.IncludeErrorDetails = true;
                    options.Validate();
                },
                identityOptions => builder.Configuration.Bind("AzureAdAuth", identityOptions),
                Constants.AzureAd);

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "SchemeSelection";
            })
            .AddPolicyScheme("SchemeSelection",
                "SchemeSelection",
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                        context.Request.Headers["Authorization-Scheme"].SingleOrDefault() ??
                        JwtBearerDefaults.AuthenticationScheme;
                });

        // Add UserManagement authorization service
        if (!string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("AuthorizationAPI:BaseAddress")) &&
            !string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("AuthorizationAPI:TokenAudience")))
        {
            // Registers the IUserAuthorizationService
            builder.Services.AddPermissionBasedPolicyAuthorization(
                builder.Configuration.GetSection("AuthorizationAPI"));
            builder.Configuration.AddUserManagementEnvironmentSpecificConfigSource();
        }

        // Add services to the container.
        builder.Services.AddTransient<IDeployModuleService, DeployModuleService>();
        builder.Services.AddSingleton<HealthCheckServiceBus>();
        builder.Services.AddSingleton<HealthCheckSql>();
        builder.Services.AddHostedService<StartupHealthCheckService>();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AzureServiceBusOptions).Assembly));
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuditLogPipeline<,>));

        // add controllers with api versioning
        builder.Services.AddVersionedControllersWithValidation();

        // configure swagger with api versioning
        builder.Services.AddSwaggerGenCustom(builder.Configuration);

        // configure MassTransit
        builder.Services.Configure<AzureServiceBusOptions>(
            builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
        builder.Services.AddMassTransit(config =>
        {
            var host = builder.Configuration.GetSection(AzureServiceBusOptions.SectionName)
                .Get<AzureServiceBusOptions>()
                ?.HostAddress;
            config.UsingAzureServiceBus((_, cfg) =>
                cfg.Host(new Uri(host ?? throw new ArgumentNullException("AzureServiceBus:HostAddress"))));
        });

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IConfigureOptions<HealthCheckServiceOptions>, RemoveMasstransitHealthChecks>());
        builder.Services.AddOptions<ADB2COptions>().BindConfiguration(ADB2COptions.CONFIG);
        builder.Services.AddAuthorization();

        // Add dependencies
        builder.Services.ConfigureDataAccess<UserInfoService>(
            builder.Configuration.GetConnectionString("DeploymentDb") ??
            throw new ArgumentNullException("ConnectionStrings:DeploymentDb"));
        builder.Services.UseManifestStorage(builder.Configuration);

        builder.Services.AddSingleton<IAuthorizationPolicyProvider, ServiceToServiceOrDefaultPolicyProvider>();

        builder.Configuration.GetSection("DependentHealthChecks").Bind(dependentHealthChecks);

        builder.Services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
        });
    }

    private static void ConfigureHealthChecks(IHealthChecksBuilder builder)
    {
        builder.AddCheck<HealthCheckServiceBus>("Service Bus", tags: new[] { "healthz" })
               .AddCheck<HealthCheckSql>("DeploymentDb SQL", tags: new[] { "healthz" });

        foreach (var healthCheck in dependentHealthChecks)
        {
            var hcArgs = new HealthCheckFederatedArgs(healthCheck.Url.ToString());
            builder.AddTypeActivatedCheck<HealthCheckFederated>(healthCheck.Name, null, ["healthz"], hcArgs);
        }
    }

    private static ValueTask ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseSwaggerAndUIWithAad();
        app.MapAllHealthChecks();
        app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/api/error-development" : "/api/error");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGroup("/api/v1/").MapV1ApplicationEndpoints();

        app.MapFallbackToFile("index.html");

        app.Services
            .GetRequiredService<IServiceProvider>()
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<DeploymentDbContext>()
            .Database
            .Migrate();

        return ValueTask.CompletedTask;
    }
}

internal class RemoveMasstransitHealthChecks : IConfigureOptions<HealthCheckServiceOptions>
{
    public void Configure(HealthCheckServiceOptions options)
    {
        var masstransitChecks = options.Registrations.Where(x => x.Tags.Contains("masstransit")).ToList();

        foreach (var check in masstransitChecks)
        {
            options.Registrations.Remove(check);
        }
    }
}
