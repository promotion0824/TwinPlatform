using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Common.Options;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Authorization.TwinPlatform.Web.HealthChecks;
using Authorization.TwinPlatform.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Willow.HealthChecks;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace Authorization.TwinPlatform.Web.Extensions;

/// <summary>
/// Class that contain extension methods used for building the application
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Extension method to register the background services used by the Front End
    /// </summary>
    /// <param name="services">Service Collection</param>
    public static void AddBackendServices(this IServiceCollection services)
    {
        services.AddScoped<IRoleManager, RoleManager>();
        services.AddScoped<IGroupTypeManager, GroupTypeManager>();
        services.AddScoped<IGroupManager, GroupManager>();
        services.AddScoped<IPermissionManager, PermissionManager>();
        services.AddScoped<IUserManager, UserManager>();
        services.AddScoped<IRoleAssignmentManager, RoleAssignmentManager>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserAuthorizationManager, UserAuthorizationManager>();
        services.AddScoped<IImportExportManager, ImportExportManager>();
        services.AddScoped<ITwinManager, TwinManager>();
        services.AddScoped<IApplicationManager, ApplicationManager>();
        services.AddScoped<IClientAssignmentManager, ClientAssignmentManager>();

        services.AddScoped<IRecordChangeListener,EmailManager>();
        services.AddScoped<IRecordChangeListener, CacheManager>();
    }

    /// <summary>
    /// Extension method to register web api authentication
    /// </summary>
    /// <param name="services">Service Collection</param>
    public static void AddApiAuthentication(this IServiceCollection services, ConfigurationManager configuration)
    {
        // Adds Microsoft Identity platform (Azure AD B2C) support to protect this Api
        _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    var jwtBearerConfig = new JwtBearerConfig();
                    configuration.Bind(JwtBearerConfig.CONFIG, jwtBearerConfig);

                    options.Authority = jwtBearerConfig.Authority;
                    options.Audience = jwtBearerConfig.Audience;
                    options.IncludeErrorDetails = true;
                    options.TokenValidationParameters.NameClaimType = "name";
                    options.Validate();

                    //options.Events = GetJwtBearerEventHandler();
                },
                identityOptions =>
                {
                    configuration.Bind("AzureAdB2C", identityOptions);
                },
                JwtBearerDefaults.AuthenticationScheme,
                subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);
    }

    /// <summary>
    /// Add User Management Web Health Checks to the DI Container
    /// </summary>
    /// <param name="services">Service Collection Instance</param>
    public static void AddUMHealthChecks(this IServiceCollection services, IConfiguration configuration, Action<IHealthChecksBuilder>? additionalChecks = null)
    {
        services.AddSingleton<HealthCheckAuthorizationPermissionApi>();

        var authAPIUrl = configuration?.GetSection(AuthorizationAPIOption.APIName)?.Get<AuthorizationAPIOption>()?.BaseAddress;

        var hcArgs = new HealthCheckFederatedArgs(authAPIUrl!);

        services.AddSingleton((c) => new HealthCheckFederated(hcArgs, c.GetRequiredService<IHttpClientFactory>()));

       var healthBuilder =  services.AddAuthorizationHealthChecks()
            .AddCheck<HealthCheckAuthorizationPermissionApi>("Authorization Permission Api", tags: ["healthz"])
            .AddCheck<HealthCheckFederated>("Authorization Api", tags: ["healthz"]);

        additionalChecks?.Invoke(healthBuilder);
    }

    /// <summary>
    /// Adds UM Observability telemetry initializers to the collection.
    /// </summary>
    /// <param name="services">IServiceCollection Instance.</param>
    /// <param name="configuration">IConfiguration Instance.</param>
    /// <returns>Instance of <see cref="Meter"/></returns>
    public static Meter AddUMTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWillowContext(configuration);
        var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName();

        services.AddSingleton(new MetricsAttributesHelper(configuration));

        var meter = new Meter(entryAssemblyName?.Name ?? "Unknown",
            entryAssemblyName?.Version?.ToString() ?? "Unknown");
        services.AddSingleton(meter);

        return meter;
    }

    private static JwtBearerEvents GetJwtBearerEventHandler()
    {
        return new JwtBearerEvents()
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                // Ensure we always have an error and error description.
                if (string.IsNullOrEmpty(context.Error))
                    context.Error = "invalid_token";
                if (string.IsNullOrEmpty(context.ErrorDescription))
                    context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                // Add some extra context for expired tokens.
                bool isExpiredToken = context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException);
                if (isExpiredToken)
                {
                    var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                    context.Response.Headers.TryAdd("x-token-expired", authenticationException?.Expires.ToString("o"));
                    context.ErrorDescription = $"The token expired on {authenticationException?.Expires.ToString("o")}";
                }

                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                HttpValidationProblemDetails()
                {
                    Title = context.Error,
                    Detail = context.ErrorDescription
                }));
            }
        };
    }
}
