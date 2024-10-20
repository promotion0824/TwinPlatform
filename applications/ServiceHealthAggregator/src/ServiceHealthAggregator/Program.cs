using Azure.Identity;
using Microsoft.Extensions.Azure;
using Willow.Api.Authentication;
using Willow.AppContext;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.Security.KeyVault;
using Willow.ServiceHealthAggregator;

List<HealthCheckConfig> healthChecks = [];

return await WebApplicationStart.RunAsync(args, "Service Health Aggregator", Configure, ConfigureApp, ConfigureHealthChecks);

void Configure(WebApplicationBuilder builder)
{
    // This semaphore is used to ensure the SecretManager is only accessed by one thread at a time
    (object key, Semaphore semaphore) = SecretManager.GetKeyedSingletonDependencies();
    builder.Services.AddKeyedSingleton(key, semaphore);

    builder.Services.AddAzureClients(clientBuilder =>
    {
        var vaultName = builder.Configuration["Azure:KeyVault:KeyVaultName"];
        var vaultUri = $"https://{vaultName}.vault.azure.net/";

        var tokenCredential = new DefaultAzureCredential();

        clientBuilder.UseCredential(tokenCredential);

        // Register clients for each service
        clientBuilder.AddSecretClient(new Uri(vaultUri));
    });

    builder.Services.Configure<WillowContextOptions>(context => builder.Configuration.Bind("WillowContext", context));

    builder.Services.AddOptions<InstanceOptions>().Bind(builder.Configuration.GetSection("InstanceOptions"));

    builder.Services.AddMemoryCache();
    builder.Services.AddClientCredentialToken(builder.Configuration);
    builder.Services.AddTransient<AuthenticationDelegatingHandler>();
    builder.Services.AddHttpClient("Willow.ServiceHealthAggregator");
    builder.Services.AddSingleton<ISecretManager, SecretManager>();
    builder.Services.AddHttpClient<ITwinsClient, TwinsClient>(ConfigureHttpClient(builder.Configuration))
            .AddHttpMessageHandler<AuthenticationDelegatingHandler>();

    builder.Services.AddHttpClient("HealthCheck");
    builder.Configuration.GetSection("HealthChecks").Bind(healthChecks);

    builder.Services.AddHostedService<ScanCustomerInstanceHostedService>();
    builder.Services.AddSnowflakeListener(options => builder.Configuration.Bind("Snowflake", options));
}

ValueTask ConfigureApp(WebApplication app) => ValueTask.CompletedTask;

void ConfigureHealthChecks(IHealthChecksBuilder builder)
{
    foreach (var healthCheck in healthChecks)
    {
        var hcArgs = new HealthCheckFederatedArgs(healthCheck.Url.ToString(), healthCheck.Path, false);
        builder.AddTypeActivatedCheck<HealthCheckFederated>(healthCheck.Name, null, ["healthz"], hcArgs);
    }
}

static Action<IServiceProvider, HttpClient> ConfigureHttpClient(IConfiguration configuration) => (serviceProvider, httpClient) =>
{
    var instanceOptions = configuration.GetSection("InstanceOptions").Get<InstanceOptions>();
    httpClient.BaseAddress = new Uri(instanceOptions?.TwinsApiEndpoint ?? "http://adt-api");
};
