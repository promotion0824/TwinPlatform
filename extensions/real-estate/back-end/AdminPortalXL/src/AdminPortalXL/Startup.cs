using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using AdminPortalXL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.Telemetry.Web;
using System.Reflection;
using System.Diagnostics.Metrics;
using Willow.Telemetry;

namespace AdminPortalXL
{
    public class Startup
    {
        private const string WillowAuthenticationScheme = "willow";
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

            ConfigureDataProtectionService(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            string domain = $"https://{Configuration["Auth0:Domain"]}/";
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = WillowAuthenticationScheme;
                options.DefaultChallengeScheme = WillowAuthenticationScheme;
            })
            .AddPolicyScheme(WillowAuthenticationScheme, WillowAuthenticationScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey("Authorization"))
                    {
                        var authHeader = context.Request.Headers["Authorization"];
                        if (authHeader[0].StartsWith("Bearer", StringComparison.InvariantCulture))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                    }
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "WillowAdminPortalAuth";
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
            })
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = Configuration["Auth0:Audience"];
                options.RequireHttpsMetadata = false;
            });

            services
                .AddHealthChecks()
                .AddDependencyService(Configuration, ApiServiceNames.DirectoryCore, HealthStatus.Unhealthy)
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.AddScoped<IDirectoryApiService, DirectoryApiService>();
            services.AddScoped<IRegionalDirectoryApi, RegionalDirectoryApi>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseAuthentication();
            app.UseApiServices(Configuration, env);
        }

        private void ConfigureDataProtectionService(IServiceCollection services)
        {
            var keyVaultName = Configuration.GetValue<string>("DataProtection:KeyVaultName");
            if (string.IsNullOrEmpty(keyVaultName))
            {
                return;
            }

            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

            var keyBundle = keyVaultClient.GetKeyAsync($"https://{keyVaultName}.vault.azure.net/", "DataProtectionKey")
                .ConfigureAwait(false).GetAwaiter().GetResult();
            var keyIdentifier = keyBundle.KeyIdentifier.Identifier;

            var storageCredentials = new StorageCredentials(
                Configuration.GetValue<string>("DataProtection:StorageAccountName"),
                Configuration.GetValue<string>("DataProtection:StorageKey"));
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference("dataprotectionkeys");

            services.AddDataProtection(options => { options.ApplicationDiscriminator = "willow"; })
                .PersistKeysToAzureBlobStorage(cloudBlobContainer, $"willowplatform")
                .SetApplicationName("WillowPlatform")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
                .ProtectKeysWithAzureKeyVault(keyVaultClient, keyIdentifier);
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
