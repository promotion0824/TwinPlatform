using Authorization.TwinPlatform.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Willow.Api.Authentication;
using Willow.AzureDigitalTwins.SDK.Extensions;
using Willow.HealthChecks;
using Willow.PublicApi.Authorization;
using Willow.PublicApi.Config;
using Willow.PublicApi.Expressions;
using Willow.PublicApi.HealthChecks;
using Willow.PublicApi.OpenApi;
using Willow.PublicApi.Services;
using Willow.PublicApi.Transforms;
using Yarp.ReverseProxy.Swagger;
using Yarp.ReverseProxy.Swagger.Extensions;

var assemblyNameObj = Assembly.GetExecutingAssembly().GetName();
var version = assemblyNameObj.Version?.ToString(2) ?? "0.0";
const string AppName = "PublicApiV3";
var pathPrefix = "/publicapi";

List<IConfigurationSection> clusters = [];

return WebApplicationStart.Run(args, AppName, Configure, ConfigureApp, ConfigureHealthChecks);

void Configure(WebApplicationBuilder builder)
{
    pathPrefix = builder.Environment.IsProduction() ? pathPrefix : string.Empty;

    builder.Configuration.AddJsonFile("appsettingstwins.json", false, true);
    builder.Configuration.AddJsonFile("appsettingstimeseries.json", false, true);
    builder.Configuration.AddJsonFile("appsettingsinsights.json", false, true);
    builder.Configuration.AddJsonFile("appsettingsworkflow.json", false, true);

    // Re-add the environment config to override the defaults.
    builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true);

    // Get the clusters.
    clusters = builder.Configuration.GetSection("ReverseProxyV1V2:Clusters").GetChildren().ToList();
    clusters.AddRange(builder.Configuration.GetSection("ReverseProxy:Clusters").GetChildren());

    builder.Services.Configure<B2CConfig>(options => builder.Configuration.Bind("AzureAdB2C", options));

    var b2cConfig = builder.GetOptions<B2CConfig>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var tenant = b2cConfig.TenantId;
            options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";
            options.TokenValidationParameters.ValidIssuers =
            [
                $"https://sts.windows.net/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0"
            ];
            options.TokenValidationParameters.ValidateAudience = true;
            options.Audience = b2cConfig.Audience;
        });

    builder.Services.AddSingleton<IAuthorizationHandler, AuthorizationServiceHandler>();
    builder.Services.AddSingleton<IAuthorizationHandler, SingleTwinExpressionHandler>();
    builder.Services.AddSingleton<IClientIdAccessor, HttpContextClientIdAccessor>();
    builder.Services.AddSingleton<IExpressionResolver, ExpressionResolver>();
    builder.Services.AddSingleton<IResourceChecker, ResourceChecker>();

    builder.Services.AddClientCredentialToken(builder.Configuration);
    builder.Services.AddUserManagementCoreServices(builder.Configuration.GetSection("AuthorizationAPI"), builder.Environment.IsDevelopment());
    builder.Services.AddAuthorization(option =>
    {
        option.AddTimeSeriesPolicies();
        option.AddTwinsPolicies();
        option.AddModelsPolicies();
        option.AddInsightsPolicies();
        option.AddTicketsPolicies();
        option.AddInspectionsPolicies();
        option.AddDocumentsPolicies();
    });
    builder.Services.AddAuthorization();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        var filterDescriptors = new List<FilterDescriptor>();

        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Willow Public API", Version = "v1" });
        options.SwaggerDoc("v2", new OpenApiInfo { Title = "Willow Public API", Version = "v2" });
        options.SwaggerDoc("v3", new OpenApiInfo { Title = "Willow Public API", Version = "v3" });

        options.DocumentFilter<ReverseProxyDocumentFilter>();
        options.DocumentFilter<RemoveUnusedSchemas>("v3");
        options.DocumentFilter<OAuthTokenEndpointDocumentFilter>("v3");
        options.DocumentFilter<PathDocumentFilter>("v3", pathPrefix);
        options.DocumentFilter<SecuritySchemes>("v3", new Uri($"{pathPrefix}/v3/oauth2/token", UriKind.Relative));
        options.DocumentFilter<OrderByTags>("v3");
    });

    builder.Services.AddLazyCache();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAdtApiClients(builder.Configuration.GetSection("AdtApi"));

    // YARP
    IConfigurationSection v1v2Section = builder.Configuration.GetSection("ReverseProxyV1V2");

    var proxyBuilder = builder.Services.AddReverseProxy()
        .LoadFromConfig(v1v2Section)
        .AddSwagger(v1v2Section)
        .AddTransforms(context =>
        {
            context.AddCounterTransform();
            context.AddCoreServicesAuthTransform();
        })
        .AddTransformFactory<BodyFormTransform>()
        .AddTransformFactory<TwinPermissionsTransform>()
    .AddTransformFactory<OriginHeaderTransform>();

    if (builder.Configuration.GetValue("EnableV3", false))
    {
        var section = builder.Configuration.GetSection("ReverseProxy");
        proxyBuilder.LoadFromConfig(section)
                    .AddSwagger(section);
    }
}

ValueTask ConfigureApp(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        if (app.Configuration.GetValue("EnableV3", false))
        {
            var b2cOptions = app.Services.GetRequiredService<IOptions<B2CConfig>>().Value;

            options.OAuthAppName("Willow Public API");
            options.SwaggerEndpoint($"{pathPrefix}/swagger/v3/swagger.json", "Willow Public API v3");
        }

        options.SwaggerEndpoint($"{pathPrefix}/swagger/v2/swagger.json", "Willow Public API v2");
        options.SwaggerEndpoint($"{pathPrefix}/swagger/v1/swagger.json", "Willow Public API v1");
    });

    if (app.Configuration.GetValue("EnableV3", false))
    {
        app.UseReDoc(options =>
        {
            options.SpecUrl = $"{pathPrefix}/swagger/v3/swagger.json";
            options.RoutePrefix = "api-docs/v3";
        });
    }

    app.UseReDoc(options =>
    {
        options.SpecUrl = $"{pathPrefix}/swagger/v2/swagger.json";
        options.RoutePrefix = "api-docs/v2";
    });

    app.UseReDoc(options =>
    {
        options.SpecUrl = $"{pathPrefix}/swagger/v1/swagger.json";
        options.RoutePrefix = "api-docs/v1";
    });

    app.MapReverseProxy();

    app.ImportUserManagementConfig();

    return ValueTask.CompletedTask;
}

void ConfigureHealthChecks(IHealthChecksBuilder builder)
{
    foreach (var cluster in clusters)
    {
        var type = cluster.GetSection("Metadata").GetValue<string>("HealthCheck");

        switch (type)
        {
            case "Ping":
                var destination = cluster.GetSection("Destinations").GetChildren().FirstOrDefault();
                if (destination != null)
                {
                    var hcArgs = new PingHealthCheckArgs(destination.GetValue<string>("Address")!);
                    builder.AddTypeActivatedCheck<PingHealthCheck>(destination.Key, null, ["healthz"], hcArgs);
                }

                break;
            case "Federated":
                {
                    destination = cluster.GetSection("Destinations").GetChildren().FirstOrDefault();
                    if (destination != null)
                    {
                        var hcArgs = new HealthCheckFederatedArgs(destination.GetValue<string>("Address")!, null, false);
                        builder.AddTypeActivatedCheck<HealthCheckFederated>(destination.Key, null, ["healthz"], hcArgs);
                    }
                }

                break;
            default:
                break;
        }
    }
}
