namespace ConnectorCore;

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Identity;
using ConnectorCore.Common.Abstractions;
using ConnectorCore.Common.Models;
using ConnectorCore.Data;
using ConnectorCore.Data.Models;
using ConnectorCore.Database;
using ConnectorCore.Entities;
using ConnectorCore.Extensions;
using ConnectorCore.Infrastructure.Configuration;
using ConnectorCore.Infrastructure.HealthCheck;
using ConnectorCore.Models;
using ConnectorCore.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.AzureDigitalTwins.SDK.Extensions;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.Infrastructure;
using Willow.Telemetry;
using PrefixKeyVaultSecretManager = Willow.Infrastructure.Azure.PrefixKeyVaultSecretManager;

internal class Program
{
    private static List<IFeatureModule> featureModules = [];

    public static int Main(string[] args)
    {
        SetJsonSerialiserDefaults();
        return WebApplicationStart.Run(args, "ConnectorCore", Configure, ConfigureApp, ConfigureHealthChecks);
    }

    internal static void Configure(WebApplicationBuilder builder)
    {
        AddKeyVault(builder);

        var services = builder.Services;
        var configuration = builder.Configuration;
        var environment = builder.Environment;

        services.AddApiServices(configuration, environment);

        var azureB2CSection = configuration.GetSection("AzureB2C");
        var azureB2CConfig = azureB2CSection.Get<AzureADB2CConfiguration>();

        services.AddJwtAuthentication(configuration["Auth0:Domain"], configuration["Auth0:Audience"], azureB2CConfig, environment);

        featureModules = GetFeatureModules().ToList();

        services.AddSingleton<IDbUpgradeChecker, DbUpgradeChecker>();

        services.Configure<AppSettings>((options) => configuration.Bind(options));
        services.Configure<ScannerBlobStorageOptions>(configuration.GetSection(nameof(ScannerBlobStorageOptions)));
        services.Configure<MSBlobStorageRootOptions>(configuration.GetSection(nameof(MSBlobStorageRootOptions)));
        services.Configure<AzureADB2CConfiguration>(azureB2CSection);
        services.AddAdtApiHttpClient(builder.Configuration.GetSection("TwinsApi"));
        services.AddHttpClient<ITwinsClient, TwinsClient>(Willow.AzureDigitalTwins.SDK.Extensions.ServiceCollectionExtensions.AdtApiClientName);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddMemoryCache();
        services.AddLazyCache();
        services.AddOptions<CacheOptions>().Bind(configuration.GetSection(nameof(CacheOptions)));
        services.AddOptions<Willow.Api.Authentication.AzureADOptions>().Bind(configuration.GetSection("AzureAd"));

        builder.Services.AddDbContext<IConnectorCoreDbContext, ConnectorCoreDbContext>((serviceProvider, options) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("ConnectorCoreDbConnection");

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.ExecutionStrategy(deps => new CustomAzureSqlExecutionStrategy(deps, 6, TimeSpan.FromSeconds(30), null, serviceProvider.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()));
            });
        });

        services.AddTransient<IDbConnectionStringProvider, DbConnectionStringProvider>();
        services.AddTransient<IDbConnectionProvider, DbConnectionProvider>();
        services.AddTransient<IDatabaseUpdater, DatabaseUpdater>();
        services.AddTransient(typeof(IContinuationTokenProvider<PointEntity, Guid>), typeof(PointsCTokenProvider));
        services.AddTransient(typeof(IContinuationTokenProvider<EquipmentEntity, Guid>), typeof(EquipmentsCTokenProvider));
        services.AddSingleton<IEquipmentCacheProviderService, EquipmentCacheProviderService>();
        services.AddTransient<AzureBlobService, AzureBlobService>();
        services.AddTransient<IScannerBlobService, ScannerBlobService>();
        services.AddTransient<IDigitalTwinService, DigitalTwinService>();
        services.AddTransient<IMSBlobStorageService, MSBlobStorageService>();
        services.AddSingleton<IMetricsCollector, MetricsCollector>();
        services.AddSingleton<ManagedIdentityTokenManager>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingAzureServiceBus((ctx, cfg) =>
            {
                var connectionString = configuration.GetConnectionString("AzureServiceBus");
                if (string.IsNullOrEmpty(connectionString))
                {
                    cfg.Host(new Uri(configuration.GetValue<string>("AzureServiceBusHost")));
                }
                else
                {
                    cfg.Host(connectionString);
                }

                cfg.ConfigureEndpoints(ctx);
            });
        });

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, RemoveMasstransitHealthChecks>());

        foreach (var featureModule in featureModules)
        {
            featureModule.Register(services, configuration, environment);
        }
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
        var assemblyName = Assembly.GetEntryAssembly().GetName();
        var prefix = $"{assemblyName.Name}--{assemblyName.Version.Major}";

        var keyVaultConfigBuilder = new ConfigurationBuilder();

        keyVaultConfigBuilder.AddAzureKeyVault(
            new Uri($"https://{keyVaultName}.vault.azure.net/"),
            new DefaultAzureCredential(),
            new PrefixKeyVaultSecretManager(prefix));

        builder.Configuration.AddConfiguration(keyVaultConfigBuilder.Build());
    }

    internal static async ValueTask ConfigureApp(WebApplication app)
    {
        foreach (var featureModule in featureModules)
        {
            featureModule.Startup(app.Services, app.Configuration);
        }

        app.MapGroup(string.Empty).MapEndpoints().RequireAuthorization();
        app.UseApiServices(app.Configuration, app.Environment);
        var isDevEnvironment = app.Environment.EnvironmentName.Equals("dev", StringComparison.InvariantCultureIgnoreCase);
        var isTestEnvironment = app.Environment.EnvironmentName.Equals("test", StringComparison.InvariantCultureIgnoreCase);

        DeployDatabaseChanges(app, isDevEnvironment || isTestEnvironment);

        await MigrateEfDatabaseAndSeedData(app);
    }

    internal static async Task MigrateEfDatabaseAndSeedData(WebApplication app)
    {
        var dbContext = app.Services.CreateScope().ServiceProvider.GetRequiredService<IConnectorCoreDbContext>();
        await dbContext.MigrateAsync();

        var existingConnectorTypes = await dbContext.ConnectorTypes.ToDictionaryAsync(x => x.Id, x => x);
        var existingSchema = await dbContext.Schemas.ToDictionaryAsync(x => x.Id, x => x);
        var existingSchemaColumn = await dbContext.SchemaColumns.ToDictionaryAsync(x => x.Id, x => x);

        var connectorTypes = JsonSerializer.Deserialize<List<ConnectorType>>(File.ReadAllText("Data/Seed/ConnectorTypes.json")).Where(x => !existingConnectorTypes.ContainsKey(x.Id));
        var schema = JsonSerializer.Deserialize<List<Schema>>(File.ReadAllText("Data/Seed/Schema.json")).Where(x => !existingSchema.ContainsKey(x.Id));
        var schemaColumn = JsonSerializer.Deserialize<List<SchemaColumn>>(File.ReadAllText("Data/Seed/SchemaColumn.json")).Where(x => !existingSchemaColumn.ContainsKey(x.Id));

        dbContext.Schemas.AddRange(schema);
        await dbContext.SaveChangesAsync();
        dbContext.SchemaColumns.AddRange(schemaColumn);
        await dbContext.SaveChangesAsync();
        dbContext.ConnectorTypes.AddRange(connectorTypes);
        await dbContext.SaveChangesAsync();
    }

    internal static void DeployDatabaseChanges(IApplicationBuilder app, bool isDevEnvironment)
    {
        var dbUpdater = app.ApplicationServices.GetRequiredService<IDatabaseUpdater>();
        var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
        dbUpdater.DeployDatabaseChanges(loggerFactory, isDevEnvironment);
    }

    internal static List<Assembly> LoadDependencyAssemblies(DependencyContext dependencyContext)
    {
        var assemblies = new List<Assembly>();
        foreach (var compilationLibrary in dependencyContext.CompileLibraries)
        {
            if (IsCandidateCompilationLibrary(compilationLibrary))
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(compilationLibrary.Name));
                    assemblies.Add(assembly);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error on loading assemblies: " + e.Message);
                }
            }
        }

        return assemblies;
    }

    internal static bool IsCandidateCompilationLibrary(Library library)
    {
        return library.Name.Contains("ConnectorCore", StringComparison.InvariantCultureIgnoreCase);
    }

    internal static IEnumerable<IFeatureModule> GetFeatureModules()
    {
        var types = LoadDependencyAssemblies(DependencyContext.Default)
            .SelectMany(asm => asm.GetTypes())
            .Where(type => type.GetInterfaces().Contains(typeof(IFeatureModule)))
            .ToList();

        foreach (var type in types)
        {
            yield return Activator.CreateInstance(type) as IFeatureModule;
        }
    }

    internal static void ConfigureHealthChecks(IHealthChecksBuilder builder)
    {
        builder.AddSingletonCheck<HealthCheckSql>("SQL", tags: ["healthz"])
               .AddSingletonCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"]);
    }

    internal static void SetJsonSerialiserDefaults()
    {
        // Workaround: https://github.com/dotnet/runtime/issues/31094#issuecomment-543342051
        var jsonSerializerOptions = (JsonSerializerOptions)typeof(JsonSerializerOptions)
                                                          .GetField("s_defaultOptions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                                                         ?.GetValue(null);
        if (jsonSerializerOptions == null || jsonSerializerOptions.IsReadOnly)
        {
            return;
        }

        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        jsonSerializerOptions.Converters.Add(new DateTimeConverter());
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
}
