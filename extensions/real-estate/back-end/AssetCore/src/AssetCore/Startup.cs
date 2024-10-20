using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Services;
using AssetCoreTwinCreator.BusinessLogic;
using AssetCoreTwinCreator.Database;
using AssetCoreTwinCreator.BusinessLogic.AssetOperations.ReadAssets;
using AssetCoreTwinCreator.Mapping;
using AssetCoreTwinCreator.Features.Asset.Search;
using AssetCoreTwinCreator.Features.Asset.Attachments;
using AssetCoreTwinCreator.Import;
using AssetCoreTwinCreator.Import.Services;
using AssetCoreTwinCreator.MappingId;
using AutoMapper;
using AssetCore.Database;
using Microsoft.Extensions.Logging;
using AssetCore.Infrastructure.Configuration;

using Willow.Api.Storage;
using Willow.Telemetry.Web;
using System.Reflection;
using Willow.Telemetry;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AssetCore
{
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
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            ConfigureTelemetryService(services);

            services.AddApiServices(Configuration, _env);

            services.AddMemoryCache();

            var azureB2CSection = Configuration.GetSection("AzureB2C");
            var azureB2CConfig = azureB2CSection.Get<AzureADB2CConfiguration>();

            services.AddJwtAuthentication(Configuration["Auth0:Domain"], Configuration["Auth0:Audience"], azureB2CConfig, _env);
            AddDbContexts(services);

            services
                .AddHealthChecks()
                .AddDbContextCheck<AssetDbContext>()
                .AddAssemblyVersion();

            AddTwinCreatorAssetServices(services);
        }

        private void AddTwinCreatorAssetServices(IServiceCollection services)
        {
            //auto mapper profiles
            services.AddSingleton(new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile())).CreateMapper());

            services.AddSingleton<IDatabase, AssetCoreTwinCreator.Database.Database>();
            services.AddSingleton<IDatabaseConfiguration, DatabaseConfiguration>();
            services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IAssetSearch, AssetSearch>();
            services.AddScoped<IAssetSearchDatabaseQueries, AssetSearchDatabaseQueries>();
            services.AddScoped<ICategoryStructure, CategoryStructure>();
            services.AddTransient<IAttachmentsService, AttachmentsService>();

            services.Configure<BlobStorageConfig>(Configuration.GetSection("Azure:BlobStorage"));
            services.Configure<AzureADB2CConfiguration>(Configuration.GetSection("AzureB2C"));

            //services
            services.AddScoped<IAssetRepository, AssetRepository>();

            //asset operations
            services.AddScoped<IReadAssets, ReadAssets>();
            services.AddTransient<IMappingService, MappingService>();
            services.AddSingleton<IAssetRegisterIndexCacheService, AssetRegisterIndexCacheService>();
            services.AddSingleton<ICategoryIndexCacheService, CategoryIndexCacheService>();

            services.AddTransient<IAssetMappingImportService, AssetMappingImportService>();
            services.AddTransient<IMappingImporter, SiteMappingImportService>();
            services.AddTransient<IMappingImporter, FloorMappingImportService>();
            services.AddTransient<IMappingImporter, AssetGeometryImportService>();
            services.AddTransient<IMappingImporter, AssetGeometryByIdImportService>();
            services.AddTransient<IMappingImporter, AssetEquipmentMappingImportService>();

            services.AddAzureStorage<AttachmentsController>(Configuration);
        }

        private void AddDbContexts(IServiceCollection services)
        {
            services.AddDbContext<AssetDbContext>(o => SetOptions(o, "AssetDb"));
            services.AddDbContext<MappingDbContext>(o => SetOptions(o, "MappingDb"));
            return;

            void SetOptions(DbContextOptionsBuilder optionsBuilder, string connectionStringName)
            {
                const int couldNotConnect = 40;
                var connectionString = Configuration.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Connection string {connectionStringName} not found");
                optionsBuilder.UseSqlServer(connectionString, b => b.EnableRetryOnFailure(6, TimeSpan.FromSeconds(30), [couldNotConnect]));
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbUpgradeChecker dbUpgradeChecker)
        {
            app.UseHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseAuthentication();
            app.UseApiServices(Configuration, env);

            dbUpgradeChecker.EnsureDatabaseUpToDate(env);
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
