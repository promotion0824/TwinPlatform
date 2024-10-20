namespace Connector.XL.Requests.Scan;

#nullable enable
using global::Connector.XL.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

internal class PutScanResultHandler
{
    internal static async Task<Results<NoContent, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId,
        [FromRoute] Guid scanId,
        [FromBody] UpdateConnectorScanRequest request,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient(Constants.ConnectorCoreApiClientName);
        var queryParameter = new Dictionary<string, string?>
        {
            { "status", request.Status.ToString() },
        };

        if (request.ErrorCount.HasValue)
        {
            queryParameter["errorCount"] = request.ErrorCount.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(request.ErrorMessage))
        {
            queryParameter["errorMessage"] = request.ErrorMessage;
        }

        if (request.Started.HasValue)
        {
            queryParameter["startTime"] = $"{request.Started.Value:yyyy-MM-dd'T'HH:mm:ss'Z'}";
        }

        if (request.Finished.HasValue)
        {
            queryParameter["endTime"] = $"{request.Finished.Value:yyyy-MM-dd'T'HH:mm:ss'Z'}";
        }

        var url = QueryHelpers.AddQueryString($"connectors/{connectorId}/scans/{scanId}", queryParameter);
        var response = await client.PatchAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        return TypedResults.NoContent();
    }
}
