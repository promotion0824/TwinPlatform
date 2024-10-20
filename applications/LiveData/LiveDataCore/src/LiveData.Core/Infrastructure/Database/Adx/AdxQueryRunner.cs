namespace Willow.LiveData.Core.Infrastructure.Database.Adx;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Exceptions;
using Kusto.Data.Net.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Retry;
using Willow.Infrastructure.Exceptions;
using Willow.LiveData.Core.Infrastructure.HealthCheck;

/// <summary>
/// Query runner for ADX.
/// </summary>
internal sealed class AdxQueryRunner : IAdxQueryRunner, IDisposable
{
    private const int DefaultTimeout = 180;
    private readonly IConfiguration config;
    private readonly IMemoryCache memoryCache;
    private ICslQueryProvider cslQueryProvider;
    private ICslAdminProvider cslAdminProvider;
    private readonly HealthCheckADX healthCheckADX;
    private readonly TokenCredential tokenCredential;

    public AdxQueryRunner(IConfiguration config, IMemoryCache memoryCache, HealthCheckADX healthCheckADX, TokenCredential tokenCredential)
    {
        this.config = config;
        this.memoryCache = memoryCache;
        this.healthCheckADX = healthCheckADX;
        this.tokenCredential = tokenCredential;
    }

    private AsyncRetryPolicy RetryPolicy =>
        Policy.Handle<KustoRequestException>(kre => kre.ErrorReason == "Unauthorized")
            .Or<KustoClientException>()
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    /// <summary>
    /// To run the query against ADX table.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="kqlQuery">Kusto query.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown if the query times out.</exception>
    public async Task<IDataReader> QueryAsync(Guid? clientId, string kqlQuery)
    {
        string kustoUri = GetClusterUri(clientId);
        string databaseName = GetDatabaseName(clientId);
        if (clientId == null && (string.IsNullOrEmpty(kustoUri) || string.IsNullOrEmpty(databaseName)))
        {
            healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
            throw new BadRequestException($"{nameof(clientId)} is required");
        }

        try
        {
            var connectionString = GetConnectionString(kustoUri);
            var kustoClient = GetCslQueryProvider(connectionString);
            var kustoClientRequestProperties = GetKustoClientRequestProperties();

            // create a timer task to cancel after the DefaultTimeout
            var timerTaskSource = new TaskCompletionSource();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeout));
            cts.Token.Register(() => timerTaskSource.TrySetCanceled());

            healthCheckADX.Current = HealthCheckADX.Healthy;
            var readerTask = RetryPolicy.ExecuteAsync(async () =>
                await kustoClient.ExecuteQueryAsync(databaseName, kqlQuery, kustoClientRequestProperties, cts.Token));

            var completedTask = await Task.WhenAny(readerTask, timerTaskSource.Task);
            if (completedTask == timerTaskSource.Task)
            {
                // we should immediately fire an exception when token is cancelled, as the query will terminate
                // eventually and not far away because we are using the server timeout option in the query property
                throw new TimeoutException("ADX execution timed out after more than 180 seconds");
            }

            if (timerTaskSource.TrySetResult())
            {
                await timerTaskSource.Task;
            }

            return await readerTask;
        }
        catch (KustoRequestThrottledException)
        {
            healthCheckADX.Current = HealthCheckADX.RateLimited;
            throw;
        }
        catch (Exception)
        {
            healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
            throw;
        }
    }

    /// <summary>
    /// To run admin level queries against ADX database.
    /// </summary>
    /// <param name="clientId">Client ID.</param>
    /// <param name="kqlQuery">Kusto query.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown if the query times out.</exception>
    public async Task<IDataReader> ControlQueryAsync(Guid? clientId, string kqlQuery)
    {
        string kustoUri = GetClusterUri(clientId);
        string databaseName = GetDatabaseName(clientId);
        if (clientId == null && (string.IsNullOrEmpty(kustoUri) || string.IsNullOrEmpty(databaseName)))
        {
            healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
            throw new BadRequestException($"{nameof(clientId)} is required");
        }

        try
        {
            var connectionString = GetConnectionString(kustoUri);
            var kustoCslAdminProvider = GetCslAdminProvider(connectionString);
            var kustoClientRequestProperties = GetKustoClientRequestProperties();

            // create a timer task to cancel after the DefaultTimeout
            var timerTaskSource = new TaskCompletionSource();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(DefaultTimeout));
            cts.Token.Register(() => timerTaskSource.TrySetCanceled());
            var readerTask = RetryPolicy.ExecuteAsync(async () =>
                await kustoCslAdminProvider.ExecuteControlCommandAsync(databaseName, kqlQuery, kustoClientRequestProperties));

            var completedTask = await Task.WhenAny(readerTask, timerTaskSource.Task);
            if (completedTask == timerTaskSource.Task)
            {
                // we should immediately fire an exception when token is cancelled, as the query will terminate
                // eventually and not far away because we are using the server timeout option in the query property
                throw new TimeoutException("ADX execution timed out after more than 180 seconds");
            }

            if (timerTaskSource.TrySetResult())
            {
                await timerTaskSource.Task;
            }

            return await readerTask;
        }
        catch (KustoRequestThrottledException)
        {
            healthCheckADX.Current = HealthCheckADX.RateLimited;
            throw;
        }
        catch (Exception)
        {
            healthCheckADX.Current = HealthCheckADX.ConnectionFailed;
            throw;
        }
    }

    private static ClientRequestProperties GetKustoClientRequestProperties()
    {
        var clientRequestProperties = new ClientRequestProperties
        {
            ClientRequestId = Guid.NewGuid().ToString(),
        };
        clientRequestProperties.SetOption(ClientRequestProperties.OptionServerTimeout,
                                               TimeSpan.FromSeconds(DefaultTimeout));
        return clientRequestProperties;
    }

    private KustoConnectionStringBuilder GetConnectionString(string kustoUri)
    {
        return new KustoConnectionStringBuilder(kustoUri)
                        .WithAadTokenProviderAuthentication(() => GetToken(new Uri(kustoUri)));
    }

    private string GetDatabaseName(Guid? clientId)
    {
        var adxDbName = config["ADX:Database"];
        if (clientId is not null && !string.IsNullOrEmpty(config[$"{clientId}:ADX:Database"]))
        {
            adxDbName = config[$"{clientId}:ADX:Database"];
        }

        return adxDbName;
    }

    private string GetClusterUri(Guid? clientId)
    {
        var adxClusterUri = config["ADX:ClusterUri"];
        if (clientId is not null && !string.IsNullOrEmpty(config[$"{clientId}:ADX:ClusterUri"]))
        {
            adxClusterUri = config[$"{clientId}:ADX:ClusterUri"];
        }

        return adxClusterUri;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        cslQueryProvider?.Dispose();
        cslQueryProvider = null;
        cslAdminProvider?.Dispose();
        cslAdminProvider = null;
    }

    private ICslQueryProvider GetCslQueryProvider(KustoConnectionStringBuilder connectionString)
    {
        return cslQueryProvider ??= KustoClientFactory.CreateCslQueryProvider(connectionString);
    }

    private ICslAdminProvider GetCslAdminProvider(KustoConnectionStringBuilder connectionString)
    {
        return cslAdminProvider ??= KustoClientFactory.CreateCslAdminProvider(connectionString);
    }

    private string GetToken(Uri clusterUri)
    {
        var requestUri = clusterUri + ".default";
        return memoryCache.GetOrCreate("AzureToken",
            c =>
        {
            c.SetPriority(CacheItemPriority.NeverRemove);
            var token = tokenCredential.GetToken(new TokenRequestContext(new[] { requestUri }), CancellationToken.None);
            c.AbsoluteExpiration = token.ExpiresOn;

            return token.Token;
        });
    }
}
