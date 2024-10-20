using System;
using Azure.Identity;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Search;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Extensions;
using WillowRules.Services;

namespace RulesEngine.Init;

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
					ExcludeInteractiveBrowserCredential = true
					// Leaves only AZ CLI and Managed Identity
					// should disable AZ CLI also on Prod
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
				b.SQL.CacheExpiration = TimeSpan.FromMinutes(15);
				b.Execution = b.Execution ?? new ExecutionOption()
				{
					SettlingInterval = TimeSpan.FromMinutes(15),
					RunFrequency = TimeSpan.FromMinutes(15)
				};
			});
		services.AddOptions<CacheOptions>().BindConfiguration(CacheOptions.CONFIG);
		services.AddOptions<ServiceBusOptions>().BindConfiguration(ServiceBusOptions.CONFIG);
		services.AddOptions<RulesOptions>().BindConfiguration(RulesOptions.CONFIG);

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

		services.AddSingleton<ITelemetryInitializer, TelemetryInitializerForInit>();
		services.AddSingleton<ITelemetryInitializer, VersionTelemetryInitializerForInit>();
		services.AddTransient<ITelemetryCollector, TelemetryCollector>();

		// The following line enables Application Insights telemetry collection.
		services.AddApplicationInsightsTelemetry(x =>
		{
			//x.EnableDebugLogger = false;
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

		services.AddTransient<ISearchBuilderService, SearchBuilderService>();

		// Services which consume WillowEnvironment are transient
		services.AddTransient<IModelService, ModelService>();
		services.AddTransient<IMetaGraphService, MetaGraphService>();
		services.AddTransient<IADTService, ADTService>();
		services.AddTransient<IADTCacheService, ADTCacheService>();
		services.AddTransient<ITwinService, TwinService>();
		services.AddTransient<IFileService, FileService>();

		services.AddSingleton<IEventHubService, EventHubService>();
		services.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();

		services.AddTransient<ICommandInsightService, CommandInsightService>();
		services.AddTransient<ITwinGraphService, TwinGraphService>();
		services.AddTransient<ITwinSystemService, TwinSystemService>();
		services.AddSingleton<IRetryPolicies, RetryPolicies>();

		services.AddMemoryCache();

		services.AddSqlServerDistributedCache();

		services.AddHttpContextAccessor();

		services.AddTransient<WillowEnvironmentId, WillowEnvironmentId>((s) =>
		{
			var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
			var willowEnvironmentId = new WillowEnvironmentId(customerOptions.Value.Id);
			return willowEnvironmentId;
		});

		// ADX resolves to a file-based service if that's configured in options (used for debugging)
		services.AddAdxService();

		// Transient, new RulesContext per usage
		services.AddTransient<RulesContext, RulesContext>((s) =>
		{
			var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
			string sqlConnection = customerOptions.Value.SQL.ConnectionString;
			var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
				.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("WillowRules"))
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
				.EnableSensitiveDataLogging()
				.Options;
			return new RulesContext(dbContextOptions);
		});

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

		// Processor needs multiple instances of the RulesContext so that
		// each thread can operate independently
		services.AddDbContextFactory<RulesContext>(b =>
		{
			string sqlConnection = customerOptions.SQL.ConnectionString;
			b.UseSqlServer(sqlConnection, c => c.MigrationsAssembly("WillowRules").EnableRetryOnFailure())
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
				.EnableSensitiveDataLogging();
		});

		services.AddHttpClient();

		// Start the initializer to migrate the database
		services.AddHostedService<Initializer>();

		services.Configure<HostOptions>(hostOptions =>
		{
			// Do not stop the host when there is an unhandled exception
			hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
		});
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

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllerRoute(
				name: "default",
				pattern: "{controller}/{action=Index}/{id?}");
		});
	}
}
