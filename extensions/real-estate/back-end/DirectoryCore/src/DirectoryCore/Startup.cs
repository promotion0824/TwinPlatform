using System;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Storage.Blobs;
using DirectoryCore.Configs;
using DirectoryCore.Database;
using DirectoryCore.Entities;
using DirectoryCore.Extensions;
using DirectoryCore.Http;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using DirectoryCore.Services.AzureB2C;
using DirectoryCore.Services.UserSetupService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Willow.Common;
using Willow.Database;
using Willow.HealthChecks;
using Willow.Infrastructure;
using Willow.Infrastructure.Database;
using Willow.Notifications;
using Willow.Security.KeyVault;
using Willow.Security.KeyVault.Options;
using Willow.ServiceBus;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace DirectoryCore
{
    public class Startup
    {
        private const int DATA_PROTECTION_KEY_LIFETIME = 30;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiServices(Configuration, _env);
            services.AddMemoryCache();
            services.AddRetryPipelines();

            ConfigureDataProtectionService(services);

            var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2COptions = azureB2CSection.Get<AzureADB2COptions>();

            services.AddJwtAuthentication(
                Configuration["Auth0:Domain"],
                Configuration["Auth0:Audience"],
                azureB2COptions,
                _env
            );

            services
                .AddHealthChecks()
                .AddDbContextCheck<DirectoryDbContext>()
                .AddDependencyService(
                    Configuration,
                    ApiServiceNames.ImageHub,
                    HealthStatus.Degraded
                )
                .AddDependencyService(
                    Configuration,
                    ApiServiceNames.SiteCore,
                    HealthStatus.Degraded
                )
                .AddAssemblyVersion();

            ConfigureTelemetryService(services);

            var connectionString = Configuration
                .GetSection($"ConnectionStrings:{Constants.DatabaseName}")
                .Value;

            services.AddDbContext<DirectoryDbContext>(
                (sp, options) =>
                    options.UseSqlServer(
                        connectionString,
                        opt =>
                            // Don't call EnableRetryLogic, the following call does the same job
                            opt.ExecutionStrategy(
                                    c =>
                                        new CustomAzureSqlExecutionStrategy(
                                            c,
                                            logger: sp.GetRequiredService<
                                                ILogger<CustomAzureSqlExecutionStrategy>
                                            >()
                                        )
                                )
                                .UseAzureSqlDefaults()
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                    )
            );

            services.AddDataProtection();

            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            // Configs
            services.Configure<SignUpOption>(Configuration.GetSection("SignUpOption"));
            services.Configure<Auth0Option>(Configuration.GetSection("Auth0"));
            services.Configure<WillowContextOptions>(Configuration.GetSection("WillowContext"));
            services.Configure<SingleTenantOptions>(
                Configuration.GetSection("SingleTenantOptions")
            );
            services.Configure<AzureADB2COptions>(azureB2CSection);

            // Services
            services.AddSingleton<IImagePathHelper, ImagePathHelper>();
            services.AddScoped<IImageHubService, ImageHubService>();
            services.AddScoped<ICustomersService, CustomersService>();
            services.AddScoped<ISitesService, SitesService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ISupervisorsService, SupervisorsService>();
            services.AddScoped<ICustomerUsersService, CustomerUsersService>();
            services.AddScoped<ISingleTenantSetupService, SingleTenantSetupService>();
            services.AddScoped<IAuth0ManagementService, Auth0ManagementService>();
            services.AddScoped<IAuth0Service, Auth0Service>();
            services.AddScoped<IAzureB2CService, AzureB2CService>();
            services.AddScoped<IDatabase>(
                p =>
                    new Willow.Database.Database(
                        this.Configuration.GetValue<string>("ConnectionStrings:DirectoryDb")
                    )
            );
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddSingleton<IMessageQueue>(p =>
            {
                var connectionString = Configuration["ServiceBusConnectionString"];
                var queue = Configuration["CommServiceQueue"];

                if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(queue))
                {
                    return null;
                }
                return new ServiceBus(connectionString, queue);
            });
            services.AddNotificationsService(opt =>
            {
                opt.QueueOrTopicName = Configuration["CommServiceQueue"];
                opt.ServiceBusConnectionString = Configuration["ServiceBusConnectionString"];
            });

            services.AddSingleton<IHttpRequestHeaders>(p => new HttpRequestHeaders());

            IConfigurationSection azureKeyVaultSection = Configuration.GetSection("Azure:KeyVault");
            services.AddSecretManager(azureKeyVaultSection);

            services.AddLazyCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IDbUpgradeChecker dbUpgradeChecker
        )
        {
            app.UseWillowHealthChecks(
                new HealthCheckResponse()
                {
                    HealthCheckDescription = "DirectoryCore app health.",
                    HealthCheckName = "DirectoryCore"
                }
            );

            app.UseAuthentication();
            app.UseAuthorization();
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
            var keyClient = new KeyClient(
                new Uri($"https://{keyVaultName}.vault.azure.net/"),
                azureCredential
            );
            var keyIdentifier = keyClient.GetKey("DataProtectionKey").Value.Id;

            var storageName = Configuration.GetValue<string>("DataProtection:StorageAccountName");
            var blobServiceClient = new BlobServiceClient(
                new Uri($"https://{storageName}.blob.core.windows.net/"),
                azureCredential
            );
            var cloudBlobContainer = blobServiceClient.GetBlobContainerClient("dataprotectionkeys");
            var blobClient = cloudBlobContainer.GetBlobClient("directorycore");
            services
                .AddDataProtection(options =>
                {
                    options.ApplicationDiscriminator = "willow";
                })
                .PersistKeysToAzureBlobStorage(blobClient)
                .SetApplicationName("DirectoryCore")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(DATA_PROTECTION_KEY_LIFETIME))
                .ProtectKeysWithAzureKeyVault(keyIdentifier, azureCredential);
        }

        private void ConfigureTelemetryService(IServiceCollection services)
        {
            if (
                string.IsNullOrWhiteSpace(
                    Configuration.GetValue<string>("ApplicationInsights:ConnectionString")
                )
            )
                return;

            services.AddWillowContext(Configuration);

            var meterName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
            var meterVersion =
                Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown";
            var meter = new Meter(meterName, meterVersion);

            services.AddSingleton(meter);

            var metricsAttributesHelper = new MetricsAttributesHelper(Configuration);
            services.AddSingleton(metricsAttributesHelper);
        }

        public static Task WriteHealthReportResponse(HttpContext context, HealthReport healthReport)
        {
            var healthCheckDto = new HealthCheckDto(
                Assembly.GetEntryAssembly().GetName().Name,
                "Health Report",
                healthReport
            );

            var healthCheckDtoJson = JsonSerializer.Serialize(
                healthCheckDto,
                JsonSerializerExtensions.DefaultOptions
            );

            context.Response.ContentType = "application/json; charset=utf-8";

            return context.Response.WriteAsync(healthCheckDtoJson);
        }
    }
}
