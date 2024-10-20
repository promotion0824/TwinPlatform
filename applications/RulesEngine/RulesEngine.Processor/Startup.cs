using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RulesEngine.Processor.Services;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Api.Authentication;
using Willow.AppContext;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.CognitiveSearch;
using Willow.HealthChecks;
using Willow.Processor;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Search;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using Willow.Telemetry;
using Willow.Telemetry.Web;
using WillowRules.Extensions;
using WillowRules.Services;

namespace RulesEngine.Processor;

/// <summary>
/// Startup for the web app
/// </summary>
public class Startup
{
	/// <summary>
	/// Creates a new <see cref="Startup" />
	/// </summary>
	public Startup(IConfiguration configuration, IWebHostEnvironment environment)
	{
		Configuration = configuration;
		Environment = environment;
	}

	/// <summary>
	/// The configuration
	/// </summary>
	public IConfiguration Configuration { get; }

	/// <summary>
	/// The web hosting environment
	/// </summary>
	public IWebHostEnvironment Environment { get; set; }

	/// <summary>
	/// This method gets called by the runtime. Use this method to add services to the container.
	/// </summary>
	public void ConfigureServices(IServiceCollection services)
	{
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
					ExcludeAzureCliCredential = !Environment.IsDevelopment()
					// Leaves only AZ CLI and Managed Identity for Development
				});

		services.AddSingleton(defaultAzureCredential);

		// When the connection string contains managed identity this will be the provider
		// This allows local dev against an Azure SQL too now
		SqlAuthenticationProvider.SetProvider(
			SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
			new AzureSqlAuthProvider(defaultAzureCredential));

		services.AddOptions<CustomerOptions>().BindConfiguration(CustomerOptions.CONFIG)
			.Configure((b) =>
			{
				b.SQL.CacheExpiration = TimeSpan.FromMinutes(5);
				b.Execution = b.Execution ?? new ExecutionOption();
			});
		services.AddOptions<ServiceBusOptions>().BindConfiguration(ServiceBusOptions.CONFIG);
		services.AddOptions<RulesOptions>().BindConfiguration(RulesOptions.CONFIG);
		services.AddOptions<GitSyncOptions>().BindConfiguration(GitSyncOptions.CONFIG);

		// Used directly for DB config below
		var customerOptions = new CustomerOptions();
		Configuration.Bind(CustomerOptions.CONFIG, customerOptions);

		if (Environment.IsDevelopment())
		{
			IdentityModelEventSource.ShowPII = true;
		}

#if DEBUG
		services.AddDatabaseDeveloperPageExceptionFilter();
#endif

		services.AddSingleton<ITelemetryInitializer, TelemetryInitializerForProcessor>();
		services.AddSingleton<ITelemetryInitializer, VersionTelemetryInitializerForProcessor>();
		services.AddSingleton<ITelemetryCollector>(s =>
		{
			var willowContext = Configuration.GetSection("WillowContext").Get<WillowContextOptions>();
			var metricsAttributesHelper = new MetricsAttributesHelper(Configuration);
			var meterName = willowContext.MeterOptions.Name;
			var meterVersion = willowContext.MeterOptions.Version;
			var logger = s.GetRequiredService<ILogger<TelemetryCollector>>();

			logger.LogInformation("Meter name is {meter}", meterName);

			return new TelemetryCollector(meterName, meterVersion, new TagList(metricsAttributesHelper.GetValues()), s.GetRequiredService<IMemoryCache>());
		});

		// Causes errors trying to get pods services.AddApplicationInsightsKubernetesEnricher();

		services.AddControllers().AddNewtonsoftJson(c =>
		{
			c.SerializerSettings.Converters.Add(new TokenExpressionJsonConverter());
		});

		// See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0
		services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
		});

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo { Title = "WillowRules", Version = "v1" });

			c.AddSecurityDefinition("ADB2C", new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Description = "Bearer scheme. Example: \"bearer {token}\". Use browser dev tools to copy from a logged-in session.",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey
			});

			c.IncludeXmlComments(
				System.IO.Path.Combine(System.AppContext.BaseDirectory,
				$"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"),
				includeControllerXmlComments: true);

			//c.OperationFilter<SecurityRequirementsOperationFilter>();  // NOSONAR, this is here to remind me
		});

		// Options for calling MSGraph to get user information
		//services.AddOptions<ADB2COptions>().BindConfiguration(ADB2COptions.CONFIG);

		services.AddLogging();
		services.AddHttpClient();

		services.AddSingleton<RuleTemplateRegistry>();
		services.AddSingleton<IDataCacheFactory, DataCacheFactory>();

		services.AddTransient<IRulesSearchBuilderService, RulesSearchBuilderService>();

		// Services which consume WillowEnvironment are transient
		services.AddTransient<IModelService, ModelService>();
		services.AddTransient<IMetaGraphService, MetaGraphService>();
		services.AddTransient<IADTService, ADTService>();
		services.AddTransient<IADTCacheService, ADTCacheService>();
		services.AddTransient<ITwinService, TwinService>();
		services.AddTransient<IFileService, FileService>();
		services.AddTransient<IMLService, MLService>();

		services.AddSingleton<IGitService, GitService>();

		services.AddSingleton<IEventHubService, EventHubService>();
		services.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();

		services.AddSingleton<IDataQualityService, DataQualityService>();
		services.AddSingleton<ICalculatedPointsService, CalculatedPointsService>();

		services.AddSingleton<ICommandInsightService, CommandInsightService>();
		services.AddSingleton<ICommandService, CommandService>();
		services.AddTransient<ITwinGraphService, TwinGraphService>();
		services.AddTransient<ITwinSystemService, TwinSystemService>();
		services.AddSingleton<IRetryPolicies, RetryPolicies>();

		services.AddMemoryCache();

		services.AddClientCredentialToken(Configuration);
		services.AddTransient<AuthenticationDelegatingHandler>();

		static Action<HttpClient> ConfigureADTClient(CustomerOptions customerOptions) => client =>
		{
			var adtApiOption = customerOptions.AdtApi;

			var scope = $"{adtApiOption.Audience}/.default";
			client.BaseAddress = new Uri(adtApiOption.Uri ?? "");
			client.DefaultRequestHeaders.Add("Scope", scope);
		};

		if (!string.IsNullOrWhiteSpace(customerOptions.AdtApi?.Uri) && !string.IsNullOrWhiteSpace(customerOptions.AdtApi?.Audience))
		{
			services.AddHttpClient<ITwinsClient, TwinsClient>(ConfigureADTClient(customerOptions)).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
			services.AddHttpClient<IRelationshipsClient, RelationshipsClient>(ConfigureADTClient(customerOptions)).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
			services.AddHttpClient<IDQCapabilityClient, DQCapabilityClient>(ConfigureADTClient(customerOptions)).AddHttpMessageHandler<AuthenticationDelegatingHandler>();
		}
		services.AddSingleton<IADTApiService, ADTApiService>();

		services.AddSqlServerDistributedCache();

		services.AddHttpContextAccessor();

		services.AddTransient<WillowEnvironmentId, WillowEnvironmentId>((s) =>
		{
			var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
			var willowEnvironmentId = new WillowEnvironmentId(customerOptions.Value.Id);
			return willowEnvironmentId;
		});

		// Map from RulesOptions to the SearchSettings used in Willow.CognitiveSearch
		services.AddOptions<AISearchSettings>()
			.Configure<WillowEnvironment, IOptions<RulesOptions>>((options, w, r) =>
			{
				options.UnifiedIndexName = r.Value.SearchApi.IndexName ?? w.Id;
				options.Uri = r.Value.SearchApi.Uri;
			});

		// ADX resolves to a file-based service if that's configured in options (used for debugging)
		services.AddAdxService();

		services.AddSingleton<IEpochTracker, EpochTracker>();

		// Register all the repositories as transient (they may be requested more than once)
		services.AddTransient<IRepositoryGlobalVariable, RepositoryGlobalVariable>();
		services.AddTransient<IRepositoryCommand, RepositoryCommand>();
		services.AddTransient<IRepositoryInsight, RepositoryInsight>();
		services.AddTransient<IRepositoryRuleInstanceMetadata, RepositoryRuleInstanceMetadata>();
		services.AddTransient<IRepositoryRuleInstances, RepositoryRuleInstances>();
		services.AddTransient<IRepositoryCalculatedPoint, RepositoryCalculatedPoint>();
		services.AddTransient<IRepositoryRules, RepositoryRules>();
		services.AddTransient<IRepositoryRuleMetadata, RepositoryRuleMetadata>();
		services.AddTransient<IRepositoryRuleExecutions, RepositoryRuleExecutions>();
		services.AddTransient<IRepositoryADTSummary, RepositoryADTSummary>();
		services.AddTransient<IRepositoryProgress, RepositoryProgress>();
		services.AddTransient<IRepositoryActorState, RepositoryActorState>();
		services.AddTransient<IRepositoryTimeSeriesBuffer, RepositoryTimeSeriesBuffer>();
		services.AddTransient<IRepositoryTimeSeriesMapping, RepositoryTimeSeriesMapping>();
		services.AddTransient<IRepositoryRuleTimeSeriesMapping, RepositoryRuleTimeSeriesMapping>();
		services.AddTransient<IRepositoryRuleExecutionRequest, RepositoryRuleExecutionRequest>();
		services.AddTransient<IRepositoryInsightChange, RepositoryInsightChange>();
		services.AddTransient<IRepositoryLogEntry, RepositoryLogEntry>();
		services.AddTransient<IRepositoryMLModel, RepositoryMLModel>();

		services.AddRulesDBContext();

		// // ugh, aspnet core DI can't handle 'AsImplementedInterfaces' like autofac can
		// // instead must manually re-register each like this:
		// services.AddSingleton<IMessageHandler>((c) => c.GetRequiredService<IHeartBeatTracker>());
		// services.AddSingleton<IMessageHandler>((c) => c.GetRequiredService<IRuleUpdateTracker>());

		// services.AddHostedService<MessageConsumer>();

		// No CORS, API only calls from Kubernetes
		// services.AddCors(options =>
		// {
		// 	options.AddDefaultPolicy(
		// 		builder =>
		// 		{
		// 			builder.WithOrigins("http://localhost:3000", "http://localhost:5050")
		// 				.AllowAnyHeader()
		// 				.AllowAnyMethod()
		// 				.AllowCredentials();
		// 		});
		// });

		services.AddSingleton<WillowEnvironment, WillowEnvironment>((s) =>
		{
			var ep = s.GetRequiredService<IEnvironmentProvider>();
			var we = ep.Create();
			return we;
		});

		services.AddHttpClient();

		services.AddSingleton<IEpochTracker, EpochTracker>();


		services.AddTransient<IMessageSenderBackEnd, MessageSenderBackEnd>();

		services.AddTransient<ITwinGraphService, TwinGraphService>();
		services.AddTransient<ITwinSystemService, TwinSystemService>();
		services.AddTransient<IValidationService, ValidationService>();
		services.AddSingleton<TagService>();
		services.AddTransient<SqlRulesService>();
		services.AddTransient<IRulesService, RulesService>();

		services.AddSingleton<RuleTemplateRegistry>();

		services.AddTransient<IModelService, ModelService>();
		services.AddTransient<ILoadMemoryGraphService, LoadMemoryGraphService>();

		services.AddTransient<IRuleInstanceProcessor, RuleInstanceProcessor>();
		services.AddTransient<IRuleExecutionProcessor, RuleExecutionProcessor>();
		services.AddTransient<ICommandSyncProcessor, CommandSyncProcessor>();
		services.AddTransient<IGitSyncProcessor, GitSyncProcessor>();
		services.AddTransient<ICalculatedPointsProcessor, CalculatedPointsProcessor>();
		services.AddTransient<IDiagnosticsProcessor, DiagnosticsProcessor>();
		services.AddTransient<IRulesManager, RulesManager>();
		services.AddTransient<IActorManager, ActorManager>();
		services.AddSingleton<ITimeSeriesManager, TimeSeriesManager>();
		services.AddSingleton<IInsightsManager, InsightsManager>();
		services.AddSingleton<ICommandsManager, CommandsManager>();

		services.AddTransient<IRuleInstancesService, RuleInstancesService>();
		services.AddTransient<IMetaGraphService, MetaGraphService>();

		services.AddMemoryCache();

		services.AddSqlServerDistributedCache();

		// Request handler is the glue between the two hosted services
		services.AddTransient<IRulesEngineRequestHandler, RulesEngineRequestHandler>();
		services.AddTransient<IMessageHandler>((s) => s.GetRequiredService<IRulesEngineRequestHandler>());

		// Internal message bus
		services.AddSingleton<IRuleOrchestrator, RuleOrchestrator>();
		services.AddSingleton<IGitSyncOrchestrator, GitSyncOrchestrator>();

		// Invokes realtime execution
		services.AddHostedService<RealtimeExecutionBackgroundService>();

		//Grabs execution requests from a DB queue
		services.AddHostedService<ProcessorQueueServiceBackgroundService>();

		// This is the main application, listening to service bus messages and acting on them
		services.AddHostedService<MessageConsumer>();
		services.AddHostedService<EventHubBackgroundService>();
		services.AddHostedService<DataQualityBackgroundService>();
		services.AddHostedService<CommandBackgroundService>();
		services.AddHostedService<InsightBackgroundService>();
		services.AddHostedService<ADTCacheSyncService>();

		// Git sync background service
		services.AddHostedService<GitSyncExecutionService>();

		services.Configure<HostOptions>(hostOptions =>
		{
			// Do not stop the host when there is an unhandled exception
			hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
		});

		services.AddSingleton<HealthCheckProcessor>();
		services.AddSingleton<HealthCheckCalculatedPoints>();
		services.AddSingleton<HealthCheckGitSync>();
		services.AddSingleton<HealthCheckPublicAPI>();
		services.AddSingleton<HealthCheckCommandApi>();
		services.AddSingleton<HealthCheckServiceBus>();
		services.AddSingleton<HealthCheckKeyVault>();
		services.AddSingleton<HealthCheckSearch>();
		services.AddSingleton<HealthCheckADX>();
		services.AddSingleton<HealthCheckADT>();
		services.AddSingleton<HealthCheckADTApi>();
		services.AddHostedService<StartupHealthCheckService>();

		// Syncs to Application Insights using open telemetry
		services.AddWillowContext(Configuration);

		services.AddHealthChecks()
			.AddCheck<HealthCheckProcessor>("Processor runtime", tags: ["healthz"])
			.AddCheck<HealthCheckPublicAPI>("Public API", tags: ["healthz"])
			.AddCheck<HealthCheckCommandApi>("Command and Control", tags: ["healthz"])
			.AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"])
			.AddCheck<HealthCheckKeyVault>("Key Vault", tags: ["healthz"])
			.AddCheck<HealthCheckSearch>("Search", tags: ["healthz"])
			.AddCheck<HealthCheckADX>("ADX", tags: ["healthz"])
			.AddCheck<HealthCheckADT>("ADT", tags: ["healthz"])
			.AddCheck<HealthCheckCalculatedPoints>("CalculatedPoints", tags: ["healthz"])
			.AddCheck<HealthCheckGitSync>("Git Sync", tags: ["healthz"])
			.AddCheck<HealthCheckADTApi>("ADTApi", tags: ["healthz"]);

	}

	/// <summary>
	/// Configures the HTTP request pipeline (called by runtime)
	/// </summary>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		// See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0
		app.UseForwardedHeaders();

		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseRouting();

		app.UseCors();

		// app.UseAuthentication();
		// app.UseAuthorization();

		app.UseSwagger();
		// app.UseSwaggerUI(c =>
		// {
		// 	var config = app.ApplicationServices.GetRequiredService<IOptions<ADB2COptions>>();
		// 	string baseurl = config.Value.BaseUrl;
		// 	c.SwaggerEndpoint(baseurl.TrimEnd('/') + "/swagger/v1/swagger.json", "Willow Rules v1");
		// });

		app.UseWillowHealthChecks(new WebHealthCheckResponse()
		{
			HealthCheckDescription = "Activate Technology Processor",
			HealthCheckName = "Activate Technology Processor",
		});

		// Turn off noisy Kusto logging
		Kusto.Cloud.Platform.Utils.TraceSourceManager.SetTraceVerbosityForAll(Kusto.Cloud.Platform.Utils.TraceVerbosity.Fatal);
	}

	private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

	private class WebHealthCheckResponse : HealthCheckResponse
	{
		public override Task WriteHealthZResponse(HttpContext context, HealthReport healthReport)
		{
			context.Response.ContentType = "application/json; charset=utf-8";
			var slimResult = new HealthCheckDto("Activate Technology Processor", "Processor health", healthReport);
			string json = JsonConvert.SerializeObject(slimResult, jsonSettings);
			return context.Response.WriteAsync(json);
		}
	}
}
