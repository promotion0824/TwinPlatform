using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using MobileXL.Security;
using MobileXL.Services;
using MobileXL.Services.Apis;
using MobileXL.Services.Apis.DirectoryApi;
using MobileXL.Services.Apis.SiteApi;
using MobileXL.Services.Apis.WorkflowApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MobileXL.Services.Apis.ConnectorApi;
using MobileXL.Services.Apis.InsightApi;
using Willow.ServiceBus;
using System.IO;
using System.Reflection;
using System.Diagnostics.Metrics;
using Willow.Telemetry.Web;
using Willow.Telemetry;
using Willow.HealthChecks;
using System.Text.Json;
using MobileXL.Infrastructure.Json;
using MobileXL.Options;
using Authorization.TwinPlatform.Common.Extensions;
using Azure.Security.KeyVault.Keys;
using Azure.Identity;
using Azure.Storage.Blobs;
using Willow.Notifications;

namespace MobileXL
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

            ConfigureDataProtectionService(services);

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });

            string b2cAuthority = Configuration["AzureB2C:Authority"];
            string b2cClientId = Configuration["AzureB2C:ClientId"];
            string b2cTenantId = Configuration["AzureB2C:TenantId"];
            services
                .AddOptions<PushInstallationOptions>()
                .BindConfiguration("ServiceBus:Queues:PushNotificationQueue");
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
                    return PlatformAuthenticationSchemes.MobileCookieScheme;
                };
            })
            .AddCookie("MobileCookies", options =>
            {
                options.Cookie.Name = "WillowMobileAuth";
                options.Cookie.Path = "/mobile-web/";       // Keep the big cookie off the Web root path
                options.Cookie.HttpOnly = true;
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
                        JwtBearerDefaults.AuthenticationScheme,
                        "MobileCookies",
                        "AzureB2C",
                        "AzureAD");
                    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
                });

            // Registers the IUserAuthorizationService
            if (Configuration.GetValue<bool>("SingleTenantOptions:UseSingleTenant"))
            {
                services.AddUserManagementCoreServices(Configuration.GetSection("AuthorizationAPI"));
            }

            services.Configure<SingleTenantOptions>(Configuration.GetSection(nameof(SingleTenantOptions)));

            services
                .AddHealthChecks()
                .AddDependencyService(Configuration, ApiServiceNames.DirectoryCore, HealthStatus.Unhealthy)
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            services.AddServiceBus(Configuration.GetSection("ServiceBus"));
            services.AddScoped<IPushNotificationServer, PushNotificationService>();

            services.AddNotificationsService(opt =>
            {
                opt.QueueOrTopicName = Configuration["CommServiceQueue"];
                opt.ServiceBusConnectionString = Configuration["ServiceBusConnectionString"];
            });
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddSingleton<IImageUrlHelper, ImageUrlHelper>();
            services.AddDigitalTwinApi(this.Configuration);
            services.AddScoped<IAccessControlService, AccessControlService>();
            services.AddScoped<IDirectoryApiService, DirectoryApiService>();
            services.AddScoped<ISiteApiService, SiteApiService>();
            services.AddScoped<IInsightApiService, InsightApiService>();
            services.AddScoped<IWorkflowApiService, WorkflowApiService>();
            services.AddScoped<IDigitalTwinService, DigitalTwinService>();
            services.AddScoped<IConnectorApiService, ConnectorApiService>();
            services.AddScoped<DigitalTwinAssetService>();
            services.AddScoped<IUserCache, UserCache>();

            services.AddSwaggerGen(options =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = "MobileXL app health.",
                HealthCheckName = "MobileXL"
            });

            app.UseAuthentication();
            app.UseWillowContext(Configuration);
            app.UseApiServices(Configuration, env);
        }

        private void ConfigureDataProtectionService(IServiceCollection services)
        {
            var azureCredential = new DefaultAzureCredential();
            var keyVaultName = Configuration.GetValue<string>("DataProtection:KeyVaultName");
            if (string.IsNullOrEmpty(keyVaultName))
            {
                return;
            }
            var keyClient = new KeyClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), azureCredential);
            var keyIdentifier = keyClient.GetKey("DataProtectionKey").Value.Id;

            var storageName = Configuration.GetValue<string>("DataProtection:StorageAccountName");
            var blobServiceClient = new BlobServiceClient(new Uri($"https://{storageName}.blob.core.windows.net/"), azureCredential);
            var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("dataprotectionkeys");
            var blobClient = cloudBlobContainer.GetBlobClient("willowplatform");
            services.AddDataProtection(options => { options.ApplicationDiscriminator = "willow"; })
               .PersistKeysToAzureBlobStorage(blobClient)
               .SetApplicationName("WillowPlatform")
               .SetDefaultKeyLifetime(TimeSpan.FromDays(30))
               .ProtectKeysWithAzureKeyVault(keyIdentifier, azureCredential);
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
    }
}
