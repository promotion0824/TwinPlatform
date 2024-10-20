using Azure.Core;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Willow.AzureDataExplorer.Options;

namespace Willow.AzureDataExplorer.Builders;

public interface IClientBuilder
{
    Task<IKustoIngestClient> GetKustoIngestClient { get; }
    Task<ICslQueryProvider> GetCslQueryProvider { get; }
    Task<ICslAdminProvider> GetCslAdminProvider { get; }
}

public class ClientBuilder : IClientBuilder
{
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<AzureDataExplorerOptions> _options;
    private readonly TokenCredential _tokenCredential;
    public ClientBuilder(IMemoryCache memoryCache,
        IOptions<AzureDataExplorerOptions> options,
        TokenCredential tokenCredential)
    {
        _memoryCache = memoryCache;
        _options = options;
        _tokenCredential = tokenCredential;
    }

    public Task<IKustoIngestClient> GetKustoIngestClient
    {
        get => GetCachedKustoClient($"{_options.Value.ClusterName}.{_options.Value.ClusterRegion}.KustoIngestClient", (c) => KustoIngestFactory.CreateDirectIngestClient(c));
    }

    public Task<ICslQueryProvider> GetCslQueryProvider
    {
        get => GetCachedKustoClient($"{_options.Value.ClusterName}.{_options.Value.ClusterRegion}.CslQueryProvider", (c) => KustoClientFactory.CreateCslQueryProvider(c));
    }

    public Task<ICslAdminProvider> GetCslAdminProvider
    {
        get => GetCachedKustoClient($"{_options.Value.ClusterName}.{_options.Value.ClusterRegion}.CslAdminProvider", (c) => KustoClientFactory.CreateCslAdminProvider(c));
    }

    private Task<T> GetCachedKustoClient<T>(string cacheKey, Func<KustoConnectionStringBuilder, T> getClient)
    {
        return _memoryCache.GetOrCreateAsync(cacheKey, async (c) =>
        {
            var kustoUri = GetClusterUri();
            c.SetPriority(CacheItemPriority.NeverRemove);
            var accessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(new[]
                {
                        $"{kustoUri}/.default"
                }), new CancellationToken());
            var connectionStringBuilder = new KustoConnectionStringBuilder(kustoUri).WithAadTokenProviderAuthentication(() => accessToken.Token);
            c.AbsoluteExpiration = accessToken.ExpiresOn.AddMinutes(-1);

            return getClient(connectionStringBuilder);
        })!;
    }

    private string GetClusterUri()
    {
        if (_options.Value.ClusterUri is not null)
            return _options.Value.ClusterUri;

        if (_options.Value.ClusterName is not null && _options.Value.ClusterRegion is not null)
            return $"https://{_options.Value.ClusterName}.{_options.Value.ClusterRegion}.kusto.windows.net/";

        throw new NotSupportedException("Azure Data Explorer authentication type not supported.");
    }
}
