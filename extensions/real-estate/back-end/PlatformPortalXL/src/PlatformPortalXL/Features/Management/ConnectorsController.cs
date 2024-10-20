using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.DirectoryCore;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;

using Willow.Api.DataValidation;

namespace PlatformPortalXL.Features.Management
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ConnectorsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IConnectorApiService _connectorApi;
        private readonly ISiteApiService _siteApi;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly IConnectorService _connectorService;
        private readonly IScanValidationService _scanValidationService;
        private static readonly string[] AllowedConnectionType = new[] { "vm", "iotedge", "streamanalyticseventhub", "streamanalyticsiothub", "publicapi" };

        public ConnectorsController(IAccessControlService accessControl,
                                    IConnectorApiService connectorApi,
                                    ISiteApiService siteApi,
                                    IDirectoryApiService directoryApi,
                                    IDigitalTwinService digitalTwinService,
                                    IConnectorService connectorService,
                                    IScanValidationService scanValidationService)
        {
            _accessControl = accessControl;
            _connectorApi = connectorApi;
            _siteApi = siteApi;
            _directoryApi = directoryApi;
            _digitalTwinService = digitalTwinService;
            _connectorService = connectorService;
            _scanValidationService = scanValidationService;
        }

        [HttpPost("sites/{siteId}/connectors")]
        [Authorize]
        [SwaggerOperation("Create Connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<ConnectorDto>> CreateConnector([FromRoute] Guid siteId, [FromBody] CreateConnectorRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var validationErrors = await ValidateConnectorRequest(request.Name, request.ConnectionType, request.ConnectorTypeId, request.Configuration);
            if (validationErrors.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new ValidationError { Items = validationErrors });
            }
            var site = await _siteApi.GetSite(siteId);
            var newConnector = await _connectorApi.CreateConnectorAsync(new Connector
            {
                Configuration = request.Configuration,
                ErrorThreshold = 10,
                IsEnabled = false,
                IsLoggingEnabled = true,
                SiteId = siteId,
                Name = request.Name,
                ClientId = site.CustomerId,
                ConnectorTypeId = request.ConnectorTypeId,
                ConnectionType = request.ConnectionType,
                IsArchived = false
            });
            var dto = await _connectorService.MapConnector(newConnector, true);
            return await SetConnectorPointsCount(dto);
        }

        [HttpPut("sites/{siteId}/connectors/{connectorId}")]
        [Authorize]
        [SwaggerOperation("Update Connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<ConnectorDto>> UpdateConnector([FromRoute] Guid siteId,
            [FromRoute] Guid connectorId, [FromBody] UpdateConnectorRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var connector = await _connectorApi.GetConnectorById(siteId, connectorId);

            var validationErrors = new List<ValidationErrorItem>();

            if (!string.IsNullOrWhiteSpace(request.Configuration))
            {
                validationErrors = await ValidateConnectorConfiguration(connector.ConnectorTypeId, request.Configuration);
                if (validationErrors.Any())
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity,
                                      new ValidationError { Items = validationErrors });
                }

                connector.Configuration = request.Configuration;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                validationErrors.Add(new ValidationErrorItem { Name = "Name", Message = $"Name is required" });

                return StatusCode(StatusCodes.Status422UnprocessableEntity,
                                     new ValidationError { Items = validationErrors });
            }

            connector.ErrorThreshold = request.ErrorThreshold ?? connector.ErrorThreshold;
            connector.IsLoggingEnabled = request.IsLoggingEnabled ?? connector.IsLoggingEnabled;
            connector.Name = request.Name;

            var updatedConnector = await _connectorApi.UpdateConnectorAsync(connector);
            var dto = await _connectorService.MapConnector(updatedConnector, true);
            return await SetConnectorPointsCount(dto);
        }


        private async Task<List<ValidationErrorItem>> ValidateConnectorRequest(string name, string connectionType, Guid connectorTypeId, string configuration)
        {
            var errors = new List<ValidationErrorItem>();
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ValidationErrorItem { Name = "Name", Message = $"Name is required" });
            }

            if (string.IsNullOrWhiteSpace(connectionType))
            {
                errors.Add(new ValidationErrorItem { Name = "ConnectionType", Message = "Connection Type is required" });
            }
            else if (!AllowedConnectionType.Contains(connectionType))
            {
                errors.Add(new ValidationErrorItem
                { Name = "ConnectionType", Message = "Connection Type is out of allowed values" });
            }

            if (connectorTypeId == Guid.Empty)
            {
                errors.Add(new ValidationErrorItem { Name = "ConnectorType", Message = "Connector Type is required" });
            }
            else
            {
                errors.AddRange(await ValidateConnectorConfiguration(connectorTypeId, configuration));
            }
            return errors;
        }

        private async Task<List<ValidationErrorItem>> ValidateConnectorConfiguration(Guid connectorTypeId, string configuration)
        {
            var errors = new List<ValidationErrorItem>();
            var connectorType = await _connectorApi.GetConnectorTypeAsync(connectorTypeId);
            var schemaColumns = await _connectorApi.GetSchemaColumnsAsync(connectorType.ConnectorConfigurationSchemaId);
            var configurationColumns = JsonSerializerHelper.Deserialize<Dictionary<string, object>>(configuration);
            foreach (var schemaColumn in schemaColumns)
            {
                if (schemaColumn.IsRequired && (!configurationColumns.TryGetValue(schemaColumn.Name, out var columnValue) ||
                                                columnValue == null))
                {
                    errors.Add(new ValidationErrorItem
                    { Name = schemaColumn.Name, Message = $"{schemaColumn.Name} is required" });
                }
            }

            return errors;
        }

        [HttpPost("sites/{siteId}/connectors/{connectorId}/password")]
        [Authorize]
        [SwaggerOperation("Get Connectors", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<ConnectorAccountPasswordDto>> GeneratePasswordForConnector([FromRoute] Guid siteId, [FromRoute] Guid connectorId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var site = await _siteApi.GetSite(siteId);
            var password = PasswordGenerator.GeneratePassword(12, 2);
            await _directoryApi.CreateConnectorAccount(siteId, site.CustomerId, connectorId,
                new CreateConnectorAccountRequest { Password = password });
            return new ConnectorAccountPasswordDto { Password = password };
        }

        [HttpGet("sites/{siteId}/connectors")]
        [Authorize]
        [SwaggerOperation("Get Connectors", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<IEnumerable<ConnectorDto>>> GetConnectors([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);

            var connectors = await _connectorService.GetConnectors(siteId);
            foreach (var dto in connectors)
            {
                await SetConnectorPointsCount(dto);
            }

            return connectors;
        }

        [HttpGet("sites/{siteId}/connectors/{connectorId}")]
        [Authorize]
        [SwaggerOperation("Get Connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<ConnectorDto>> GetConnector([FromRoute] Guid siteId, [FromRoute] Guid connectorId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);

            var connectorDto = await _connectorService.GetConnector(siteId, connectorId);

            await SetConnectorPointsCount(connectorDto);

            return connectorDto;
        }

        [HttpGet("sites/{siteId}/connectors/{connectorId}/logs")]
        [Authorize]
        [SwaggerOperation("Get Connector Logs", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<List<ConnectorLogDto>>> GetConnectorLogs([FromRoute] Guid siteId, [FromRoute] Guid connectorId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var logs = await _connectorApi.GetLatestConnectorLogsAsync(connectorId, count: 1000, includeErrors: false, source: null);
            return Ok(ConnectorLogDto.MapFrom(logs));
        }

        [HttpPut("sites/{siteId}/connectors/{connectorId}/isEnabled")]
        [Authorize]
        [SwaggerOperation("Set isEnable flag for connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> SetConnectorEnabled([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromQuery, BindRequired] bool enabled)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            await _connectorApi.SetConnectorEnabled(connectorId, enabled);
            return NoContent();
        }

        [HttpGet("sites/{siteId}/connectors/{connectorId}/logs/{logId}/content")]
        [Authorize]
        [SwaggerOperation("Download Connector Log Details", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> DownloadConnectorLogsDetails([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromRoute] long logId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var logErrorsResponse = await _connectorApi.GetConnectorLogErrorsAsync(connectorId, logId);

            await using var memoryStream = new MemoryStream();
            await using var writer = new StreamWriter(memoryStream);
            await writer.WriteLineAsync("Messages");
            var messages = logErrorsResponse.Errors
                .Replace("\r\n", "\n")
                .Split("\n")
                .Select(x => $"'{x}'");
            foreach (var message in messages)
            {
                await writer.WriteLineAsync(message);
            }
            await writer.FlushAsync();
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream.ToArray(), "application/octet-stream", "log.csv");
        }

        [HttpGet("sites/{siteId}/connectors/{connectorId}/scans")]
        [Authorize]
        [SwaggerOperation("Get Connector Scans", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<List<ConnectorScanDto>>> GetConnectorScans([FromRoute] Guid siteId, [FromRoute] Guid connectorId)
        {
            // no permission check here because scanner accounts do not exist in directory core, but need to use this api
            var scans = await _connectorApi.GetConnectorScansAsync(connectorId);
            return Ok(ConnectorScanDto.MapFrom(scans.OrderByDescending(x => x.CreatedAt).ToList()));
        }

        [HttpPost("sites/{siteId}/connectors/{connectorId}/scans")]
        [Authorize]
        [SwaggerOperation("Create Connector Scan", Tags = new[] { "Connectors" })]
        public async Task<ActionResult<ConnectorScanDto>> CreateScan([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromBody] CreateConnectorScanRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var validationErrors = await _scanValidationService.ValidateCreateConnectorScan(siteId, connectorId, request.Configuration);
            if (validationErrors.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new ValidationError { Items = validationErrors });
            }
            var currentUser = await _directoryApi.GetUser(this.GetCurrentUserId());
            var connectorScan = new ConnectorScan
            {
                Message = request.Message,
                DevicesToScan = request.DevicesToScan,
                CreatedBy = currentUser.Email,
                Configuration = request.Configuration
            };
            var scan = await _connectorApi.CreateConnectorScanAsync(connectorId, connectorScan);
            return Ok(ConnectorScanDto.MapFrom(scan));
        }

        [HttpPut("sites/{siteId}/connectors/{connectorId}/scans/{scanId}")]
        [Authorize]
        [SwaggerOperation("Update Connector Scan", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> UpdateConnectorScan([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromRoute] Guid scanId, [FromBody] UpdateConnectorScanRequest request)
        {
            // no permission check here because scanner accounts do not exist in directory core, but need to use this api
            await _connectorApi.UpdateConnectorScanAsync(connectorId, scanId, request);
            return NoContent();
        }

        [HttpPost("sites/{siteId}/connectors/{connectorId}/scans/{scanId}/stop")]
        [Authorize]
        [SwaggerOperation("Stop Scanning For Connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> StopConnectorScan([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromRoute] Guid scanId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var scan = await _connectorApi.GetConnectorScanByIdAsync(connectorId, scanId);
            if (scan.Status != ScanStatus.Scanning && scan.Status != ScanStatus.New)
            {
                var validationErrorItem = new ValidationErrorItem { Name = nameof(scanId), Message = "Only New and Scanning requests may be stopped." };
                var items = new List<ValidationErrorItem> { validationErrorItem };
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new ValidationError { Items = items });
            }

            await _connectorApi.StopConnectorScanAsync(connectorId, scanId);
            return NoContent();
        }

        [HttpGet("sites/{siteId}/connectors/{connectorId}/scans/{scanId}/content")]
        [Authorize]
        [SwaggerOperation("Download Connector Scan Details", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> DownloadConnectorScanDetails([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromRoute] Guid scanId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            var scan = await _connectorApi.GetConnectorScanByIdAsync(connectorId, scanId);
            if (scan.Status != ScanStatus.Finished && scan.Status != ScanStatus.Failed)
            {
                var validationErrorItem = new ValidationErrorItem { Name = nameof(scanId), Message = "Only data for  Finished or Failed requests can be downloaded." };
                var items = new List<ValidationErrorItem> { validationErrorItem };
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new ValidationError { Items = items });
            }


            var stream = await _connectorApi.DownloadScanResultAsStream(connectorId, scanId);
            return File(stream, "application/zip", $"scan_{scan.Id}.zip");
        }

        private async Task<ConnectorDto> SetConnectorPointsCount(ConnectorDto connectorDto)
        {
            if (connectorDto.PointsCount != 0)
            {
                return connectorDto;
            }
            var liveDataService = await _digitalTwinService.GetLiveDataServiceAsync(connectorDto.SiteId);
            connectorDto.PointsCount = await liveDataService.GetConnectorPointCount(connectorDto.SiteId, connectorDto.Id);

            return connectorDto;
        }

        [HttpPut("sites/{siteId}/connectors/{connectorId}/isArchived")]
        [Authorize]
        [SwaggerOperation("Set isArchived flag for connector", Tags = new[] { "Connectors" })]
        public async Task<ActionResult> SetConnectorArchived([FromRoute] Guid siteId, [FromRoute] Guid connectorId, [FromBody] bool archive)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageConnectors, siteId);
            await _connectorApi.SetConnectorArchived(connectorId, archive);
            return NoContent();
        }
    }
}
