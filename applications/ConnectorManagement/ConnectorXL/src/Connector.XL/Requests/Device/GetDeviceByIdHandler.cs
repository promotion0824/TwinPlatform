namespace Connector.XL.Requests.Device;

using global::Connector.XL.Common.Extensions;
using global::Connector.XL.Common.Models.DigitalTwinCore.Dto;
using global::Connector.XL.Common.Services;
using global::Connector.XL.Infrastructure;
using global::Connector.XL.Requests.Connector;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal static class GetDeviceByIdHandler
{
    internal static async Task<Results<Ok<DeviceEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId,
        [FromRoute] Guid deviceId,
        [FromQuery] bool? includePoints,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var connector = await GetConnectorHandler.GetConnectorByIdAsync(httpClientFactory, connectorId);

        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = $"/sites/{connector.SiteId:D}/devices/{deviceId:D}";
        if (includePoints != null)
        {
            url += $"?includePoints={includePoints}";
        }

        //GET DigitalTwinCore /sites/{siteId}/devices/{id}
        var response = await client.GetAsync(new Uri(url, UriKind.Relative));
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadAsAsync<DeviceDto>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(connector.SiteId);
        return TypedResults.Ok(DeviceDto.MapToEntity(connector.SiteId, clientId, dto));
    }

    [Obsolete("Use HandleAsync instead")]
    internal static async Task<Results<Ok<DeviceEntity>, BadRequest<ProblemDetails>, NotFound>> HandleWithSiteIdAsync([FromRoute] Guid siteId,
        [FromRoute] Guid connectorId,
        [FromRoute] Guid deviceId,
        [FromQuery] bool? includePoints,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = $"/sites/{siteId:D}/devices/{deviceId:D}";
        if (includePoints != null)
        {
            url += $"?includePoints={includePoints}";
        }

        //GET DigitalTwinCore /sites/{siteId}/devices/{id}
        var response = await client.GetAsync(new Uri(url, UriKind.Relative));
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadAsAsync<DeviceDto>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(siteId);
        return TypedResults.Ok(DeviceDto.MapToEntity(siteId, clientId, dto));
    }
}
