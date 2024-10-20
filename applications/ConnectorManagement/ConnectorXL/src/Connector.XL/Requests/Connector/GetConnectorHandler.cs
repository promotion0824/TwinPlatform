namespace Connector.XL.Requests.Connector;

using global::Connector.XL.Features.ConnectorFeatureGroup.ConnectorsFeature;
using global::Connector.XL.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

internal static class GetConnectorHandler
{
    internal static async Task<Ok<ConnectorEntity>> HandleAsync([FromRoute] Guid connectorId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        var connector = await GetConnectorByIdAsync(httpClientFactory, connectorId);
        return TypedResults.Ok(connector);
    }

    [Obsolete("Use HandleAsync instead")]
    internal static async Task<Ok<IList<ConnectorEntity>>> HandleWithSiteIdAsync([FromRoute] Guid siteId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        var connector = await GetConnectorsWithSiteIdAsync(httpClientFactory, siteId);
        return TypedResults.Ok(connector);
    }

    internal static async Task<ConnectorEntity> GetConnectorByIdAsync(IHttpClientFactory httpClientFactory, Guid connectorId)
    {
        var client = httpClientFactory.CreateClient(Constants.ConnectorCoreApiClientName);
        var url = $"connectors/{connectorId}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var strResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ConnectorEntity>(strResponse);
    }

    internal static async Task<IList<ConnectorEntity>> GetConnectorsWithSiteIdAsync(IHttpClientFactory httpClientFactory, Guid siteId)
    {
        var client = httpClientFactory.CreateClient(Constants.ConnectorCoreApiClientName);
        var url = $"sites/{siteId}/connectors";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var strResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<ConnectorEntity>>(strResponse);
    }
}
