using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Dto;
using MobileXL.Features.Inspections.Requests;
using MobileXL.Features.Inspections.Response;
using MobileXL.Models;
using MobileXL.Security;
using MobileXL.Services;
using MobileXL.Services.Apis.DigitalTwinApi;
using MobileXL.Services.Apis.DirectoryApi;
using MobileXL.Services.Apis.InsightApi;
using MobileXL.Services.Apis.SiteApi;
using MobileXL.Services.Apis.WorkflowApi;
using MobileXL.Services.Apis.WorkflowApi.Requests;
using MobileXL.Services.Apis.WorkflowApi.Responses;
using Swashbuckle.AspNetCore.Annotations;

using Willow.Api.Client;
using Willow.Common;

namespace MobileXL.Features.Inspections
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InspectionsController : ControllerBase
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;
        private readonly IInsightApiService _insightApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly IDigitalTwinApiService _digitalTwinApiService;

        public InspectionsController(
            IDateTimeService dateTimeService,
            IAccessControlService accessControl,
            IDirectoryApiService directoryApi,
            ISiteApiService siteApi,
            IInsightApiService insightApi,
            IWorkflowApiService workflowApi,
            IDigitalTwinApiService digitalTwinApiService,
            IImageUrlHelper imageUrlHelper)
        {
            _dateTimeService = dateTimeService;
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _siteApi = siteApi;
            _insightApi = insightApi;
            _workflowApi = workflowApi;
            _imageUrlHelper = imageUrlHelper;
            _digitalTwinApiService=digitalTwinApiService;
        }

        [HttpGet("sites/{siteId}/inspectionZones")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(InspectionZoneDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of inspection zones for the given site", Tags = new [] { "Inspections" })]
        public async Task<IActionResult> GetInspectionZones([FromRoute] Guid siteId)
        {
            var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var zones = await _workflowApi.GetInspectionZones(siteId, userId);
            return Ok(InspectionZoneDto.Map(zones));
        }

        [HttpGet("sites/{siteId}/inspectionZones/{inspectionZoneId}")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(InspectionZoneDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of inspection zones for the given site", Tags = new [] { "Inspections" })]
        public async Task<IActionResult> GetInspectionZone([FromRoute] Guid siteId, [FromRoute] Guid inspectionZoneId)
        {
            var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var zone = await _workflowApi.GetInspectionZone(siteId, userId, inspectionZoneId);
            return Ok(InspectionZoneDto.Map(zone));
        }

        [HttpGet("sites/{siteId}/inspectionZones/{inspectionZoneId}/inspections")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(List<InspectionDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of inspections for the given site and zone", Tags = new [] { "Inspections" })]
        public async Task<IActionResult> GetInspectionsByZoneId([FromRoute] Guid siteId, [FromRoute] Guid inspectionZoneId)
        {
            var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var inspections = await _workflowApi.GetInspectionsByZoneId(siteId, userId, inspectionZoneId);
            var inspectionDtos = await EnrichInspections(siteId, inspections);

            return Ok(inspectionDtos);
        }

        [HttpGet("sites/{siteId}/inspections/{inspectionId}/lastRecord")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(InspectionRecordDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets last record for given inspection", Tags = new [] { "Inspections" })]
        public async Task<IActionResult> GetInspectionLastRecord([FromRoute] Guid siteId, [FromRoute] Guid inspectionId)
        {
            var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var inspectionRecord = await _workflowApi.GetInspectionLastRecord(siteId, inspectionId);

            var inspections = await EnrichInspections(siteId, new List<Inspection> { inspectionRecord.Inspection });

            var inspectionRecordDto = InspectionRecordDto.Map(inspectionRecord, _imageUrlHelper);
            inspectionRecordDto.Inspection = inspections.FirstOrDefault();

            return Ok(inspectionRecordDto);
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}/lastRecord/checkRecords/{checkRecordId}")]
        [MobileAuthorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Mark record as checked", Tags = new [] { "Inspections" })]
        public async Task<IActionResult> SubmitCheckRecord([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, [FromRoute] Guid checkRecordId, [FromBody] SubmitCheckRecordRequest request)
        {
            var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var site = await _siteApi.GetSite(siteId);
           
            WorkflowSubmitCheckRecordResponse response;
            try
            {
	            var userFullName = await _workflowApi.GetUserFullname(userId, this.GetCurrentUserType(),site.CustomerId);
				response = await _workflowApi.SubmitCheckRecord(siteId, inspectionId, checkRecordId, new WorkflowSubmitCheckRecordRequest
                {
                    Notes = request.Notes,
                    NumberValue = request.NumberValue,
                    StringValue = request.StringValue,
                    DateValue = request.DateValue,
                    SubmittedUserId = userId,
					SubmittedUserFullname = userFullName,
					TimeZoneId = site.TimeZoneId,
                    Attachments = request.Attachments
                });
            }
            catch (RestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var error = new ValidationError();
                    error.Items.Add(new ValidationErrorItem("checkRecord", "An error has occurred. Please refresh."));
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
                }
                throw;
            }

            if (response.RequiredInsight != null && !string.IsNullOrEmpty(response.RequiredInsight.TwinId))
            {
                var customer = await _directoryApi.GetCustomer(site.CustomerId);
                var asset = await _digitalTwinApiService.GetAssetAsync(siteId, response.RequiredInsight.TwinId);
                var createInsightRequest = new CreateInsightCoreRequest
                {
                    CustomerId = site.CustomerId,
                    SequenceNumberPrefix = site.Code,
                    TwinId = response.RequiredInsight.TwinId,
                    Type = response.RequiredInsight.Type,
                    Name = response.RequiredInsight.Name,
                    Description = (response.RequiredInsight.Description ?? string.Empty) + $"\r\nAsset: {asset?.Name}",
                    Priority = response.RequiredInsight.Priority,
                    State = InsightState.Active,
                    OccurredDate = _dateTimeService.UtcNow,
                    DetectedDate = _dateTimeService.UtcNow,
                    SourceType = InsightSourceType.Inspection,
                    SourceId = null,
                    ExternalId = string.Empty,
                    ExternalStatus = string.Empty,
                    ExternalMetadata = string.Empty,
                    OccurrenceCount = 1,
                    AnalyticsProperties = new Dictionary<string, string> 
                    {
                        { "Site", site.Name },
                        { "Company", customer.Name }
                    },
					CreatedUserId = userId
                };
                var createdInsight = await _insightApi.CreateInsight(siteId, createInsightRequest);
                await _workflowApi.UpdateCheckRecordInsight(siteId, inspectionId, checkRecordId, createdInsight.Id);
            }

            return NoContent();
        }

        [HttpPost("sites/{siteId}/syncInspectionRecords")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(InspectionRecordsResponse), StatusCodes.Status200OK)]
        [SwaggerOperation("Sync Inspection Records", Tags = new[] { "Inspections" })]
        public async Task<IActionResult> SyncInspectionRecords([FromRoute] Guid siteId, [FromBody] InspectionRecordsRequest request)
        {
            var userId = this.GetCurrentUserId();
			var userType=this.GetCurrentUserType();
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

            var site = await _siteApi.GetSite(siteId);
            var response = await _workflowApi.SyncInspectionRecords(site, userId, userType, request);

            return Ok(response);
        }

        [HttpGet("inspections")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(InspectionsDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets inspections", Tags = new[] { "Inspections" })]
        public async Task<IActionResult> GetInspections()
        {
            var userId = this.GetCurrentUserId();

            var inspections = await _workflowApi.GetInspections(userId);

            return Ok(inspections);
        }

		[HttpGet("sites/{siteId}/inspections/{inspectionId}/checks/{checkId}/submittedhistory")]
		[MobileAuthorize]
		[ProducesResponseType(typeof(CheckRecordDto), StatusCodes.Status200OK)]
		[SwaggerOperation("Gets the last (count) records of the Check submitted", Tags = new[] { "Inspections" })]
		public async Task<IActionResult> GetCheckSubmittedHistory(
			[FromRoute] Guid siteId,
			[FromRoute] Guid inspectionId,
			[FromRoute] Guid checkId,
			[FromQuery] int? count)
		{
			var userId = this.GetCurrentUserId();
			await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

			var checkRecords = await _workflowApi.GetCheckSubmittedHistory(siteId, inspectionId, checkId, count ?? 10);
			var siteUsers = await _directoryApi.GetSiteUsers(siteId);
			var checkRecordsDtos = CheckRecordDto.Map(checkRecords, _imageUrlHelper);
			foreach (var checkRecord in checkRecordsDtos)
			{
				checkRecord.EnteredBy = siteUsers.Where(x => x.Id == checkRecord.SubmittedUserId).FirstOrDefault();
			}

			return Ok(checkRecordsDtos);
		}

        private async Task<List<InspectionDto>> EnrichInspections(Guid siteId, List<Inspection> inspections)
        {
            var inspectionDtos = InspectionDto.Map(inspections, _imageUrlHelper);
            var twinIds = inspectionDtos.Select(i => i.TwinId).Distinct().ToList();
            var assets = await GetAssets(siteId, twinIds);

            foreach (var dto in inspectionDtos)
            {
                dto.AssetName = assets.FirstOrDefault(a => a.Id == dto.TwinId)?.Name;
            }
            return inspectionDtos.ToList();
        }

        private async Task<List<TwinSimpleResponse>> GetAssets(Guid siteId, IEnumerable<string> twinIds)
        {
            return twinIds.Any()
                ? await _digitalTwinApiService.GetAssetsByTwinIdsAsync(siteId, twinIds)
                : new List<TwinSimpleResponse>();
        }
    }
}
