namespace ConnectorCore.Extensions;

using ConnectorCore.Requests.Connector;
using ConnectorCore.Requests.ConnectorTypes;
using ConnectorCore.Requests.Logs;
using ConnectorCore.Requests.Scan;

internal static class Endpoints
{
    public static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder app)
    {
        var connectorEndpoints = app.MapGroup(string.Empty).WithOpenApi().WithTags("Connectors");

        connectorEndpoints.MapGet("connectors", GetConnectorsHandler.HandleWithNoSiteIdAsync);
        connectorEndpoints.MapGet("/sites/{siteId}/connectors", GetConnectorsHandler.HandleWithSiteIdAsync);
        connectorEndpoints.MapGet("/connectors/{connectorId}", GetConnectorByIdHandler.HandleWithNoSiteIdAsync);
        connectorEndpoints.MapGet("/sites/{siteId}/connectors/{connectorId}", GetConnectorByIdHandler.HandleWithSiteIdAsync);
        connectorEndpoints.MapGet("/connectors/bytype/{connectorTypeId}", GetConnectorByTypeIdHandler.HandleWithNoSiteIdAsync);
        connectorEndpoints.MapGet("/sites/{siteId}/connectors/bytype/{connectorTypeId}",
            GetConnectorByTypeIdHandler.HandleWithSiteIdAsync);
        connectorEndpoints.MapPost("/connectors/{connectorId}/enable", EnableConnectorHandler.HandleAsync);
        connectorEndpoints.MapPost("/connectors/{connectorId}/disable", DisableConnectorHandler.HandleAsync);
        connectorEndpoints.MapPost("/connectors/{connectorId}/archive", ArchiveConnectorHandler.HandleAsync);
        connectorEndpoints.MapGet("/customers/{customerId}/sites/{siteId}/iotHub", GetSiteIoTHubHandler.HandleAsync);
        connectorEndpoints.MapPost("/connectors", PostConnectorHandler.HandleAsync).DisableAntiforgery();
        connectorEndpoints.MapPut("/connectors", PutConnectorHandler.HandleAsync).DisableAntiforgery();
        connectorEndpoints.MapGet("{connectorId}/forImportValidation", GetConnectorForImportValidationHandler.HandleAsync);
        connectorEndpoints.MapPost("/customers/{customerId}/export", ExportConnectorStateHandler.HandleAsync).DisableAntiforgery();

        var connectorTypesEndpoints = app.MapGroup(string.Empty).WithOpenApi().WithTags("ConnectorTypes");

        connectorTypesEndpoints.MapGet("/ConnectorTypes", GetConnectorTypesHandler.HandleAsync);
        connectorTypesEndpoints.MapGet("/ConnectorTypes/{connectorTypeId}", GetConnectorTypesByIdHandler.HandleAsync);

        var scanEndpoints = app.MapGroup(string.Empty).WithOpenApi().WithTags("Scan");

        scanEndpoints.MapGet("/connectors/{connectorId}/scans", GetScansByConnectorIdHandler.HandleAsync);
        scanEndpoints.MapPost("/connectors/{connectorId}/scans", CreateScanHandler.HandleAsync).DisableAntiforgery();
        scanEndpoints.MapGet("/connectors/{connectorId}/scans/{scanId}", GetScanByIdHandler.HandleAsync);
        scanEndpoints.MapPatch("/connectors/{connectorId}/scans/{scanId}/stop", StopScanHandler.HandleAsync);
        scanEndpoints.MapPatch("/connectors/{connectorId}/scans/{scanId}", PatchScanHandler.HandleAsync);
        scanEndpoints.MapGet("/connectors/{connectorId}/scans/{scanId}/content", DownloadScannerDataHandler.HandleAsync);

        var logsEndpoints = app.MapGroup(string.Empty).WithOpenApi().WithTags("Logs");

        logsEndpoints.MapGet("/logs/healthcheck", GetLogHealthcheckHandler.HandleAsync);
        logsEndpoints.MapGet("/connectors/logs/latest", GetLatestConnectorLogsHandler.HandleAsync);
        logsEndpoints.MapGet("/connectors/{connectorId}/logs/latest", GetLatestConnectorLogsByIdHandler.HandleAsync);
        logsEndpoints.MapGet("/connectors/{connectorId}/logs/latest/{logId}/errors", GetConnectorErrorLogsHandler.HandleAsync);
        logsEndpoints.MapGet("/connectors/{connectorId}/logs", GetLogsForConnectorHandler.HandleAsync);
        logsEndpoints.MapPost("/logs", PostLogHandler.HandleAsync).DisableAntiforgery();
        return app;
    }
}
