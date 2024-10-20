namespace Connector.XL.Requests.Connector;

using global::Connector.XL.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal static class ExportConnectorStateHandler
{
    public static async Task<NoContent> HandleAsync([FromRoute] Guid customerId,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(Constants.ConnectorCoreApiClientName);
        var url = $"customers/{customerId}/export";
        var response = await client.PostAsync(url, null, cancellationToken);

        response.EnsureSuccessStatusCode();

        return TypedResults.NoContent();
    }
}
