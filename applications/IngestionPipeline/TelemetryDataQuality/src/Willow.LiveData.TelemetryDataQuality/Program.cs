using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Azure;
using StackExchange.Redis;
using Willow.Api.Authentication;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.AzureDigitalTwins.SDK.Extensions;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.LiveData.TelemetryDataQuality;
using Willow.LiveData.TelemetryDataQuality.Infrastructure;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;
using Willow.LiveData.TelemetryDataQuality.Options;
using Willow.LiveData.TelemetryDataQuality.Services;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Security.KeyVault;

return WebApplicationStart.Run(args, "TelemetryDataQuality", Configure, ConfigureApp, ConfigureHealthChecks);

void Configure(WebApplicationBuilder builder)
{
    builder.Services.AddLazyCache();

    //Eventhub
    builder.Services.AddBatchEventHubListener<TelemetryProcessor>(config => builder.Configuration.Bind("EventHub", config));
    builder.Services.AddEventHubSender<TelemetryDataQuality>(config => builder.Configuration.Bind("EventHub", config));

    //Twins-Api
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<AuthenticationDelegatingHandler>();
    builder.Services.AddAdtApiHttpClient(builder.Configuration.GetSection("TwinsApi"));
    builder.Services.AddOptions<AzureADOptions>().Bind(builder.Configuration.GetSection("AzureAd"));
    builder.Services
        .AddHttpClient<ITwinsClient, TwinsClient>(Willow.AzureDigitalTwins.SDK.Extensions.ServiceCollectionExtensions.AdtApiClientName)
        .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

    ConfigureCaching(builder);

    //App specific services
    builder.Services.AddOptions<TwinsCacheOption>().Bind(builder.Configuration.GetSection(TwinsCacheOption.Section));
    builder.Services.AddSingleton<ITwinsService, TwinsService>();

    builder.Services.AddOptions<TimeSeriesPersistenceOption>()
        .Bind(builder.Configuration.GetSection(TimeSeriesPersistenceOption.Section));
    builder.Services.AddSingleton<ITimeSeriesService, TimeSeriesService>();

    builder.Services.AddHostedService<BackgroundMonitoringService>();
}

void ConfigureCaching(WebApplicationBuilder builder)
{
    builder.Services.Configure<RedisOption>(options => builder.Configuration.Bind("Redis", options));
    var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOption>();
    if (redisOptions is not null && redisOptions.Enabled)
    {
        //KeyVault
        var vaultName = builder.Configuration.GetValue<string>("Azure:KeyVault:KeyVaultName");
        if (string.IsNullOrWhiteSpace(vaultName))
        {
            Console.WriteLine("Startup Variable 'Azure:KeyVault:KeyVaultName' not found. Exiting");
            throw new ArgumentNullException("Azure:KeyVault:KeyVaultName");
        }

        var (key, semaphore) = SecretManager.GetKeyedSingletonDependencies();
        builder.Services.AddKeyedSingleton(key, semaphore);

        var vaultUri = $"https://{vaultName}.vault.azure.net/";
        builder.Services.AddAzureClients(clientBuilder => clientBuilder.AddSecretClient(new Uri(vaultUri)));
        builder.Services.AddSingleton<ISecretManager, SecretManager>();

        builder.Services.AddSingleton<RedisClientFactory>();
        builder.Services.AddSingleton<IConnectionMultiplexer>(services =>
            services.GetRequiredService<RedisClientFactory>().CreateConnectionMultiplexer().GetAwaiter().GetResult() ??
            throw new InvalidOperationException());
        builder.Services.AddSingleton<ICacheService<TwinDetails>, RedisCacheService<TwinDetails>>();
        builder.Services.AddSingleton<ICacheService<TimeSeries>, RedisCacheService<TimeSeries>>();
    }
    else
    {
        builder.Services.AddSingleton<ICacheService<TwinDetails>, MemoryCacheService<TwinDetails>>();
        builder.Services.AddSingleton<ICacheService<TimeSeries>, MemoryCacheService<TimeSeries>>();
    }
}

ValueTask ConfigureApp(WebApplication application)
{
    LoadTwinsCache(application.Services);

    return ValueTask.CompletedTask;
}

static void LoadTwinsCache(IServiceProvider services)
{
    var twinsCacheService = services.GetRequiredService<ITwinsService>();
    twinsCacheService.LoadTwins().GetAwaiter().GetResult();
}

static void ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder)
{
    healthChecksBuilder.AddSingletonCheck<HealthCheckTwinsApi>("TwinsApi", tags: ["healthz"]);
}
