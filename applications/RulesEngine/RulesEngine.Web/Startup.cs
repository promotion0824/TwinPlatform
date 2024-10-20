using Authorization.TwinPlatform.Common.Extensions;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RulesEngine.Processor.Services;
using RulesEngine.Web;
using RulesEngine.Web.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Willow.AppContext;
using Willow.CognitiveSearch;
using Willow.HealthChecks;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Options;
using Willow.Rules.Repository;
using Willow.Rules.Search;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using Willow.Telemetry;
using Willow.Telemetry.Web;
using WillowRules.Extensions;
using WillowRules.Services;

namespace Willow.Rules.Web;

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
    /// Azure credentials
    /// </summary>
    public static DefaultAzureCredential AzureCredentials(IHostEnvironment env)
    {
        return new DefaultAzureCredential(
                new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeVisualStudioCredential = true,
                    ExcludeSharedTokenCacheCredential = false,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeEnvironmentCredential = true,
                    ExcludeInteractiveBrowserCredential = true,
                    ExcludeAzureCliCredential = !env.IsDevelopment()
                    // Leaves only AZ CLI and Managed Identity for Development
                });
    }
    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        var defaultAzureCredential = AzureCredentials(Environment);

        services.AddSingleton(defaultAzureCredential);

        // When the connection string contains managed identity this will be the provider
        // This allows local dev against an Azure SQL too now
        SqlAuthenticationProvider.SetProvider(
            SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
            new AzureSqlAuthProvider(defaultAzureCredential));

        if (Environment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }

        var jwtBearerConfig = new JwtBearerConfig();
        Configuration.Bind(JwtBearerConfig.CONFIG, jwtBearerConfig);

        var b2cConfig = new B2cConfig();
        Configuration.Bind(B2cConfig.CONFIG, b2cConfig);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer()
            .AddMicrosoftIdentityWebApi(//Configuration,
            (JwtBearerOptions jwtOptions) =>
            {
                jwtOptions.Authority = jwtBearerConfig.Authority;
                jwtOptions.Audience = jwtBearerConfig.Audience;
                //jwtOptions.Configuration.Issuer = jwtBearerConfig.Issuer;
            },
            (MicrosoftIdentityOptions identityoptions) =>
            {
                identityoptions.Instance = b2cConfig.Instance;
                identityoptions.Domain = b2cConfig.Domain;
                identityoptions.ClientId = b2cConfig.ClientId;
                identityoptions.ClientSecret = b2cConfig.ClientSecret;
                identityoptions.Authority = b2cConfig.Authority;
                identityoptions.TenantId = b2cConfig.TenantId;
            },
            "Bearer", // JwtBearerDefaults.AuthenticationScheme,
            /* diagnostics */ true);


        //TODO this check should be removed once the AD group fallback is not needed anymore
        if (!string.IsNullOrWhiteSpace(Configuration.GetValue<string>("AuthorizationAPI:BaseAddress")) &&
            !string.IsNullOrWhiteSpace(Configuration.GetValue<string>("AuthorizationAPI:TokenAudience")))
        {
            // Registers the IUserAuthorizationService
            services.AddUserManagementCoreServices(Configuration.GetSection("AuthorizationAPI"));
        }

        services.AddAuthorization(options =>
        {
            // This maps requirements by name to requirement class instances
            options.AddAuthPolicies();

            //
            options.InvokeHandlersAfterFailure = false;
        });

        // Register all of the requirements
        services.AddAuthPolicies();

        //the InstanceType setting comes from the infra pulumi setup
        services.AddRoles(Configuration.GetValue<string>("AuthorizationAPI:InstanceType"));

#if DEBUG
        services.AddDatabaseDeveloperPageExceptionFilter();
#endif

        services.AddSingleton<ITelemetryCollector>(s =>
        {
            var metricsAttributesHelper = new MetricsAttributesHelper(Configuration);
            var meterName = "RulesEngine.Web";
            var meterVersion = "v1";
            return new TelemetryCollector(meterName, meterVersion, new TagList(metricsAttributesHelper.GetValues()), s.GetRequiredService<IMemoryCache>());
        });

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
            c.SwaggerDoc("health", new OpenApiInfo { Title = "Health", Version = "health" });
            c.OperationFilter<FileResultContentTypeOperationFilter>();

            c.AddSecurityDefinition("ADB2C", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Bearer scheme. Example: \"bearer {token}\". Use browser dev tools to copy from a logged-in session.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                //The name of the HTTP Authorization scheme to be used in the Authorization header.
                Scheme = "bearer"
            });

            //After defining the security scheme, apply it by adding it as a security requirement.
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "ADB2C" //The name of the defined security scheme above.
							}
                        }, new List<string>()
                    }
            });

            c.IncludeXmlComments(
                System.IO.Path.Combine(System.AppContext.BaseDirectory,
                $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml"),
                includeControllerXmlComments: true);

            //c.OperationFilter<SecurityRequirementsOperationFilter>();  // NOSONAR, this is here to remind me
        });

        // In production, the React files will be served from this directory
        services.AddSpaStaticFiles(configuration =>
        {
            // dist is where Vite puts the built files
            configuration.RootPath = "ClientApp/dist";
        });

        services.AddOptions<CustomerOptions>().BindConfiguration(CustomerOptions.CONFIG)
        .Configure((b) =>
        {
            b.SQL.CacheExpiration = TimeSpan.FromMinutes(5);
        });
        services.AddOptions<RulesOptions>().BindConfiguration(RulesOptions.CONFIG);

        // Web site uses a single ServiceBus configuration (not per-customer)
        // Sends messages on one topic and then individual processors get just the messages intended for them
        services.AddOptions<ServiceBusOptions>().BindConfiguration(ServiceBusOptions.CONFIG);

        // Options for calling MSGraph to get user information
        services.AddOptions<ADB2COptions>().BindConfiguration(ADB2COptions.CONFIG);

        // Options for calling service-to-service
        services.AddOptions<ServicesConfiguration>().BindConfiguration(ServicesConfiguration.CONFIG);

        services.AddLogging();
        services.AddHttpClient();

        services.AddSingleton<RuleTemplateRegistry>();
        services.AddSingleton<IDataCacheFactory, DataCacheFactory>();
        //TimeSeriesManager is only used by simulation. Make transient so that we don't keep track of timeseries objects from past simulations
        services.AddTransient<ITimeSeriesManager, TimeSeriesManager>();
        services.AddTransient<IMLService, MLService>();

        // Map from RulesOptions to the SearchSettings used in Willow.CognitiveSearch
        services.AddOptions<AISearchSettings>()
            .Configure<WillowEnvironment, IOptions<RulesOptions>>((options, w, r) =>
            {
                options.UnifiedIndexName = r.Value.SearchApi.IndexName ?? w.Id;
                options.Uri = r.Value.SearchApi.Uri;
            });

        services.AddTransient<ISearchService, SearchService>();
        services.AddTransient<IRulesSearchBuilderService, RulesSearchBuilderService>();

        // Services which consume WillowEnvironment are transient
        services.AddTransient<IModelService, ModelService>();
        services.AddTransient<IMetaGraphService, MetaGraphService>();
        services.AddTransient<IADTService, ADTService>();
        services.AddTransient<IADTCacheService, ADTCacheService>();
        services.AddTransient<ITwinService, TwinService>();
        services.AddTransient<IFileService, FileService>();
        services.AddSingleton<IEnvironmentProvider, EnvironmentProvider>();

        services.AddSingleton<ICommandInsightService, CommandInsightService>();
        services.AddSingleton<ICommandService, CommandService>();
        services.AddTransient<ITwinGraphService, TwinGraphService>();
        services.AddTransient<ITwinSystemService, TwinSystemService>();
        services.AddSingleton<IRetryPolicies, RetryPolicies>();

        services.AddSingleton<TagService>();
        services.AddTransient<SqlRulesService>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IPolicyDecisionService, PolicyDecisionService>();

        services.AddMemoryCache();

        services.AddSqlServerDistributedCache();

        services.AddHttpContextAccessor();

        services.AddSingleton<WillowEnvironmentId, WillowEnvironmentId>((s) =>
        {
            var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
            var willowEnvironmentId = new WillowEnvironmentId(customerOptions.Value.Id);
            return willowEnvironmentId;
        });

        services.AddSingleton<WillowEnvironment, WillowEnvironment>((s) =>
        {
            var ep = s.GetRequiredService<IEnvironmentProvider>();
            var we = ep.Create();
            return we;
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

        services.AddTransient<IRulesService, RulesService>();
        services.AddTransient<IRuleSimulationService, RuleSimulationService>();

        services.AddRulesDBContext();

        services.AddSingleton<IMessageSenderFrontEnd, MessageSenderFrontEnd>();

        // Service bus receiver runs as a hosted service so
        // it's always listening even before first web page request
        services.AddSingleton<IHeartBeatTracker, HeartBeatTracker>();
        services.AddSingleton<IRuleUpdateTracker, RuleUpdateTracker>();

        // ugh, aspnet core DI can't handle 'AsImplementedInterfaces' like autofac can
        // instead must manually re-register each like this:
        services.AddSingleton<IMessageHandler>((c) => c.GetRequiredService<IHeartBeatTracker>());
        services.AddSingleton<IMessageHandler>((c) => c.GetRequiredService<IRuleUpdateTracker>());

        services.AddHostedService<MessageConsumer>();

        // On development server allow NPM serve website to call backend running in IDE on 5050
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.WithOrigins("http://localhost:3000", "http://localhost:5050")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        // Syncs to Application Insights using open telemetry
        services.AddWillowContext(Configuration);

        services.AddProblemDetails();

        services.AddSingleton<HealthCheckAuthorizationService>();
        services.AddSingleton<HealthCheckPublicAPI>();
        services.AddSingleton<HealthCheckCommandApi>();
        services.AddSingleton<HealthCheckServiceBus>();
        services.AddSingleton<HealthCheckKeyVault>();
        services.AddSingleton<HealthCheckADX>();
        services.AddSingleton<HealthCheckADT>();
        services.AddSingleton<HealthCheckADTApi>();
        services.AddSingleton<HealthCheckFederatedProcessor>();
        services.AddSingleton<HealthCheckFederatedInsightCore>();
        services.AddSingleton<HealthCheckSearch>();

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        services.AddHealthChecks()
            .AddCheck<HealthCheckFederatedProcessor>("Rules Engine Processor", tags: ["healthz"])
            .AddCheck<HealthCheckPublicAPI>("Public API", tags: ["healthz"])
            .AddCheck<HealthCheckCommandApi>("Command API", tags: ["healthz"])
            .AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"])
            .AddCheck<HealthCheckServiceBus>("Key Vault", tags: ["healthz"])
            .AddCheck<HealthCheckSearch>("Search", tags: ["healthz"])
            .AddCheck<HealthCheckADX>("ADX", tags: ["healthz"])
            .AddCheck<HealthCheckADT>("ADT", tags: ["healthz"])
            .AddCheck<HealthCheckAuthorizationService>("Authorization Service", tags: ["healthz"])
            .AddCheck<HealthCheckADTApi>("ADTApi", tags: ["healthz"]);

        services.AddSingleton<IHealthCheckPublisher>(_ => new HealthCheckPublisher("Rules Web"));
    }

    /// <summary>
    /// Configures the HTTP request pipeline (called by runtime)
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // See https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-6.0
        app.UseForwardedHeaders();

        //required to send ProblemDetails objects by defualt e.g. 403's and the like so that the client app can respond to it
        app.UseStatusCodePages();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
        }

        // Middleware that drops an env.js file into any path when the SPA load index.html on any path
        app.Use(async (HttpContext context, Func<Task> next) =>
        {
            if (context.Request.Path.Value.EndsWith("/env.js"))
            {
                var appInsights = Configuration.GetSection("ApplicationInsights");
                var appInsightsConnectionString = appInsights.GetValue<string>("Connection_String");
                var customerOptions = app.ApplicationServices.GetRequiredService<IOptions<CustomerOptions>>();
                var config = app.ApplicationServices.GetRequiredService<IOptions<ADB2COptions>>();

                var result = new
                {
                    customer = customerOptions.Value.Name,
                    customerId = customerOptions.Value.Id,
                    redirect = config.Value.Redirect,
                    baseurl = config.Value.BaseUrl,
                    // Base url for api requests, can be different from baseurl for local testing (npm run + IDE)
                    baseapi = config.Value.BaseApi,
                    clientId = config.Value.ClientId,// ,
                    authority = config.Value.Authority,
                    knownAuthorities = config.Value.KnownAuthorities,
                    b2cscopes = config.Value.B2CScopes,
                    version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    appInsightsConnectionString = appInsightsConnectionString
                };

                // Writes one-line of javascript (!) to the output
                string envjs = "window._env_=" + JsonConvert.SerializeObject(result) + ";";
                // dynamically changes the base href
                envjs += $"document.getElementById('basehref').href='{config.Value.BaseUrl}';";

                context.Response.GetTypedHeaders().CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                {
                    NoCache = true,
                    NoStore = true
                };

                context.Response.Headers.ContentType = "text/javascript";
                await context.Response.WriteAsync(envjs);
            }
            else
            {
                // If you get a 'Failed to proxy' exception here, make sure the Vite server is running
                // change to ClientApp and type `npm run dev`. It should say "running at http://localhost:3000"
                await next.Invoke();
            }
        });

        app.UseRouting();

        app.UseResponseCompression();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();

        app.UseSwaggerUI(c =>
        {
            var config = app.ApplicationServices.GetRequiredService<IOptions<ADB2COptions>>();
            string baseurl = config.Value.BaseUrl;
            c.SwaggerEndpoint(baseurl.TrimEnd('/') + "/swagger/v1/swagger.json", "Willow Activate v1");
            // c.SwaggerEndpoint(baseurl.TrimEnd('/') + "/swagger/health/swagger.json", "Health");
        });

        app.UseWillowHealthChecks(new WebHealthCheckResponse()
        {
            HealthCheckDescription = "Activate Technology Web App",
            HealthCheckName = "Activate Technology Web App",
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");

            endpoints.MapControllers();
        });

        // In production
        //app.UseStaticFiles();  // this is wwwroot - useless?
        if (!env.IsDevelopment())
        {
            // This serves the ClientApp/dist folder, see above
            app.UseSpaStaticFiles();
        }

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";
            if (env.IsDevelopment())
            {
                spa.Options.DevServerPort = 3000;
                //the npm script must start with "echo Starting the development server &&" for this to work
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
        });

        // Turn off noisy Kusto logging
        Kusto.Cloud.Platform.Utils.TraceSourceManager.SetTraceVerbosityForAll(Kusto.Cloud.Platform.Utils.TraceVerbosity.Fatal);
    }

    private static readonly JsonSerializerSettings jsonSettings = new() { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

    private class WebHealthCheckResponse : HealthCheckResponse
    {
        public override Task WriteHealthZResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var slimResult = new HealthCheckDto("Activate Technology Web", "Web health", healthReport);
            string json = JsonConvert.SerializeObject(slimResult, jsonSettings);
            return context.Response.WriteAsync(json);
        }
    }
}
