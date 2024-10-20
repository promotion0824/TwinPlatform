using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using Willow.Data;
using Willow.ExceptionHandling.Exceptions;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class UserInspectionsController : ControllerBase
    {
        private readonly IUserInspectionService _inspectionService;
        private readonly IImagePathHelper _helper;
        private readonly IReadRepository<Guid, Site> _siteRepo;

        public UserInspectionsController(IUserInspectionService inspectionService, IReadRepository<Guid, Site> siteRepo, IImagePathHelper helper)
        {
            _inspectionService = inspectionService;
            _siteRepo = siteRepo;
            _helper = helper;
        }

        [HttpGet("sites/{siteId}/users/{userId}/zones")]
        [Authorize]
        [ProducesResponseType(typeof(List<ZoneDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserZones([FromRoute] Guid siteId, [FromRoute] Guid userId, [FromQuery] bool? includeStatistics)
        {
            includeStatistics = includeStatistics ?? false;
            var zones = await _inspectionService.GetUserZones(siteId, userId, includeStatistics.Value);
            return Ok(ZoneDto.MapFromModels(zones));
        }

        [HttpGet("sites/{siteId}/users/{userId}/zones/{zoneId}")]
        [Authorize]
        [ProducesResponseType(typeof(ZoneDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserZone([FromRoute] Guid siteId, [FromRoute] Guid userId, [FromRoute] Guid zoneId, [FromQuery] bool? includeStatistics)
        {
            includeStatistics = includeStatistics ?? false;
            var zone = await _inspectionService.GetUserZone(siteId, userId, zoneId, includeStatistics.Value);
            return Ok(ZoneDto.MapFromModel(zone));
        }

        [HttpGet("sites/{siteId}/users/{userId}/zones/{zoneId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetUserZoneInspections([FromRoute] Guid siteId, [FromRoute] Guid userId, [FromRoute] Guid zoneId)
        {
            var inspections = await _inspectionService.GetUserZoneInspections(siteId, userId, zoneId);
            return Ok(InspectionDto.MapFromModels(inspections));
        }

        [HttpGet("sites/{siteId}/inspections/{inspectionId}/lastRecord")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionRecordDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInspectionLastRecord([FromRoute] Guid siteId, [FromRoute] Guid inspectionId)
        {
            var site = await _siteRepo.Get(siteId);
            var inspection = await _inspectionService.GetInspectionAndChecks(siteId, inspectionId, true);
            if (!inspection.LastRecordId.HasValue)
            {
                throw new NotFoundException( new { InspectionId = inspectionId });
            }
			var inspectionRecord = await _inspectionService.GetInspectionRecord(inspection.LastRecordId.Value);
            var checkRecords = await _inspectionService.GetCheckRecords(siteId, inspection.LastRecordId.Value);
            var dto = new InspectionRecordDto
            {
                Id = inspection.LastRecordId.Value,
                InspectionId = inspectionId,
				EffectiveAt = inspectionRecord.EffectiveDate,
                Inspection = InspectionDto.MapFromModel(inspection),
                CheckRecords = CheckRecordDto.MapFromModels(checkRecords)
            };
			switch (inspection.FrequencyUnit)
			{
				case SchedulingUnit.Hours:	dto.ExpiresAt = inspectionRecord.EffectiveDate.AddHours(inspection.Frequency); break;
				case SchedulingUnit.Days:	dto.ExpiresAt = inspectionRecord.EffectiveDate.AddDays(inspection.Frequency); break;
				case SchedulingUnit.Weeks:	dto.ExpiresAt = inspectionRecord.EffectiveDate.AddDays(inspection.Frequency * 7); break;
				case SchedulingUnit.Months: dto.ExpiresAt = inspectionRecord.EffectiveDate.AddMonths(inspection.Frequency); break;
				case SchedulingUnit.Years:	dto.ExpiresAt = inspectionRecord.EffectiveDate.AddYears(inspection.Frequency); break;
				default: break;
			}

			foreach(var checkRecordDto in dto.CheckRecords)
            {
                var cr = checkRecords.Single(x => x.Id == checkRecordDto.Id);
                checkRecordDto.Attachments = AttachmentDto.MapFromCheckRecordModels(cr.Attachments, _helper, site.CustomerId, inspection.SiteId, cr.Id);
            }
            return Ok(dto);
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}")]
        [Authorize]
        [ProducesResponseType(typeof(SubmitCheckRecordResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SubmitCheckRecord([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, [FromRoute] Guid checkRecordId, [FromBody] SubmitCheckRecordRequest request)
        {
            var result = await _inspectionService.SubmitCheckRecord(siteId, inspectionId, checkRecordId, request, null);
            return Ok(result);
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}/{inspectionRecordId}/checkRecords/{checkRecordId}")]
        [Authorize]
        [ProducesResponseType(typeof(SubmitCheckRecordResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCheckRecord([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, 
            [FromRoute] Guid inspectionRecordId, [FromRoute] Guid checkRecordId, [FromBody] SubmitCheckRecordRequest request)
        {
            var result = await _inspectionService.SubmitCheckRecord(siteId, inspectionId, checkRecordId, request, inspectionRecordId);
            return Ok(result);
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}/insight")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateCheckRecordInsight(
            [FromRoute] Guid siteId, 
            [FromRoute] Guid inspectionId, 
            [FromRoute] Guid checkRecordId, 
            [FromBody] UpdateCheckRecordInsightRequest request)
        {
            await _inspectionService.UpdateCheckRecordInsight(siteId, inspectionId, checkRecordId, request.InsightId);
            return NoContent();
        }
    }
}
