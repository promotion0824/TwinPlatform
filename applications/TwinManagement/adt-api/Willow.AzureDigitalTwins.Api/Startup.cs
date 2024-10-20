using Autofac;
using AutoMapper;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Willow.AppContext;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;
using Willow.AzureDataExplorer.Infra;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDataExplorer.Query;
using Willow.AzureDigitalTwins.Api.DataQuality;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Messaging.Handlers;
using Willow.AzureDigitalTwins.Api.Persistence;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Processors;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Services.Domain.Instance.Readers;
using Willow.AzureDigitalTwins.Api.Services.Hosted;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Api.TimeSeries;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Domain.Instance.Writers;
using Willow.AzureDigitalTwins.Services.Infra;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.CognitiveSearch;
using Willow.Copilot.ProxyAPI;
using Willow.Copilot.ProxyAPI.Extensions;
using Willow.DataQuality.Execution.Extensions;
using Willow.DataQuality.Model.Serialization;
using Willow.Exceptions;
using Willow.HealthChecks;
using Willow.Model.Requests;
using Willow.ServiceBus;
using Willow.ServiceBus.HostedServices;
using Willow.Storage;
using Willow.Storage.Blobs;
using Willow.Storage.Blobs.Options;
using Willow.Storage.Providers;
using Willow.Telemetry;
using Willow.Telemetry.Web;

namespace Willow.AzureDigitalTwins.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private CancellationTokenSource readinessCancellationTokenSource = new();
        private bool startupComplete = false;
        private readonly string applicationName;
        private readonly IWebHostEnvironment webHostEnvironment;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            webHostEnvironment = hostEnvironment;
            applicationName = _configuration.GetValue<string>("ApplicationInsights:CloudRoleName");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add Environment Specific Token Credentials
            var tokenCredOption = new TokenCredentialOptions();
            tokenCredOption.Retry.NetworkTimeout = TimeSpan.FromSeconds(3);
            TokenCredential tokenCredentials = webHostEnvironment.IsProduction() ?
                new ManagedIdentityCredential(options: tokenCredOption) :
                new AzureCliCredential();
            services.AddSingleton<TokenCredential>(tokenCredentials);

            services.AddControllers();
            services.AddDbContext(_configuration, tokenCredentials);
            services.AddDbContextForJobs(_configuration, tokenCredentials);

            services.AddOpenApiDocument(c => { c.Version = "v1"; c.Title = "Willow.AzureDigitalTwins.Api"; });

            // TODO: Unused code. ADT API no longer uses "SyncCacheMessageHandler" which only updates the cache from Twin events.
            // It is safe to remove only when all dependencies of this feature is completely removed.
            //services
            //    .AddOptions<CacheSyncTopic>()
            //    .BindConfiguration("ServiceBus:Topics:CacheSyncTopic");

            services
                .AddOptions<AcsSyncTopic>()
                .BindConfiguration("ServiceBus:Topics:AcsSyncTopic");

            services
                .AddOptions<AdxSyncTopic>()
                .BindConfiguration("ServiceBus:Topics:AdxSyncTopic");

            services
                .AddOptions<BlobStorageOptions>()
                .BindConfiguration("BlobStorage");

            services
                .AddOptions<StorageSettings>()
                .BindConfiguration("BlobStorage");

            if (!_configuration.GetSection("DocumentStorage").Exists())
            {
                services.AddOptions<DocumentStorageOptions>().BindConfiguration("BlobStorage");
                services.AddOptions<DocumentStorageSettings>().BindConfiguration("BlobStorage");
            }
            else
            {
                services.AddOptions<DocumentStorageOptions>().BindConfiguration("DocumentStorage");
                services.AddOptions<DocumentStorageSettings>().BindConfiguration("DocumentStorage");
            }

            var copilotSettings = _configuration.GetSection("Copilot").Get<CopilotSettings>();
            if (copilotSettings != null)
            {
                services.ConfigureCopilotClients(copilotSettings);
            }
            services.AddTransient(typeof(IOptionalDependency<>), typeof(OptionalDependency<>));

            // Configure ADX Settings
            services
                .AddOptions<AzureDataExplorerOptions>()
                .BindConfiguration("AzureDataExplorer");

            services.AddSingleton<HealthCheckServiceBus>();
            services.AddSingleton<HealthCheckADX>();
            services.AddSingleton<HealthCheckADT>();
            services.AddSingleton<HealthCheckSqlServer>();
            services.AddSingleton((c) => new HealthCheckFederated(
            new HealthCheckFederatedArgs(_configuration.GetValue<string>("Copilot:BaseAddress"), "/healthz", false),
            c.GetRequiredService<IHttpClientFactory>()));

            services.AddHealthChecks()
            .AddCheck<HealthCheckFederated>("Copilot", tags: ["healthz"])
            .AddCheck<HealthCheckADT>("ADT", tags: ["healthz"])
            .AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"])
            .AddCheck<HealthCheckADX>("ADX", tags: ["healthz"])
            .AddCheck<HealthCheckSearch>("ACS", tags: ["healthz"])
            .AddCheck<HealthCheckSqlServer>("SQL", tags: ["healthz"])
            .AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
            .AddCheck("readyz", () =>
            {
                readinessCancellationTokenSource.Token.ThrowIfCancellationRequested();
                return startupComplete ?
                HealthCheckResult.Healthy("System is ready.") :
                HealthCheckResult.Unhealthy("System not ready.");
            }, tags: ["readyz"]);

            services.AddOptions();
            services.AddMemoryCache();
            services.AddServiceBus(_configuration.GetSection("ServiceBus"));
            services.AddCors(options => options.AddDefaultPolicy(builder =>
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("*")));
            services.AddLogging();
            services.AddRuleCheckers();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(_configuration);//configSectionName: "AzureAd" - AzureAd Configuration section will get picked up by default


            services.AddAuthorization();

            services.RegisterCustomFormatters();

            services.AddMvc(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });
            AddAutoMapper(services);

            services.AddSingleton(sp =>
            {
                var willowContext = _configuration.GetSection("WillowContext").Get<WillowContextOptions>();
                return new Meter(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown");
            });

            var metricsAttributesHelper = new MetricsAttributesHelper(_configuration);
            services.AddSingleton(metricsAttributesHelper);

            services.AddSingleton<ITelemetryCollector, TelemetryCollector>();

            if (!string.IsNullOrWhiteSpace(applicationName))
            {
                //syncs to app insights using open telemetry
                services.AddWillowContext(_configuration);
            }

            // Todo: Remove DefaultAzureCredentials Deps from Cognitive Search SDK
            services.AddSingleton(new DefaultAzureCredential());
            services.AddAISearchCapabilities("AzureCognitiveSearch");

            services.AddGitHubRepositoryService();
        }

        private static void AddAutoMapper(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile<Mappings>();
            });
            mapperConfig.CompileMappings();

            var mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var azureDigitalTwinsSettings = _configuration.GetSection("AzureDigitalTwins").Get<AzureDigitalTwinsSettings>();

            builder.RegisterFilesReader(azureDigitalTwinsSettings);
            builder.RegisterDigitalTwinsService(azureDigitalTwinsSettings);

            builder.Register(x => new AdxSyncMessageHandler(
                x.Resolve<IOptions<AdxSyncTopic>>(),
                x.Resolve<IAzureDigitalTwinReader>(),
                x.Resolve<IExportService>(),
                x.Resolve<IAdxSetupService>(),
                x.Resolve<IOptions<AzureDataExplorerOptions>>(),
                x.Resolve<ILogger<AdxSyncMessageHandler>>(),
                x.Resolve<ILogger<TwinsChangeEventMessageHandlerBase>>(),
                x.Resolve<IConfiguration>(),
                x.Resolve<HealthCheckServiceBus>(), x.Resolve<ITelemetryCollector>())).As<ITopicMessageHandler>();
            builder.RegisterType<AcsSyncMessageHandler>().As<ITopicMessageHandler>();
            builder.RegisterType<MessageListenerBackgroundService>().As<IHostedService>();

            //Register Storage Services Client
            builder.RegisterType<BlobService>().As<IBlobService>();
            builder.RegisterType<DocumentBlobService>().As<IDocumentBlobService>();
            builder.RegisterType<DocumentService>().As<IDocumentService>();
            builder.RegisterType<TwinsService>().As<ITwinsService>();
            builder.RegisterType<CustomColumnService>().As<ICustomColumnService>();
            builder.RegisterGeneric(typeof(AsyncService<>)).As(typeof(IAsyncService<>));
            builder.RegisterType<StorageSasProvider>().As<IStorageSasProvider>();
            builder.RegisterType<BulkTwinProcessor>().As<IBulkProcessor<BulkImportTwinsRequest, BulkDeleteTwinsRequest>>();
            builder.RegisterType<BulkRelationshipProcessor>().As<IBulkProcessor<IEnumerable<BasicRelationship>, BulkDeleteRelationshipsRequest>>();
            builder.Register(x => new BulkModelProcessor(
                new AzureDigitalTwinReader(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<ILogger<AzureDigitalTwinReader>>()),
                new AzureDigitalTwinWriter(azureDigitalTwinsSettings.Instance, x.Resolve<TokenCredential>(), x.Resolve<IAzureDigitalTwinModelParser>(), x.Resolve<ILogger<AzureDigitalTwinWriter>>(), x.Resolve<IEnumerable<IAzureDigitalTwinValidator>>()),
                x.Resolve<ILogger<BulkModelProcessor>>(),
                x.Resolve<IAzureDigitalTwinModelParser>(),
                x.Resolve<IExportService>(),
                x.Resolve<IAzureDigitalTwinCacheProvider>(),
                x.Resolve<ITelemetryCollector>(),
                x.Resolve<IJobsService>()
                )).As<IBulkProcessor<BulkImportModels, BulkDeleteModelsRequest>>();
            builder.RegisterType<RuleTemplateSerializer>().As<IRuleTemplateSerializer>();
            builder.RegisterType<DQRuleService>().As<IDQRuleService>();
            builder.RegisterType<AdxSetupService>().As<IAdxSetupService>().SingleInstance();
            builder.RegisterType<ExportService>().As<IExportService>();

            // Register Mapping Service
            builder.RegisterType<MappingService>().As<IMappingService>();
            builder.RegisterType<MappedAsyncService>().As<IMappedAsyncService>();

            // Register Jobs Service
            builder.RegisterType<JobsService>().As<IJobsService>();

            builder.RegisterType<HealthService>().As<IHealthService>();
            builder.RegisterType<DataQualityAdxService>().As<IDataQualityAdxService>();
            builder.RegisterType<ImportService>().As<IBulkImportService>();
            builder.RegisterType<ClientBuilder>().As<IClientBuilder>();
            builder.RegisterType<AdxService>().As<IAdxService>();
            builder.RegisterType<AzureDataExplorerInfra>().As<IAzureDataExplorerInfra>();
            builder.RegisterType<AzureDataExplorerIngest>().As<IAzureDataExplorerIngest>();
            builder.RegisterType<AdxDataIngestionLocalStore>().As<IAdxDataIngestionLocalStore>().SingleInstance();
            builder.RegisterType<AzureDataExplorerCommand>().As<IAzureDataExplorerCommand>();
            builder.RegisterType<AzureDataExplorerQuery>().As<IAzureDataExplorerQuery>();
            builder.RegisterInstance(azureDigitalTwinsSettings);
            builder.RegisterType<AzureDigitalTwinEventRoute>().As<IAzureDigitalTwinEventRoute>();
            builder.RegisterType<RuleTemplateSerializer>().As<IRuleTemplateSerializer>();
            builder.RegisterType<TimeSeriesAdxService>().As<ITimeSeriesAdxService>();

            // Register Task Processor and Hosted Background service
            if (_configuration.GetSection(TimedBackgroundServiceOption.Name).Get<TimedBackgroundServiceOption>().Enabled)
            {
                // Add Hosted Job and Queue
                builder.RegisterType<TimedBackgroundService>().As<IHostedService>();
                builder.RegisterGeneric(typeof(BackgroundTaskQueue<>)).As(typeof(IBackgroundTaskQueue<>)).SingleInstance();

                // Add Jobs
                builder.RegisterType<TwinIncrementalScanJob>().As<IJobProcessor>();
                builder.RegisterType<TwinScanJob>().As<IJobProcessor>();
                builder.RegisterType<AcsFlushJob>().As<IJobProcessor>();
                builder.RegisterType<AdxFlushJob>().As<IJobProcessor>();
                builder.RegisterType<MarkSweepDocTwinsJob>().As<IJobProcessor>();
                builder.RegisterType<MarkSweepUnifiedJobCleanup>().As<IJobProcessor>();
                builder.RegisterType<TwinModelMigrationJob>().As<IJobProcessor>();
                builder.RegisterType<AdtImportJob>().As<IJobProcessor>();
                builder.RegisterType<DQTwinsValidationJob>().As<IJobProcessor>();
                builder.RegisterType<AdtToAdxExportJob>().As<IJobProcessor>();
                builder.RegisterType<TimeSeriesImportJob>().As<IJobProcessor>();

                // Add Twin Job Processors
                builder.RegisterType<TwinCustomColumnProcessor>().As<ITwinProcessor>();
                builder.RegisterType<TwinAdxSyncProcessor>().As<ITwinProcessor>();
                builder.RegisterType<TwinAcsSyncProcessor>().As<ITwinProcessor>();
                builder.RegisterType<DocTwinMetadataProcessor>().As<ITwinProcessor>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
#pragma warning disable S3168 // "async" methods should not return "void"
#pragma warning disable S107  // Methods should not have too many parameters: Disabling this
        public async void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime hostApplicationLifetime,
            IAzureDigitalTwinEventRoute azureDigitalTwinEventRoute,
            AzureDigitalTwinsSettings instanceSettings,
            ILogger<Startup> logger,
            IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
            IAdxSetupService adxSetupService,
            IDataQualityAdxService dqService,
            ITimeSeriesAdxService tsService,
            IDQRuleService rulesService,
            IAISearchIndexerSetupService searchIndexerSetupService)
#pragma warning restore S107  // Methods should not have too many parameters
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            var tasks = new List<Task>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOpenApi(); // serve OpenAPI/Swagger documents
            app.UseSwaggerUi(); // serve Swagger UI

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseWillowContext(_configuration);

            // Register the cancellation token
            hostApplicationLifetime.ApplicationStopping.Register(readinessCancellationTokenSource.Cancel);
            hostApplicationLifetime.ApplicationStopped.Register(readinessCancellationTokenSource.Cancel);
            // Use Willow Health Checks
            app.UseWillowHealthChecks(new HealthCheckResponse()
            {
                HealthCheckDescription = $"{applicationName} health",
                HealthCheckName = applicationName,
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            tasks.Add(SetupEventRoutesAsync(azureDigitalTwinEventRoute, instanceSettings, logger));
            tasks.Add(InitializeCacheAsync(logger, azureDigitalTwinCacheProvider, rulesService));
            tasks.Add(SetupAdxAsync(logger, adxSetupService, dqService, tsService));
            tasks.Add(MigrateDatabase(logger, _configuration));
            tasks.Add(searchIndexerSetupService.Setup());

            await Task.WhenAll(tasks);

            //await TriggerHealthCheck(logger, healthService);

            ConductLogTesting(logger);
            CheckCopilotSetting(logger);

            // At this point, app startup is considered complete. Readiness probe will return as Ready.
            startupComplete = true;
        }

        public void CheckCopilotSetting(ILogger logger)
        {
            var copilotSettings = _configuration.GetSection("Copilot").Get<CopilotSettings>();
            if (copilotSettings is null)
                logger.LogWarning("Copilot settings are not configured.");
        }

        public static async Task MigrateDatabase(ILogger logger, IConfiguration config)
        {
            try
            {
                logger.LogInformation("Begin Mapping Database migration");
                await new ContextFactory<MappingContext>(logger).MigrateAsync(config);
                logger.LogInformation("End Mapping Database migration");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on Mapping Database migration");
            }
            try
            {
                logger.LogInformation("Begin TwinsApi Database migration");
                await new ContextFactory<JobsContext>(logger).MigrateAsync(config);
                logger.LogInformation("End TwinsApi Database migration");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on TwinsApi Database migration");
            }
        }

        private static async Task SetupAdxAsync(ILogger<Startup> logger, IAdxSetupService adxSetupService, IDataQualityAdxService dqService, ITimeSeriesAdxService timeSeriesService)
        {
            try
            {
                logger.LogInformation("Setting up Adx");

                var initDataQualityAdxTask = dqService.InitDQAdxSettingsAsync();

                var initAdxTask = adxSetupService.InitializeAdxLazy();

                var initTimeSeriesAdxTask = timeSeriesService.InitAdxSettingsAsync();

                await Task.WhenAll(initAdxTask, initDataQualityAdxTask, initTimeSeriesAdxTask);

                logger.LogInformation("Done setting up Adx");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on ADX initialization");
            }
        }

        private static async Task InitializeCacheAsync(ILogger<Startup> logger, IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider, IDQRuleService rulesService)
        {
            try
            {
                var initRules = rulesService.InitializeValidationRules();

                var initCache = azureDigitalTwinCacheProvider.InitializeCache();

                await Task.WhenAll(initCache, initRules);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to initialize cache");
            }
        }

        private async Task SetupEventRoutesAsync(IAzureDigitalTwinEventRoute azureDigitalTwinEventRoute, AzureDigitalTwinsSettings instanceSettings, ILogger<Startup> logger)
        {
            try
            {
                var eventRouteFilterTypes = new List<EventRouteFilterType>
                {
                    EventRouteFilterType.TwinDelete,
                    EventRouteFilterType.RelationshipCreate,
                    EventRouteFilterType.RelationshipUpdate,
                    EventRouteFilterType.RelationshipDelete
                };

                // Fallback to event pipeline for adx sync of Twin Create/Update
                // if the hosted services are disabled
                if (!_configuration.GetSection(TimedBackgroundServiceOption.Name).Get<TimedBackgroundServiceOption>().Enabled)
                {
                    eventRouteFilterTypes.AddRange(new[] {
                        EventRouteFilterType.TwinCreate,
                        EventRouteFilterType.TwinUpdate });
                }

                logger.LogInformation($"Setting up {instanceSettings.Instance.EventRouteName} event route for {instanceSettings.Instance.EndpointName} endpoint.");
                var eventRoute = await azureDigitalTwinEventRoute.GetEventRouteAsync(instanceSettings.Instance.EventRouteName);
                if (eventRoute != null && eventRoute.Filter == azureDigitalTwinEventRoute.GetEventRouteFilterString(eventRouteFilterTypes))
                {
                    logger.LogInformation($"{instanceSettings.Instance.EventRouteName} event route already exists.");
                    return;
                }

                logger.LogInformation($"Creating / Updating {instanceSettings.Instance.EventRouteName} event route for {instanceSettings.Instance.EndpointName} endpoint.");
                await azureDigitalTwinEventRoute.CreateOrReplaceEventRouteAsync(instanceSettings.Instance.EventRouteName, instanceSettings.Instance.EndpointName, eventRouteFilterTypes);
                logger.LogInformation($"Done creating / Updating {instanceSettings.Instance.EventRouteName} event route for {instanceSettings.Instance.EndpointName} endpoint.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to setup event routes");
            }
        }

        private static void ConductLogTesting(ILogger logger)
        {
            logger.LogTrace("Startup - Log test: Log.Trace");
            logger.LogDebug("Startup - Log test: Log.Debug");
            logger.LogInformation("Startup - Log test: Log.Information");
            logger.LogWarning("Startup - Log test: Log.Warning");
            logger.LogError("Startup - Log test: Log.Error");
            logger.LogCritical("Startup - Log test: Log.Critical");
        }
    }
}
