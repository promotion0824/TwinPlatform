namespace Connector.XL.Requests.Device;

using global::Connector.XL.Common.Extensions;
using global::Connector.XL.Common.Models.DigitalTwinCore.Dto;
using global::Connector.XL.Common.Services;
using global::Connector.XL.Infrastructure;
using global::Connector.XL.Requests.Connector;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal static class GetDevicesHandler
{
    internal static async Task<Results<Ok<List<DeviceEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId,
        [FromQuery] bool? includePoints,
        [FromQuery] bool? isEnabled,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var connector = await GetConnectorHandler.GetConnectorByIdAsync(httpClientFactory, connectorId);
        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = new Uri($"/sites/{connector.SiteId:D}/connectors/{connectorId:D}/devices?" + HttpHelper.ToQueryString(new
        {
            includePoints,
            isEnabled,
        }),
            UriKind.Relative);

        //GET DigitalTwinCore /sites/{siteId}/connectors/{connectorId}/devices
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var dtos = await response.Content.ReadAsAsync<List<DeviceDto>>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(connector.SiteId);
        var result = dtos?.Select(d => DeviceDto.MapToEntity(connector.SiteId, clientId, d)).ToList() ?? [];
        return TypedResults.Ok(result);
    }

    [Obsolete("Use HandleAsync instead")]
    internal static async Task<Results<Ok<List<DeviceEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleWithSiteIdAsync([FromRoute] Guid siteId,
        [FromRoute] Guid connectorId,
        [FromQuery] bool? includePoints,
        [FromQuery] bool? isEnabled,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = new Uri($"/sites/{siteId:D}/connectors/{connectorId:D}/devices?" + HttpHelper.ToQueryString(new
        {
            includePoints,
            isEnabled,
        }),
            UriKind.Relative);

        //GET DigitalTwinCore /sites/{siteId}/connectors/{connectorId}/devices
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var dtos = await response.Content.ReadAsAsync<List<DeviceDto>>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(siteId);
        var result = dtos?.Select(d => DeviceDto.MapToEntity(siteId, clientId, d)).ToList() ?? [];
        return TypedResults.Ok(result);
    }
}
