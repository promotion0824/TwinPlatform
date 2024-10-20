namespace Connector.XL.Requests.Device;

using global::Connector.XL.Common.Extensions;
using global::Connector.XL.Common.Models.DigitalTwinCore.Dto;
using global::Connector.XL.Common.Services;
using global::Connector.XL.Infrastructure;
using global::Connector.XL.Requests.Connector;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class PutDeviceHandler
{
    internal static async Task<Results<Ok<DeviceEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId,
        [FromForm] DeviceEntity device,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var connector = await GetConnectorHandler.GetConnectorByIdAsync(httpClientFactory, connectorId);
        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = new Uri($"/sites/{connector.SiteId:D}/devices/{device.Id}", UriKind.Relative);

        var deviceMetadataDto = DeviceMetadataDto.MapFromEntity(device);

        //PUT DigitalTwinCore /sites/{siteId}/devices/{id}
        var response = await client.PutAsJsonAsync(url, deviceMetadataDto);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadAsAsync<DeviceDto>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(connector.SiteId);
        return TypedResults.Ok(DeviceDto.MapToEntity(connector.SiteId, clientId, dto));
    }

    [Obsolete("Use HandleAsync instead")]
    internal static async Task<Results<Ok<DeviceEntity>, BadRequest<ProblemDetails>, NotFound>> HandleWithSiteIdAsync([FromRoute] Guid siteId,
        [FromForm] DeviceEntity device,
        [FromServices] ISiteClientIdProvider siteClientIdProvider,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken = default)
    {
        //TODO use endpoint with no SiteId when DigitalTwinCore service is updated.
        var client = httpClientFactory.CreateClient(Constants.DigitalTwinCoreApiClientName);
        var url = new Uri($"/sites/{device.SiteId:D}/devices/{device.Id}", UriKind.Relative);

        var deviceMetadataDto = DeviceMetadataDto.MapFromEntity(device);

        //PUT DigitalTwinCore /sites/{siteId}/devices/{id}
        var response = await client.PutAsJsonAsync(url, deviceMetadataDto);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadAsAsync<DeviceDto>();
        var clientId = await siteClientIdProvider.GetClientIdForSiteAsync(siteId);
        return TypedResults.Ok(DeviceDto.MapToEntity(siteId, clientId, dto));
    }
}
