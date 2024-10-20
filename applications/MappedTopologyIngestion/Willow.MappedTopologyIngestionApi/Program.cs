namespace Willow.MappedTopologyIngestionApi;

using System.Diagnostics.Metrics;
using Asp.Versioning;
using Azure.Identity;
using global::HealthChecks.AzureServiceBus;
using global::HealthChecks.AzureServiceBus.Configuration;
using Mapped.Ontologies.Mappings.OntologyMapper;
using Mapped.Ontologies.Mappings.OntologyMapper.Mapped;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Willow.Api.Authentication;
using Willow.AppContext;
using Willow.HealthChecks;
using Willow.MappedTopologyIngestionApi.HealthChecks;
using Willow.Security.KeyVault;
using Willow.ServiceBus;
using Willow.ServiceBus.HostedServices;
using Willow.ServiceBus.Options;
using Willow.Telemetry;
using Willow.Telemetry.Web;
using Willow.TopologyIngestion;
using Willow.TopologyIngestion.Extensions;
using Willow.TopologyIngestion.Interfaces;
using Willow.TopologyIngestion.Mapped;

/// <summary>
/// The Program to run the MappedTopologyIngestionApi.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">A list of arguments for application startup.</param>
    /// <exception cref="ArgumentNullException">Thrown if the KeyVaultName setting is not found at application startup.</exception>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

        builder.Configuration.AddJsonFile("appsettings.json", optional: false);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddJsonFile("appsettings.Development.json");
        }

        builder.Configuration.AddEnvironmentVariables();

        var settings = builder.Configuration;

        var vaultName = settings["Azure:KeyVault:KeyVaultName"];

        if (string.IsNullOrWhiteSpace(vaultName))
        {
            Console.WriteLine("Startup Variable 'Azure:KeyVault:KeyVaultName' not found. Exiting");
            throw new ArgumentNullException("Azure:KeyVault:KeyVaultName");
        }

        var vaultUri = $"https://{vaultName}.vault.azure.net/";
        builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());

        builder.Services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersion.Default;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                               new HeaderApiVersionReader("api-version"));
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        // Add services to the container.
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "V1", Version = "v1" });
            c.SwaggerDoc("v2", new OpenApiInfo { Title = "V2", Version = "v2" });

            c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
        });

        var defaultAzureCredential =
            new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeVisualStudioCredential = true,
                    ExcludeSharedTokenCacheCredential = false,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeEnvironmentCredential = true,
                    ExcludeInteractiveBrowserCredential = true,
                });

        builder.Services.AddSingleton(defaultAzureCredential);

        // See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        builder.Services.AddWillowContext(builder.Configuration);

        // This semaphore is used to ensure the SecretManager is only accessed by one thread at a time
        (object key, Semaphore semaphore) = SecretManager.GetKeyedSingletonDependencies();
        builder.Services.AddKeyedSingleton(key, semaphore);

        builder.Services.AddAzureClients(clientBuilder =>
        {
            var vaultName = builder.Configuration["Azure:KeyVault:KeyVaultName"];
            var vaultUri = $"https://{vaultName}.vault.azure.net/";

            // Register clients for each service
            clientBuilder.AddSecretClient(new Uri(vaultUri));
        });

        builder.Services.AddSingleton<ISecretManager, SecretManager>();

        builder.Services.AddOptions<MtiOptions>().Bind(builder.Configuration.GetSection("MtiOptions"));

        builder.Services.AddMemoryCache();
        builder.Services.AddClientCredentialToken(builder.Configuration);
        builder.Services.AddTransient<AuthenticationDelegatingHandler>();
        builder.Services.AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = builder.Configuration["Issuer"];
                options.Audience = builder.Configuration["Audience"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true;
            });

        // Implements IInputGraphManager, IGraphIngestionProcessor, IOutputGraphManager, ITelemetryIngestionProcessor
        builder.Services.AddMappedIngestionManager(options =>
        {
            // Mapped Specific
            options.MappedRootUrl = builder.Configuration["MappedRootUrl"] ?? throw new NullReferenceException("MappedRootUrl");
            options.EnableUpdates = builder.Configuration["EnableUpdates"] != null ? builder.Configuration.GetValue<bool>("EnableUpdates") : true;
            options.ThingQueryBatchSize = builder.Configuration["ThingQueryBatchSize"] != null ? builder.Configuration.GetValue<int>("ThingQueryBatchSize") : 25;
            options.EnableResetExternalId = builder.Configuration["EnableResetExternalId"] != null ? builder.Configuration.GetValue<bool>("EnableResetExternalId") : false;
            options.EnableTwinReplace = builder.Configuration["EnableTwinReplace"] != null ? builder.Configuration.GetValue<bool>("EnableTwinReplace") : false;

            // Ingestion Manager
            options.AdtApiEndpoint = builder.Configuration["TwinsApi:BaseUrl"] ?? throw new NullReferenceException("TwinsApi:BaseUrl");
            options.Audience = builder.Configuration["TwinsApi:Audience"] ?? throw new NullReferenceException("TwinsApi:Audience");
        });

        builder.Services.AddSingleton<IOntologyMappingLoader>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<MappedHttpOntologyMappingLoader>>();
            return new MappedHttpOntologyMappingLoader(logger, builder.Configuration["ontologyMappingUrl"] ?? throw new NullReferenceException("ontologyMappingUrl"));
        });

        builder.Services.AddServiceBus(builder.Configuration.GetSection("ServiceBus"));
        builder.Services.AddHostedService<MessageListenerBackgroundService>();
        builder.Services.AddTransient<IQueueMessageHandler, SyncMessageHandler>();
        builder.Services.AddSingleton<ITwinMappingIndexer, NullTwinMappingIndexer>();
        builder.Services.AddSingleton<IOntologyMappingManager, OntologyMappingManager>();
        builder.Services.AddTransient<IGraphIngestionProcessor, WillowGraphIngestionProcessor<MappedIngestionManagerOptions>>();
        builder.Services.AddSingleton<IGraphNamingManager, WillowGraphNamingManager>();

        builder.Services.AddSingleton(sp =>
        {
            var willowContext = builder.Configuration.GetSection("WillowContext").Get<WillowContextOptions>();
            return new Meter(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown");
        });

        var metricsAttributesHelper = new MetricsAttributesHelper(builder.Configuration);
        builder.Services.AddSingleton(metricsAttributesHelper);

        builder.Services.AddSingleton<HealthCheckTwinsApi>();
        builder.Services.AddSingleton<HealthCheckMappedApi>();
        builder.Services.AddSingleton<HealthCheckServiceBus>();
        builder.Services.AddSingleton(o =>
        {
            var azureServiceBusQueueHealthCheckOptions = builder.Configuration.GetSection("AzureServiceBusQueueHealthCheck").Get<AzureServiceBusQueueHealthCheckOptions>();
            if (azureServiceBusQueueHealthCheckOptions == null)
            {
                throw new ArgumentNullException("AzureServiceBusQueueHealthCheckOptions");
            }

            var serviceBusOptions = builder.Configuration.GetSection("ServiceBus").Get<ServiceBusOptions>();

            if (serviceBusOptions == null)
            {
                throw new ArgumentNullException("ServiceBusOptions");
            }

            azureServiceBusQueueHealthCheckOptions.Credential = defaultAzureCredential;
            azureServiceBusQueueHealthCheckOptions.FullyQualifiedNamespace ??= serviceBusOptions.Namespaces["CustomerServiceBus"];

            return new AzureServiceBusQueueHealthCheck(azureServiceBusQueueHealthCheckOptions);
        });

        builder.Services.AddHostedService<StartupHealthCheckService>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<HostOptions>(hostOptions =>
        {
            // Do not stop the host when there is an unhandled exception
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        builder.Services.AddSingleton<IHealthCheckPublisher>(hcp =>
        {
            return new HealthCheckPublisher("MappedTopologyIngestion");
        });

        var cancellationTokenSource = new CancellationTokenSource();

        builder.Services.AddHealthChecks()
            .AddCheck<AzureServiceBusQueueHealthCheck>("Service Bus Queue", tags: ["healthz"])
            .AddCheck<HealthCheckTwinsApi>("Twins Api", tags: ["healthz"])
            .AddCheck<HealthCheckMappedApi>("Mapped Api", tags: ["healthz"])
            .AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"])
            .AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
            .AddCheck("readyz",
                      () =>
                      {
                          cancellationTokenSource.Token.ThrowIfCancellationRequested();
                          return HealthCheckResult.Healthy("System is ready.");
                      },
                      tags: ["readyz"]);

        var app = builder.Build();

        app.Lifetime.ApplicationStopping.Register(cancellationTokenSource.Cancel);
        app.Lifetime.ApplicationStopped.Register(cancellationTokenSource.Cancel);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Willow.MappedTopologyIngestionApi V1");
                c.SwaggerEndpoint($"/swagger/v2/swagger.json", "Willow.MappedTopologyIngestionApi V2");
            });
        }

        // See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0
        app.UseForwardedHeaders();

        app.UseAuthorization();

        app.MapControllers();

        app.UseRouting();

        app.UseWillowContext(builder.Configuration);

        var healthCheckResponse = new HealthCheckResponse()
        {
            HealthCheckDescription = "MTI Health",
            HealthCheckName = "Mapped Topology Ingestion",
        };

        app.UseWillowHealthChecks(healthCheckResponse);

        if (builder.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.Run();
    }
}
