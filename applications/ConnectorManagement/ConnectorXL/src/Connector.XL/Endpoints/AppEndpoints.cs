namespace Connector.XL.Endpoints;

using Connector.XL.Requests.Connector;
using Connector.XL.Requests.Device;
using Connector.XL.Requests.Log;
using Connector.XL.Requests.Scan;

internal static class AppEndpoints
{
    public static RouteGroupBuilder MapApplicationEndpoints(this RouteGroupBuilder group)
    {
        var scanGroup = group.MapGroup(string.Empty).WithOpenApi().WithTags("Scan");
        scanGroup.MapGet("/connectors/{connectorId}/scans", GetScanRequestsHandler.HandleAsync);
        scanGroup.MapPut("/connectors/{connectorId}/scans/{scanId}", PutScanResultHandler.HandleAsync).DisableAntiforgery();

        var deviceGroup = group.MapGroup(string.Empty).WithOpenApi().WithTags("Device");
        deviceGroup.MapGet("/connectors/{connectorId}/devices", GetDevicesHandler.HandleAsync);
        deviceGroup.MapGet("/connectors/{connectorId}/devices/{deviceId}", GetDeviceByIdHandler.HandleAsync);
        deviceGroup.MapPut("/connectors/{connectorId}/devices", PutDeviceHandler.HandleAsync).DisableAntiforgery();
        deviceGroup.MapPut("/sites/{siteId}/devices", PutDeviceHandler.HandleWithSiteIdAsync)
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            }).DisableAntiforgery();
        deviceGroup.MapGet("/sites/{siteId}/connectors/{connectorId}/devices", GetDevicesHandler.HandleWithSiteIdAsync)
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            });
        deviceGroup.MapGet("/sites/{siteId}/connectors/{connectorId}/devices/{deviceId}", GetDeviceByIdHandler.HandleWithSiteIdAsync)
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            });

        var connectorGroup = group.MapGroup(string.Empty).WithOpenApi().WithTags("Connector");
        connectorGroup.MapGet("/connectors/{connectorId}", GetConnectorHandler.HandleAsync);
        connectorGroup.MapPost("/customers/{customerId}/connectors/export", ExportConnectorStateHandler.HandleAsync);
        connectorGroup.MapGet("/sites/{siteId}/connectors", GetConnectorHandler.HandleWithSiteIdAsync)
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            });

        group.MapPost("/logs", PostLogHandler.HandleAsync)
            .DisableAntiforgery()
            .WithOpenApi()
            .WithTags("Logs");
        return group;
    }
}
