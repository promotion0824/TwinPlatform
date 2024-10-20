using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics;
using Willow.Hosting.Web;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Extensions;
using Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;
using Willow.LiveData.Core.Features.Connectors.Helpers;
using Willow.LiveData.Core.Features.Connectors.Interfaces;
using Willow.LiveData.Core.Features.Connectors.Repositories;
using Willow.LiveData.Core.Features.Connectors.Services;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData.Adx;
using Willow.LiveData.Core.Features.Telemetry.Helpers;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;
using Willow.LiveData.Core.Features.Telemetry.Repositories;
using Willow.LiveData.Core.Features.Telemetry.Services;
using Willow.LiveData.Core.Features.TimeSeries;
using Willow.LiveData.Core.Infrastructure.Attributes;
using Willow.LiveData.Core.Infrastructure.Azure.KeyVault;
using Willow.LiveData.Core.Infrastructure.Configuration;
using Willow.LiveData.Core.Infrastructure.Database.Adx;
using Willow.LiveData.Core.Infrastructure.HealthCheck;

WebApplicationStart.Run(args, "LiveDataCore", Configure, ConfigureApp, ConfigureHealthChecks);

static void Configure(WebApplicationBuilder builder)
{
    ConfigureKeyVault(builder);
    RegisterConfiguration(builder);
    RegisterServices(builder);
}

static void ConfigureKeyVault(WebApplicationBuilder builder)
{
    var keyVaultName = builder.Configuration.GetValue<string>("Azure:KeyVault:KeyVaultName");
    if (string.IsNullOrEmpty(keyVaultName))
    {
        return;
    }

    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net");
    var credential = new DefaultAzureCredential();
    var assemblyName = Assembly.GetEntryAssembly()?.GetName();
    var prefix = string.Join(string.Empty, assemblyName?.Name?.Where(c => (char.IsLetterOrDigit(c) && c < 128) || c == '-') ?? string.Empty);
    builder.Configuration.AddAzureKeyVault(keyVaultUri, credential, new PrefixKeyVaultSecretManager(prefix));
}

static void RegisterConfiguration(WebApplicationBuilder builder)
{
    builder.Services.Configure<TelemetryConfiguration>(c => builder.Configuration.Bind("Telemetry", c));
    builder.Services.Configure<RequestConfiguration>(c => builder.Configuration.Bind("Request", c));
    builder.Services.Configure<Auth0Configuration>(c => builder.Configuration.Bind("Auth0", c));
}

static void RegisterServices(WebApplicationBuilder builder)
{
    builder.Services.AddApiServices(builder.Configuration, builder.Environment);

    var azureB2CSection = builder.Configuration.GetSection("AzureB2C");
    var azureB2COptions = azureB2CSection.Get<AzureADB2CConfiguration>();

    builder.Services.AddJwtAuthentication(builder.Configuration["Auth0:Domain"], builder.Configuration["Auth0:Audience"], azureB2COptions, builder.Environment, builder.Configuration);

    builder.Services.Configure<AzureADB2CConfiguration>(azureB2CSection);

    builder.Services.AddSingleton<IDateTimeIntervalService, DateTimeIntervalService>();
    builder.Services.AddScoped<IAdxQueryRunner, AdxQueryRunner>();
    builder.Services.AddTransient<IAdxLiveDataRepository, AdxLiveDataRepository>();
    builder.Services.AddTransient<ILiveDataService, LiveDataService>();
    builder.Services.AddTransient<IAdxLiveDataService, AdxLiveDataService>();
    builder.Services.AddTransient(typeof(IAdxContinuationTokenProvider<string, int>), typeof(AdxCTokenProvider));
    builder.Services.AddTransient(typeof(IContinuationTokenProvider<string, string>), typeof(AdxStoredQueryResultTokenProvider));
    builder.Services.AddScoped<PageSizeValidationAttribute>();

    builder.Services.AddTransient<IIoTEdgeConnectorType, IoTEdgeConnectorType>();
    builder.Services.AddTransient<IVmConnectorType, VmConnectorType>();
    builder.Services.AddTransient<IStreamingAnalyticsConnectorType, StreamingAnalyticsConnectorType>();
    builder.Services.AddTransient<IPublicApiConnectorType, PublicApiConnectorType>();
    builder.Services.AddSingleton<IConnectorTypeFactory, ConnectorTypeFactory>();

    builder.Services.AddTransient<IConnectorStatsResultHelper, ConnectorStatsResultHelper>();

    builder.Services.AddTransient<IAdxConnectorRepository, AdxConnectorRepository>();
    builder.Services.AddTransient<IConnectorService, ConnectorService>();

    builder.Services.AddScoped<IAdxQueryHelper, AdxQueryHelper>();
    builder.Services.AddTransient<ITelemetryService, TelemetryService>();
    builder.Services.AddTransient<ITelemetryRepository, TelemetryRepository>();

    builder.Services.AddSingleton<HealthCheckADX>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.WriteIndented = true;
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });

    builder.Services.AddEventHubSender(options => builder.Configuration.Bind("EventHub", options));
}

static void ConfigureHealthChecks(IHealthChecksBuilder builder)
{
    builder.AddCheck<HealthCheckADX>("ADX", tags: ["healthz"]);
}

static ValueTask ConfigureApp(WebApplication app)
{
    var isDevEnvironment = app.Environment.IsDevelopment() || app.Environment.IsEnvironment("dev");
    var isTestEnvironment = app.Environment.IsEnvironment("test");

    if (isDevEnvironment || isTestEnvironment)
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature != null)
                {
                    Console.WriteLine($"{exceptionHandlerFeature.Error.Message}");
                }

                return Task.CompletedTask;
            });
        });

        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    if (app.Configuration.GetValue<bool>("EnableSwagger"))
    {
        var routePrefix = string.Empty;
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                swaggerDoc.Servers =
                [
                    new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{routePrefix}" }
                ];
            });
        });

        app.UseSwaggerUI(options =>
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            options.DocumentTitle = $"{assemblyName} - {app.Environment.EnvironmentName}";
            options.SwaggerEndpoint($"{routePrefix}/swagger/v1/swagger.json", assemblyName + " API V1");

            if (app.Configuration.GetValue<string>("Auth0:ClientId") != null)
            {
                var clientId = app.Configuration.GetValue<string>("Auth0:ClientId");
                options.OAuthClientId(clientId);
                var audience = app.Configuration.GetValue<string>("Auth0:Audience");
                options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "audience", audience } });
            }
        });
    }

    app.UseCors(corsPolicyBuilder => { corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGroup("api").WithOpenApi().MapEndpoints().RequireAuthorization();
    app.MapTimeseries().WithOpenApi().RequireAuthorization();

    return ValueTask.CompletedTask;
}
