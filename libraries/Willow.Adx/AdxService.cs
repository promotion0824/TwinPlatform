namespace Willow.Adx;

using Azure.Core;
using Azure.Identity;
using Dapper;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Exceptions;
using Kusto.Data.Net.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

internal class AdxService(IOptions<AdxConfig> adxConfigOptions, IMemoryCache memoryCache, ILogger<AdxService> logger)
    : IAdxService
{
    private const int DefaultTimeout = 180;
    private const string AzureAdxAccessTokenCacheKey = "AzureAdxAccessToken";
    private const int ConnectRetryDelayMs = 1000;

    private ICslQueryProvider? cslQueryProvider;
    private ICslAdminProvider? cslAdminProvider;

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<T>(async () =>
        {
            var kustoClient = GetCslQueryProvider();
            var dataReader = await kustoClient.ExecuteQueryAsync(adxConfigOptions.Value.DatabaseName, query, GetKustoClientRequestProperties(), cancellationToken);
            return dataReader.Parse<T>();
        },
        () => cslQueryProvider = null,
        cancellationToken);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string query, IDictionary<string, string> parameters, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync<T>(async () =>
        {
            var kustoClient = GetCslQueryProvider();
            var dataReader = await kustoClient.ExecuteQueryAsync(adxConfigOptions.Value.DatabaseName, query, GetKustoClientRequestProperties(parameters), cancellationToken);
            return dataReader.Parse<T>();
        },
        () => cslQueryProvider = null,
        cancellationToken);
    }

    private async Task<IEnumerable<T>> ExecuteWithRetryAsync<T>(Func<Task<IEnumerable<T>>> executeFunc, Action clearProviderAction, CancellationToken cancellationToken)
    {
        // Default retry policy when not passed via config
        var retryPolicy = adxConfigOptions.Value.RetryPolicy ??
                          Policy.Handle<KustoClientException>()
                                .WaitAndRetryAsync(3,
                                  retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var authRetryPolicy = Policy
            .Handle<KustoRequestException>(ex => ex.ErrorReason == "Unauthorized")
            .RetryAsync(1,
                onRetryAsync: async (exception, _) =>
            {
                logger.LogWarning("Retrying ADX query after {Exception}", exception.Message);
                clearProviderAction();
                memoryCache.Remove(AzureAdxAccessTokenCacheKey);
                await Task.Delay(ConnectRetryDelayMs, cancellationToken);
            });

        var combinedPolicy = authRetryPolicy.WrapAsync(retryPolicy);

        return await combinedPolicy.ExecuteAsync(async () => await executeFunc());
    }

    public async Task<IEnumerable<T>> ControlQueryAsync<T>(string query, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            var kustoClient = GetCslAdminProvider();
            var dataReader = await kustoClient.ExecuteControlCommandAsync(adxConfigOptions.Value.DatabaseName, query, GetKustoClientRequestProperties());

            return dataReader.Parse<T>();
        },
        () => cslAdminProvider = null,
        cancellationToken);
    }

    public Task ControlQueryAsync(string query, CancellationToken cancellationToken = default) =>
        GetCslAdminProvider().ExecuteControlCommandAsync(adxConfigOptions.Value.DatabaseName, query, GetKustoClientRequestProperties());

    private ICslQueryProvider GetCslQueryProvider() => cslQueryProvider ??= KustoClientFactory.CreateCslQueryProvider(GetKustoConnectionString());

    private ICslAdminProvider GetCslAdminProvider() => cslAdminProvider ??= KustoClientFactory.CreateCslAdminProvider(GetKustoConnectionString());

    private KustoConnectionStringBuilder GetKustoConnectionString()
    {
        var clusterUri = adxConfigOptions.Value.ClusterUri;
        ArgumentNullException.ThrowIfNull(clusterUri);

        var token = memoryCache.GetOrCreate(AzureAdxAccessTokenCacheKey, entry =>
        {
            entry.SetPriority(CacheItemPriority.NeverRemove);
            var tokenResponse = new DefaultAzureCredential().GetToken(new TokenRequestContext(new[] { new Uri(clusterUri).AbsoluteUri + "/.default" }));

            entry.AbsoluteExpirationRelativeToNow = tokenResponse.ExpiresOn - DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5);

            return tokenResponse.Token;
        });

        return new KustoConnectionStringBuilder(clusterUri).WithAadTokenProviderAuthentication(() => token);
    }

    private static ClientRequestProperties GetKustoClientRequestProperties() => GetKustoClientRequestProperties(new Dictionary<string, string>());

    private static ClientRequestProperties GetKustoClientRequestProperties(IDictionary<string, string> parameters)
    {
        var clientRequestProperties = new ClientRequestProperties
        {
            ClientRequestId = Guid.NewGuid().ToString(),
        };
        clientRequestProperties.SetOption(ClientRequestProperties.OptionServerTimeout,
                                               TimeSpan.FromSeconds(DefaultTimeout));

        foreach (var parameter in parameters)
        {
            clientRequestProperties.SetParameter(parameter.Key, parameter.Value);
        }

        return clientRequestProperties;
    }
}
