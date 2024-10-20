using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Common;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;

using Willow.Data;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InspectionController : ControllerBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IInspectionService _inspectionService;
        private readonly IInspectionUsageService _inspectionUsageService;
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IReadRepository<Guid, Site> _siteRepo;
        public InspectionController(
            IDateTimeService dateTimeService,
            IInspectionService inspectionService,
            IInspectionUsageService inspectionUsageService,
            IImagePathHelper imagePathHelper,
            IReadRepository<Guid, Site> siteRepo)

        {
            _dateTimeService = dateTimeService;
            _inspectionService = inspectionService;
            _inspectionUsageService = inspectionUsageService;
            _imagePathHelper = imagePathHelper;
            _siteRepo = siteRepo;
        }

        [HttpPost("sites/{siteId}/zones")]
        [Authorize]
        [ProducesResponseType(typeof(ZoneDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateZone([FromRoute] Guid siteId, [FromBody] CreateZoneRequest request)
        {
            var zone = await _inspectionService.CreateZone(siteId, request);
            return Ok(ZoneDto.MapFromModel(zone));
        }

        [HttpGet("sites/{siteId}/zones")]
        [Authorize]
        [ProducesResponseType(typeof(ZoneDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetZones([FromRoute] Guid siteId, [FromQuery] bool? includeStatistics)
        {
            var zones = await _inspectionService.GetZones(siteId, includeStatistics ?? false);
            return Ok(ZoneDto.MapFromModels(zones));
        }

        /// <summary>
        /// Get zones by list of siteId
        /// </summary>
        /// <param name="siteIds">List of requested site Ids</param>
        /// <returns>Return list of zones for the requested siteIds</returns>
        [HttpPost("zones/bySiteIds")]
        [Authorize]
        [ProducesResponseType(typeof(ZoneDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetZonesBySiteIds([FromBody] List<Guid> siteIds)
        {

            var zones = await _inspectionService.GetZones(siteIds);
            return Ok(ZoneDto.MapFromModels(zones));
        }

        [HttpPut("sites/{siteId}/zones/{zoneId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateZone([FromRoute] Guid siteId, [FromRoute] Guid zoneId, UpdateZoneRequest updateZoneRequest)
        {
            await _inspectionService.UpdateZone(siteId, zoneId, updateZoneRequest);
            return NoContent();
        }

        [HttpGet("sites/{siteId}/zones/{zoneId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetZoneInspections([FromRoute] Guid siteId, [FromRoute] Guid zoneId)
        {
            var inspections = await _inspectionService.GetZoneInspections(siteId, zoneId);
            return Ok(InspectionDto.MapFromModels(inspections));
        }

        [HttpPost("sites/{siteId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateInspection([FromRoute] Guid siteId, [FromBody] CreateInspectionRequest request)
        {
            var inspection = await _inspectionService.CreateInspection(siteId, request);
            return Ok(InspectionDto.MapFromModel(inspection));
        }

        [HttpGet("sites/{siteId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteInspections([FromRoute] Guid siteId)
        {
            var inspections = await _inspectionService.GetSiteInspections(siteId);
            return Ok(InspectionDto.MapFromModels(inspections));
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateInspection([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, [FromBody] UpdateInspectionRequest request)
        {
            var inspection = await _inspectionService.UpdateInspection(siteId, inspectionId, request);
            return Ok(InspectionDto.MapFromModel(inspection));
        }
        [Obsolete("Use the endpoint without SiteId, inspections/{id}")]
        [HttpGet("sites/{siteId}/inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInspection([FromRoute] Guid siteId, [FromRoute] Guid inspectionId)
        {
            var inspection = await _inspectionService.GetInspection(siteId, inspectionId);
            return Ok(InspectionDto.MapFromModel(inspection));
        }

        [HttpGet("inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInspection([FromRoute] Guid inspectionId)
        {
            var inspection = await _inspectionService.GetInspection(inspectionId);
            return Ok(InspectionDto.MapFromModel(inspection));
        }

        [HttpGet("sites/{siteId}/inspectionUsage")]
        [ProducesResponseType(typeof(InspectionUsageDto), (int)HttpStatusCode.OK)]
        [Authorize]
        public async Task<IActionResult> GetInspectionUsage([FromRoute] Guid siteId, [FromQuery] InspectionUsagePeriod inspectionUsagePeriod)
        {
            var inspectionUsage = await _inspectionUsageService.GetInspectionUsage(siteId, inspectionUsagePeriod);
            return Ok(InspectionUsageDto.MapFromModel(inspectionUsage));
        }

        [HttpGet("sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/history")]
        [Authorize]
        [ProducesResponseType(typeof(CheckRecordDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCheckHistory(
            [FromRoute] Guid siteId,
            [FromRoute] Guid inspectionId,
            [FromRoute] Guid checkId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var site = await _siteRepo.Get(siteId);
            var now = _dateTimeService.UtcNow;
            var start = startDate ?? now.AddDays(-7);
            var end = endDate ?? now;
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be greater than the end date.");
            }
            var checkHistory = await _inspectionService.GetCheckHistory(siteId, site.CustomerId, inspectionId, checkId, start, end);
            return Ok(checkHistory);
        }

        /// <summary>
        /// Get check history report for one or all checks
        /// </summary>
        /// <param name="inspectionId">the inspection id</param>
        /// <param name="siteId">the id for the inspection's site</param>
        /// <param name="customerId">the id for the customer</param>
        /// <param name="checkId">the check id  to get report</param>
        /// <param name="startDate">the report start date</param>
        /// <param name="endDate">the report end date</param>
        /// <returns>List of the check records</returns>
        /// <exception cref="ArgumentException"></exception>
        [HttpGet("inspections/{inspectionId}/checks/history")]
        [Authorize]
        [ProducesResponseType(typeof(CheckRecordDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCheckHistory(
            [FromRoute] Guid inspectionId,
            [FromQuery] Guid siteId,
            [FromQuery] Guid customerId,
            [FromQuery] Guid? checkId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var now = _dateTimeService.UtcNow;
            var start = startDate ?? now.AddDays(-7);
            var end = endDate ?? now;
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date cannot be greater than the end date.");
            }
            var inspection = await _inspectionService.GetInspection(inspectionId);
            if (inspection.Checks == null || !inspection.Checks.Any())
            {
                return null;
            }
            var checkHistory = await _inspectionService.GetCheckHistory(siteId,customerId,inspectionId, checkId, start, end);
            return Ok(checkHistory);
        }

        [HttpPost("sites/{siteId}/zones/{zoneId}/archive")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ArchiveZone([FromRoute] Guid siteId, [FromRoute] Guid zoneId, [FromQuery] bool? isArchived)
        {
            await _inspectionService.ArchiveZone(siteId, zoneId, isArchived ?? true);
            return NoContent();
        }

        [HttpPost("sites/{siteId}/inspections/{inspectionId}/archive")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> ArchiveInspection([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, [FromQuery] bool? isArchived)
        {
            await _inspectionService.ArchiveInspection(siteId, inspectionId, isArchived ?? true);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/zones/{zoneId}/inspections/sortOrder")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdateInspectionSortOrder([FromRoute] Guid siteId, [FromRoute] Guid zoneId, [FromBody] UpdateInspectionSortOrderRequest request)
        {
            await _inspectionService.UpdateSortOrder(siteId, zoneId, request);
            return NoContent();
        }

		[HttpGet("sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory/{count}")]
		[Authorize]
		[ProducesResponseType(typeof(CheckRecordDto), (int)HttpStatusCode.OK)]
		public async Task<IActionResult> GetCheckSubmittedHistory(
			[FromRoute] Guid siteId,
			[FromRoute] Guid inspectionId,
			[FromRoute] Guid checkId,
			[FromRoute] int count)
		{
			var site = await _siteRepo.Get(siteId);
			var checkHistory = await _inspectionService.GetCheckSubmittedHistory(siteId, inspectionId, checkId, count);
			return Ok(CheckRecordDto.MapFromModels(checkHistory, _imagePathHelper, site.CustomerId, siteId));
		}

		/// <summary>
		/// Inspection with multi assets
		/// will create inspection for each asset
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost("sites/{siteId}/inspections/batch-create")]
		[Authorize]
		[ProducesResponseType(typeof(List<InspectionDto>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.BadRequest)]
		[SwaggerOperation("Create Inspection for each asset in the AssetList", Tags = new[] { "Inspection" })]
		public async Task<ActionResult<List<InspectionDto>>> CreateInspections([FromRoute] Guid siteId, [FromBody] CreateInspectionsRequest request)
		{
			if(request.AssetList is not null && request.AssetList.Any())
			{
				var inspections = await _inspectionService.CreateInspections(siteId, request);
				return Ok(InspectionDto.MapFromModels(inspections));
			}
			else
			{
				return BadRequest("Asset list is null or empty");
			}
			
		}
	}
}
