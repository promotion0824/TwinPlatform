namespace Willow.PublicApi.Authorization;

using Azure.DigitalTwins.Core;
using LazyCache;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.PublicApi.Expressions;
using Willow.PublicApi.Services;

internal class ResourceChecker(IExpressionResolver expressionResolver, IClientIdAccessor clientIdAccessor, ITwinsClient client, IAppCache cache) : IResourceChecker
{
    public bool HasFullPermissions()
    {
        var clientId = clientIdAccessor.GetClientId();

        return expressionResolver.Expressions.TryGetValue(clientId, out QueryResult? queryResult) && queryResult == null;
    }

    public Task<bool> HasTwinPermission(string? twinId, CancellationToken cancellationToken = default) =>
        HasIdPermission(twinId, t => t.TwinId == twinId, cancellationToken);

    public Task<bool> HasExternalIdPermission(string? externalId, CancellationToken cancellationToken = default) =>
        HasIdPermission(externalId, t => t.ExternalId == externalId, cancellationToken);

    public Task<IEnumerable<string?>> FilterTwinPermission(IEnumerable<string?> twinIds, CancellationToken cancellationToken = default) =>
        FilterIds(twinIds, t => t.TwinId, cancellationToken);

    public Task<IEnumerable<string?>> FilterExternalIdPermission(IEnumerable<string?> externalIds, CancellationToken cancellationToken = default) =>
        FilterIds(externalIds, t => t.ExternalId, cancellationToken);

    public Task<IEnumerable<TwinIds>> GetAllowedTwins(CancellationToken cancellationToken = default)
    {
        var clientId = clientIdAccessor.GetClientId();
        if (!expressionResolver.Expressions.TryGetValue(clientId, out QueryResult? queryResult) || queryResult == null || !queryResult.Success)
        {
            return Task.FromResult(Enumerable.Empty<TwinIds>());
        }

        return GetTwinIds(queryResult);
    }

    private async Task<IEnumerable<TwinIds>> GetTwinIds(QueryResult queryResult)
    {
        List<(string, string?)> ids = [];

        if (queryResult.Request is not null)
        {
            var parent = await client.GetTwinByIdAsync(queryResult.Request.LocationId);
            ids.Add(GetTwinIds(parent.Twin));

            var result = await client.QueryTwinsAsync(queryResult.Request);

            ids.AddRange(result.Content.Select(c => GetTwinIds(c.Twin)));

            while (result.ContinuationToken is not null)
            {
                result = await client.QueryTwinsAsync(queryResult.Request, continuationToken: result.ContinuationToken);
                ids.AddRange(result.Content.Select(c => GetTwinIds(c.Twin)));
            }
        }

        return ids;

        static (string, string?) GetTwinIds(BasicDigitalTwin twin)
        {
            twin.Contents.TryGetValue("externalID", out object? externalId);

            return (twin.Id, externalId?.ToString());
        }
    }

    private async Task<bool> HasIdPermission(string? id, Func<TwinIds, bool> predicate, CancellationToken cancellationToken = default)
    {
        string clientId = clientIdAccessor.GetClientId();

        if (string.IsNullOrEmpty(id) || !expressionResolver.Expressions.TryGetValue(clientId, out QueryResult? queryResult) || (!queryResult?.Success ?? false))
        {
            return false;
        }

        // null indicates no expression provided, therefore the client has full resource permissions.
        if (queryResult == null)
        {
            return true;
        }

        var twins = await cache.GetOrAddAsync(queryResult.ToString(), (_) => GetTwinIds(queryResult));
        return twins.Any(predicate);
    }

    public async Task<IEnumerable<string?>> FilterIds(IEnumerable<string?> ids, Func<TwinIds, string?> selector, CancellationToken cancellationToken = default)
    {
        string clientId = clientIdAccessor.GetClientId();

        if (!expressionResolver.Expressions.TryGetValue(clientId, out QueryResult? queryResult) || queryResult == null || !queryResult.Success)
        {
            return [];
        }

        var twins = await cache.GetOrAddAsync(queryResult.ToString(), (_) => GetTwinIds(queryResult));
        return ids.Intersect(twins.Select(selector));
    }
}
