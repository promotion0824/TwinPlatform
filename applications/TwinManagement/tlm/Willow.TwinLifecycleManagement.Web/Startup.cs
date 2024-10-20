using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Willow.Copilot.ProxyAPI;
using Willow.Copilot.ProxyAPI.Extensions;
using Willow.Exceptions;
using Willow.HealthChecks;
using Willow.Telemetry.Web;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Diagnostic;
using Willow.TwinLifecycleManagement.Web.Extensions;

namespace Willow.TwinLifecycleManagement.Web;

public class Startup
{
    private readonly CancellationTokenSource readinessCancellationTokenSource = new();
    private readonly string applicationName;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
        applicationName = configuration.GetValue<string>("ApplicationInsights:CloudRoleName");
    }

    public IConfiguration Configuration { get; }

    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add controllers and make it visible through app part manager
        services.AddControllers()
            .ConfigureApplicationPartManager(apm => apm.ApplicationParts.Add(new AssemblyPart(typeof(Program).Assembly)));
        services.AddOptions(Configuration);
        services.ConfigureSwagger();

        services.AddHttpContextAccessor();

        services.AddSingleton((c) => new HealthCheckFederated(
            new HealthCheckFederatedArgs(Configuration.GetValue<string>("TwinsApi:BaseUrl"), "/healthz", Environment.IsDevelopment()),
            c.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<HealthCheckMTI>();
        services.AddHostedService<StartupHealthCheckService>();
        services.AddHealthChecks()
            .AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
            .AddCheck("readyz", () =>
            {
                readinessCancellationTokenSource.Token.ThrowIfCancellationRequested();
                return HealthCheckResult.Healthy("System is ready.");
            }, tags: ["readyz"])
            .AddCheck<HealthCheckFederated>("twins-api", tags: ["healthz"]);

        // Add HealthCheckMTI if the feature flag is enabled
        bool isMappedDisabled = Configuration.GetValue<bool>("MtiOptions:IsMappedDisabled", defaultValue: false);
        if (!isMappedDisabled)
        {
            services.AddHealthChecks().AddCheck<HealthCheckMTI>("MTI", tags: ["healthz"]);
        }

        services.AddMemoryCache();
        services.RegisterDependentServices(Configuration);
        services.RegisterTLMServices();
        services.RegisterTools();
        services.RegisterDataQualityApiServices();
        services.ConfigureCopilotClients(Configuration.GetSection("Copilot").Get<CopilotSettings>());

        if (!string.IsNullOrWhiteSpace(applicationName) &&
            !string.IsNullOrWhiteSpace(Configuration.GetValue<string>("ApplicationInsights:ConnectionString")))
        {
            // syncs to app insights using open telemetry
            services.AddWillowContext(Configuration);
        }

        var jwtBearerConfig = new JwtBearerConfig();
        Configuration.Bind(JwtBearerConfig.Config, jwtBearerConfig);
        services.Configure<MicrosoftIdentityOptions>(JwtBearerDefaults.AuthenticationScheme, Configuration.GetSection("AzureAdB2C"));

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddMicrosoftIdentityWebApi(
            (JwtBearerOptions jwtOptions) =>
            {
                jwtOptions.Authority = jwtBearerConfig.Authority;
                jwtOptions.Audience = jwtBearerConfig.Audience;
                jwtOptions.ClaimsIssuer = jwtBearerConfig.Issuer;
            },
            (MicrosoftIdentityOptions identityOptions) =>
            {
                identityOptions = Configuration.GetSection("AzureAdB2C").Get<MicrosoftIdentityOptions>();
            },
            JwtBearerDefaults.AuthenticationScheme,
            true);
        services.RegisterAuthorizationServices(Configuration.GetSection("AuthorizationAPI"), Environment);
        services.AddCors();
        services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    public void Configure(WebApplication app, IHostApplicationLifetime hostApplicationLifetime)
    {
        if (app.Environment.IsDevelopment())
        {
            // Enable showing of personally identifiable information in authentication requests
            IdentityModelEventSource.ShowPII = true;

            Microsoft.AspNetCore.Builder.SwaggerBuilderExtensions.UseSwagger(app);
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Twin Lifecycle Management v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition")
            .SetIsOriginAllowed(origin => true) // allow any origin (Development-only)
            .AllowCredentials());
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseWillowContext(Configuration);

        hostApplicationLifetime.ApplicationStopping.Register(readinessCancellationTokenSource.Cancel);
        hostApplicationLifetime.ApplicationStopped.Register(readinessCancellationTokenSource.Cancel);
        app.UseWillowHealthChecks(new HealthCheckResponse()
        {
            HealthCheckDescription = $"{applicationName} health.",
            HealthCheckName = applicationName,
        });

        app.MapControllers();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");

        app.MapFallbackToFile("index.html");
    }
}
