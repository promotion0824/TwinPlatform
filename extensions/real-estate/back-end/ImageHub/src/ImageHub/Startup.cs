using System;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using ImageHub.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Willow.Api.AzureStorage;
using Willow.Azure.Storage;
using Willow.Common;
using Willow.ImageHub.Services;
using ImageHub.Controllers;
using Willow.Telemetry.Web;
using System.Reflection;
using Willow.Telemetry;
using System.Diagnostics.Metrics;
using Willow.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Willow.Infrastructure;

namespace ImageHub
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

            string b2cAuthority = Configuration["AzureB2C:Authority"];
            string b2cClientId = Configuration["AzureB2C:ClientId"];
            string b2cTenantId = Configuration["AzureB2C:TenantId"];

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
                        if (authHeader[0].StartsWith("Bearer"))
                        {
                            return JwtBearerDefaults.AuthenticationScheme;
                        }
                    }
                    if (context.Request.Cookies.ContainsKey("WillowMobileAuth"))
                    {
                        return "MobileCookies";
                    }
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "WillowPlatformAuth";
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
            .AddCookie("MobileCookies", options =>
            {
                options.Cookie.Name = "WillowMobileAuth";
                options.Cookie.Path = "/mobile-web/";       // Keep the big cookie off the Web root path
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
            })
            .AddJwtBearer("AzureB2C", options =>
            {
                options.Authority = b2cAuthority;
                options.Audience = b2cClientId;
                options.RequireHttpsMetadata = false;
            })
            .AddJwtBearer("AzureAD", options =>
            {
                options.Authority = $"https://login.microsoftonline.com/{b2cTenantId}";
                options.Audience = b2cClientId;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudience = b2cClientId,
                    ValidIssuer = $"https://login.microsoftonline.com/{b2cTenantId}/v2.0"
                };
            });

            services
                .AddAuthorization(options =>
                {
                    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        JwtBearerDefaults.AuthenticationScheme,
                        "MobileCookies",
                        "AzureB2C",
                        "AzureAD");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });

            var healthChecksBuilder = services
                .AddHealthChecks()
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.AddSingleton<IFileNameParser, FileNameParser>();
            services.AddSingleton<IImageEngine, ImageEngine>();

            services.AddSingleton<IImageService>( (p)=>
            {
                var logger = p.GetService<ILogger>();

                return new ImageService(p.GetRequiredService<IFileNameParser>(),
                                        CreateImageRepository(p, "OriginalImage", "original", logger),
                                        CreateImageRepository(p, "CachedImage", "cached", logger),
                                        p.GetRequiredService<IImageEngine>());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = "ImageHub app health.",
                HealthCheckName = "ImageHub"
            });

            app.UseAuthentication();
            app.UseWillowContext(Configuration);

            app.UseApiServices(Configuration, env);
        }

        #region Private 

        private IImageRepository CreateImageRepository(IServiceProvider p, string configName, string containerName, ILogger logger)
        {
            return new ImageRepository(p.CreateBlobStore<ImagesController>(new BlobStorageConfig 
                                       { 
                                           AccountName      = this.Configuration.Get($"Storages:{configName}:AccountName", logger),
                                           AccountKey       = this.Configuration.Get($"Storages:{configName}:Key", logger, false),
                                           ContainerName    = containerName,
                                           ConnectionString = this.Configuration.Get($"Storages:{configName}:ConnectionString", logger, false),
                                       }, "", true));
        }

        private void ConfigureDataProtectionService(IServiceCollection services)
        {
            var keyVaultName = Configuration.Get("DataProtection:KeyVaultName", null, false);

            if (string.IsNullOrWhiteSpace(keyVaultName))
            {
                return;
            }

            var storageAccount      = Configuration.Get("DataProtection:StorageAccountName", null);
            var storageKey          = Configuration.Get("DataProtection:StorageKey", null);
            var keyVaultClient      = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));

            var keyBundle           = keyVaultClient.GetKeyAsync($"https://{keyVaultName}.vault.azure.net/", "DataProtectionKey").ConfigureAwait(false).GetAwaiter().GetResult();
            var keyIdentifier       = keyBundle.KeyIdentifier.Identifier;

            var storageCredentials  = new StorageCredentials(storageAccount, storageKey);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            var cloudBlobClient     = cloudStorageAccount.CreateCloudBlobClient();
            
            var cloudBlobContainer  = cloudBlobClient.GetContainerReference("dataprotectionkeys");
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

        public static Task WriteHealthReportResponse(HttpContext context, HealthReport healthReport)
        {
            var healthCheckDto = new HealthCheckDto(Assembly.GetEntryAssembly().GetName().Name, "Health Report", healthReport);

            var healthCheckDtoJson = JsonSerializer.Serialize(healthCheckDto, JsonSerializerExtensions.DefaultOptions);

            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(healthCheckDtoJson);
        }
        #endregion
    }

    public static class ConfigurationExtensions
    {
        public static string Get(this IConfiguration config, string name, ILogger logger, bool required = true)
        {
            string val = "";
            
            try
            {
                val = config[name];
            }
            catch
            {
            }

            if(required && (string.IsNullOrWhiteSpace(val) || val.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase)))
            {
                if(logger != null)
                    logger.LogError($"Missing configuration entry: {name}");

                throw new Exception($"Missing configuration entry: {name}");
            }

            if(val == null || val.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase))
                return null;

            return val;
        }
    }    
}
