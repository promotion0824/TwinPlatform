namespace Connector.XL.Requests.Scan;

using global::Connector.XL.Common.Extensions;
using global::Connector.XL.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class GetScanRequestsHandler
{
    internal static async Task<Ok<List<ConnectorScanDto>>> HandleAsync([FromRoute] Guid connectorId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient(Constants.ConnectorCoreApiClientName);
        var response = await client.GetAsync($"connectors/{connectorId}/scans");
        response.EnsureSuccessStatusCode();
        var connectorScans = await response.Content.ReadAsAsync<List<ConnectorScan>>();
        return TypedResults.Ok(ConnectorScanDto.MapFrom(connectorScans));
    }
}
