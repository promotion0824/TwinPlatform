using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Commands.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.LiveDataApi;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using Willow.Platform.Users;

namespace PlatformPortalXL.Features.Commands
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class CommandsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IConnectorApiService _connectorApiService;
        private readonly IInsightApiService _insightsApiService;
        private readonly IDirectoryApiService _directoryApiService;
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly ILiveDataApiService _liveDataApiService;
        private readonly IUserService _userService;
        private readonly ILogger<CommandsController> _logger;
        private readonly IInsightApiService _insightApi;

        public CommandsController(
            IAccessControlService accessControl,
            IConnectorApiService connectorApiService,
            IInsightApiService insightApiService,
            IDirectoryApiService directoryApiService,
            IDigitalTwinService digitalTwinService,
            ILiveDataApiService liveDataApiService,
            IUserService userService,
            ILogger<CommandsController> logger,
            IInsightApiService insightApi
            )
        {
            _accessControl = accessControl;
            _connectorApiService = connectorApiService;
            _insightsApiService = insightApiService;
            _directoryApiService = directoryApiService;
            _digitalTwinService = digitalTwinService;
            _liveDataApiService = liveDataApiService;
            _userService = userService;
            _logger = logger;
            _insightApi = insightApi;
        }

        [HttpGet("sites/{siteId}/insights/{insightId}/commands")]
        [Authorize]
        [SwaggerOperation("Gets available commands and command history for a given insight id", Tags = new [] { "Commands" })]
        public async Task<ActionResult<InsightSetPointCommandInfoDto>> GetCommandsForInsight([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var siteFeatures = await _directoryApiService.GetSiteFeatures(siteId);
            if (!siteFeatures.IsCommandsEnabled)
            {
                return null;
            }

            var insight = await _insightsApiService.GetInsight(siteId, insightId);

            if (insight == null)
            {
                throw new NotFoundException().WithData(new { insightId });
            }

            if (insight.EquipmentId == null)
            {
                return null;
            }

            var history = await _connectorApiService.GetSetPointCommandsAsync(siteId, insight.Id, insight.EquipmentId);

            var result = new InsightSetPointCommandInfoDto
            {
                Available = await GetAvailableCommandForInsightAsync(siteId, insight),
                History = await SetPointCommandDto.MapFrom(history, _userService)
            };

            return Ok(result);
        }

        [HttpPost("sites/{siteId}/commands")]
        [Authorize]
        [SwaggerOperation("Create a new command", Tags = new[] { "Commands" })]
        public async Task<ActionResult<SetPointCommandDto>> PostCommand([FromRoute] Guid siteId, [FromBody] CreateCommandRequest request)
        {
	        var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserId, Permissions.ManageSites, siteId);

            if (request.DesiredDurationMinutes > 10080)
            {
                return UnprocessableEntity(new ValidationError { Items = new List<ValidationErrorItem> {
                    new ValidationErrorItem { Name = nameof(request.DesiredDurationMinutes), Message = "Duration must not be greater than 7 days" }
                }});
            }

            var insight = await _insightsApiService.GetInsight(siteId, request.InsightId);

            if (insight == null)
            {
                throw new NotFoundException().WithData(new { request.InsightId });
            }
            if (insight.EquipmentId == null || insight.EquipmentId.Value.Equals(Guid.Empty))
            {
                return UnprocessableEntity(new ValidationError { Items = new List<ValidationErrorItem> {
                    new ValidationErrorItem { Name = nameof(request.InsightId), Message = "Insight must be for a specific equipment in order to use commands" }
                }});
            }

            var pointsService = await _digitalTwinService.GetLiveDataServiceAsync(siteId);
            var point = await pointsService.GetPointAsync(siteId, request.PointId);
            var setPoint = await pointsService.GetPointAsync(siteId, request.SetPointId);

            var pointDevice = await pointsService.GetDeviceAsync(siteId, point.DeviceId);
            var setPointDevice = await pointsService.GetDeviceAsync(siteId, setPoint.DeviceId);

            if (pointDevice.ConnectorId != setPointDevice.ConnectorId)
            {
                return UnprocessableEntity(new ValidationError { Items = new List<ValidationErrorItem> { new ValidationErrorItem { Name = nameof(request.PointId), Message = "Point and SetPoint must be linked to the same connector" } } });
            }

            var model = new SetPointCommand
            {
                InsightId = request.InsightId,
                EquipmentId = insight.EquipmentId.Value,
                ConnectorId = setPointDevice.ConnectorId,
                DesiredDurationMinutes = request.DesiredDurationMinutes,
                CurrentReading = request.CurrentReading,
                DesiredValue = request.DesiredValue,
                OriginalValue = request.OriginalValue,
                PointId = point.EntityId,
                SetPointId = setPoint.EntityId,
                SiteId = siteId,
                Unit = point.Unit,
                Type = request.Type,
                CreatedBy = this.GetCurrentUserId()
            };

            var Available = await GetAvailableCommandForInsightAsync(siteId, insight);
            if (Available != null && Available.DesiredValueLimitation != 0)
            {
                if (model.DesiredValue + Available.DesiredValueLimitation < model.OriginalValue || model.DesiredValue - Available.DesiredValueLimitation > model.OriginalValue)
                {
                    return UnprocessableEntity(new ValidationError { Items = new List<ValidationErrorItem> {
                    new ValidationErrorItem { Name = nameof( model.DesiredValue ), Message = $"The variation range exceeds the limitation : {Available.DesiredValueLimitation}"}}});
                }
            }
            var result = await _connectorApiService.CreateSetPointCommandAsync(model);

            await _insightsApiService.UpdateBatchInsightStatusAsync(siteId, new BatchUpdateInsightStatusRequest
            {
                Ids = new List<Guid> { result.InsightId },
                Status = InsightStatus.InProgress,
                UpdatedByUserId = currentUserId
            });

            return Created($"/sites/{siteId}/commands/{result.Id}", await SetPointCommandDto.MapFromAsync(result, _userService));
        }

        [HttpPut("sites/{siteId}/commands/{setPointCommandId}")]
        [Authorize]
        [SwaggerOperation("Update a command", Tags = new[] { "Commands" })]
        public async Task<ActionResult<SetPointCommandDto>> PutCommand([FromRoute] Guid siteId, [FromRoute] Guid setPointCommandId, [FromBody] UpdateCommandRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var model = await _connectorApiService.GetSetPointCommandAsync(siteId, setPointCommandId);

            model.DesiredDurationMinutes = request.DesiredDurationMinutes;
            model.DesiredValue = request.DesiredValue;


            var insight = await _insightApi.GetInsight(siteId, model.InsightId);
            var Available = await GetAvailableCommandForInsightAsync(siteId, insight);
            if (Available != null && Available.DesiredValueLimitation != 0)
            {
                if (model.DesiredValue + Available.DesiredValueLimitation < model.OriginalValue || model.DesiredValue - Available.DesiredValueLimitation > model.OriginalValue)
                {
                    return UnprocessableEntity(new ValidationError { Items = new List<ValidationErrorItem> {
                    new ValidationErrorItem { Name = nameof( model.DesiredValue ), Message = $"The variation range exceeds the limitation : {Available.DesiredValueLimitation}"}}});
                }
            }
            var result = await _connectorApiService.UpdateSetPointCommandAsync(model);
            return Ok(await SetPointCommandDto.MapFromAsync(result, _userService));
        }

        [HttpDelete("sites/{siteId}/commands/{setPointCommandId}")]
        [Authorize]
        [SwaggerOperation("Deletes a command", Tags = new[] { "Commands" })]
        public async Task<ActionResult> DeleteCommand([FromRoute] Guid siteId, [FromRoute] Guid setPointCommandId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            await _connectorApiService.DeleteSetPointCommandAsync(siteId, setPointCommandId);
            return NoContent();
        }

        private async Task<AvailableSetPointCommandDto> GetAvailableCommandForInsightAsync(Guid siteId, Insight insight)
        {
            var configurations = await _connectorApiService.GetSetPointCommandConfigurationsAsync();

            var matchingConfigurations = FindMatchingConfigurationsForInsight(insight.Name, configurations);

            if (matchingConfigurations.Any())
            {
                var liveDataService = await _digitalTwinService.GetLiveDataServiceAsync(siteId);
                var equipment = await liveDataService.GetAssetByEquipmentIdAsync(siteId, insight.EquipmentId.Value);

                foreach (var config in matchingConfigurations)
                {
                    var availableCommand = await GetCommandForPointsAsync(insight, equipment, config);

                    if (availableCommand != null)
                    {
                        return availableCommand;
                    }
                }
            }

            return null;
        }

        private List<SetPointCommandConfiguration> FindMatchingConfigurationsForInsight(string name, List<SetPointCommandConfiguration> configurations)
        {
            return configurations.Where(c => name.Contains(c.InsightName, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        private async Task<AvailableSetPointCommandDto> GetCommandForPointsAsync(Insight insight, Asset equipment, SetPointCommandConfiguration configuration)
        {
            var setPoint = configuration.FindSetPoint(equipment.Points);
            var point = configuration.FindPoint(equipment.Points);

            if (point != null && setPoint != null && point.Id != setPoint.Id && point.Unit == setPoint.Unit)
            {
                var currentReading = await GetCurrentPointValueAsync(insight.CustomerId, point.TwinId);
                var originalValue = await GetCurrentPointValueAsync(insight.CustomerId, setPoint.TwinId);

                if (currentReading != null)
                {
                    return new AvailableSetPointCommandDto
                    {
                        InsightId = insight.Id,
                        PointId = point.Id,
                        SetPointId = setPoint.Id,
                        Unit = setPoint.Unit,
                        CurrentReading = currentReading.Value,
                        OriginalValue = originalValue.GetValueOrDefault(),
                        Type = configuration.Type,
                        DesiredValueLimitation = configuration.DesiredValueLimitation
                    };
                }
                else
                {
                    LogLiveDataErrors(insight, configuration.Description, point, currentReading, originalValue);
                }
            }
            else
            {
                LogPointTagErrors(insight, configuration.Description, setPoint, point);
            }

            return null;
        }

        private void LogPointTagErrors(Insight insight, string description, AssetPoint setPoint, AssetPoint point)
        {
            if (point == null)
            {
                _logger.LogInformation("Command {Description} not available for insight {InsightId} on equipment {EquipmentId} because there is no point tagged as the data source",
                    description, insight.Id, insight.EquipmentId);
            }
            if (setPoint == null)
            {
                _logger.LogInformation("Command {Description} not available for insight {InsightId} on equipment {EquipmentId} because there is no set point tagged",
                    description, insight.Id, insight.EquipmentId);
            }
            if (point != null && setPoint != null && point.Unit != setPoint.Unit)
            {
                _logger.LogInformation("Command {Description} not available for insight {InsightId} on equipment {EquipmentId} because the read point unit ({ReadPointUnit}) is different to the set point unit ({SetPointUnit})",
                    description, insight.Id, insight.EquipmentId, point.Unit, setPoint.Unit);
            }
        }

        private void LogLiveDataErrors(Insight insight, string description, AssetPoint point, decimal? currentReading, decimal? originalValue)
        {
            if (currentReading == null)
            {
                _logger.LogInformation("Command {Description} not available for insight {InsightId} on equipment {EquipmentId} because we could not find the current value for the read point {ReadPointId}",
                    description, insight.Id, insight.EquipmentId, point.Id);
            }
            if (originalValue == null)
            {
                _logger.LogInformation("Command {Description} not available for insight {InsightId} on equipment {EquipmentId} because we could not find the current value for the set point {SetPointId}",
                    description, insight.Id, insight.EquipmentId, point.Id);
            }
        }

        private async Task<decimal?> GetCurrentPointValueAsync(Guid customerId, string twinId)
        {
            var trendLogs = await _liveDataApiService.GetTimeSeriesAnalogData(customerId, twinId, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow, null);
            return (trendLogs.LastOrDefault())?.Average;
        }
    }
}
