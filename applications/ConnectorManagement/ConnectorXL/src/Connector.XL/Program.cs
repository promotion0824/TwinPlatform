namespace Connector.XL;

using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using Connector.XL.Common.Services;
using Connector.XL.Endpoints;
using Connector.XL.Infrastructure.Azure;
using Connector.XL.Infrastructure.HealthCheck;
using Connector.XL.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Willow.Hosting.Web;
using Willow.Telemetry;

/// <summary>
/// Program.
/// </summary>
public class Program
{
    private static List<HealthCheckConfig> dependentHealthChecks = [];

    private const string WillowAuthenticationScheme = "willow";

    /// <summary>
    /// Main.
    /// </summary>
    /// <param name="args">Command line args.</param>
    /// <returns>An exit code.</returns>
    public static int Main(string[] args)
    {
        return WebApplicationStart.Run(args, "ConnectorXL", Configure, ConfigureApp, ConfigureHealthChecks);
    }

    internal static void Configure(WebApplicationBuilder builder)
    {
        AddKeyVault(builder);
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        var services = builder.Services;

        services.AddApiServices(builder.Configuration, builder.Environment);
        services.AddTransient<IAzureBlobService, AzureBlobService>();

        // Add observability packages
        var meterName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        var meterVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        var meter = new Meter(meterName, meterVersion);

        services.AddSingleton(meter);

        var metricsAttributesHelper = new MetricsAttributesHelper(builder.Configuration);
        services.AddSingleton(metricsAttributesHelper);

        services.Configure<HealthCheckSettings>(builder.Configuration.GetSection("HealthChecks"));
        builder.Configuration.GetSection("DependentHealthChecks").Bind(dependentHealthChecks);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddTransient<ISiteClientIdProvider, SiteClientIdProvider>();
        services.AddTransient<ISitesService, SitesService>();
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        string domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = WillowAuthenticationScheme;
            options.DefaultChallengeScheme = WillowAuthenticationScheme;
        }).AddPolicyScheme(WillowAuthenticationScheme,
            WillowAuthenticationScheme,
            options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey("Authorization"))
                    {
                        var authHeader = context.Request.Headers["Authorization"];
                        if (authHeader[0]!.StartsWith("Bearer"))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                    }

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            }).AddCookie(options =>
        {
            options.Cookie.Name = "WillowPlatformAuth";
            options.Cookie.Path = "/";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;

            //options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;   SWAP THIS BACK to SameAsRequest, applicationGateway doesn't have HTTPS setup so I set it to "None"
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        }).AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = builder.Configuration["Auth0:Audience"];
            options.RequireHttpsMetadata = false;
        });

        var licenseKey = builder.Configuration.GetValue<string>("JsonDotNetSchemaLicense");
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            License.RegisterLicense(licenseKey);
        }

        builder.Services.AddEndpointsApiExplorer();
    }

    internal static ValueTask ConfigureApp(WebApplication app)
    {
        app.UseCookiePolicy();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(context =>
                {
                    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (exceptionHandlerFeature != null)
                    {
                        loggerFactory.CreateLogger("ExceptionHandler").LogError(exceptionHandlerFeature.Error, string.Empty, exceptionHandlerFeature.Error.Message);
                    }

                    return Task.CompletedTask;
                });
            });

            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        if (!app.Environment.IsProduction())
        {
            if (app.Configuration.GetValue<bool>("EnableSwagger"))
            {
                var routePrefix = string.Empty;
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.DocumentTitle = $"ConnectorXL - {app.Environment.EnvironmentName}";

                    options.SwaggerEndpoint($"{routePrefix}/swagger/v1/swagger.json", Assembly.GetEntryAssembly()?.GetName().Name + " API V1");

                    if (app.Configuration?.GetSection("Auth0") != null)
                    {
                        var clientId = app.Configuration.GetValue<string>("Auth0:ClientId");
                        options.OAuthClientId(clientId);
                        var audience = app.Configuration.GetValue<string>("Auth0:Audience");
                        options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                        {
                            { "audience", audience },
                        });
                    }
                });
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGroup("/").MapApplicationEndpoints().RequireAuthorization();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return ValueTask.CompletedTask;
    }

    internal static void ConfigureHealthChecks(IHealthChecksBuilder builder)
    {
        foreach (var healthCheck in dependentHealthChecks)
        {
            var hcArgs = new Willow.HealthChecks.HealthCheckFederatedArgs(healthCheck.Url.ToString());
            builder.AddTypeActivatedCheck<Willow.HealthChecks.HealthCheckFederated>(healthCheck.Name, null, ["healthz"], hcArgs);
        }

        builder.AddCheck<DbHealthCheck>(nameof(DbHealthCheck), tags: new[] { "healthz" });
    }

    internal static void AddKeyVault(WebApplicationBuilder builder)
    {
        var keyVaultName = builder.Configuration.GetValue<string>("Azure:KeyVault:KeyVaultName");
        if (string.IsNullOrEmpty(keyVaultName))
        {
            return;
        }

        // The appVersion obtains the app version (1.0.0.0), which
        // is set in the project file and obtained from the entry
        // assembly. The versionPrefix holds the major version
        // for the PrefixKeyVaultSecretManager.
        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        if (assemblyName != null && assemblyName.Version != null)
        {
            var prefix = $"{assemblyName.Name}--{assemblyName.Version.Major}";

            var keyVaultConfigBuilder = new ConfigurationBuilder();

            keyVaultConfigBuilder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net/"), new DefaultAzureCredential(), new PrefixKeyVaultSecretManager(prefix));

            builder.Configuration.AddConfiguration(keyVaultConfigBuilder.Build());
        }
    }
}
