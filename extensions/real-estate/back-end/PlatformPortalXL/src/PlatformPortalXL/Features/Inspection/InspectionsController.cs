using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Sites;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Inspection
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InspectionsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly IInspectionService _inspectionService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IDateTimeService _dateTimeService;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly ISiteService _siteService;
        private const string NameMandatoryErrorMessage =  "Name is required";

        public InspectionsController(
            IAccessControlService accessControl,
            IWorkflowApiService workflowApi,
            ISiteApiService siteApi,
            IInspectionService inspectionService,
            IDirectoryApiService directoryApi,
            IDateTimeService dateTimeService,
            IImageUrlHelper imageUrlHelper,
            ISiteService siteService)
        {
            _accessControl = accessControl;
            _workflowApi = workflowApi;
            _siteApi = siteApi;
            _inspectionService = inspectionService;
            _directoryApi = directoryApi;
            _dateTimeService = dateTimeService;
            _imageUrlHelper = imageUrlHelper;
            _siteService = siteService;
        }

        #region Zones

        [HttpPost("sites/{siteId}/inspectionZones")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionZoneDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Create a Inspection Zone", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> CreateInspectionZone([FromRoute] Guid siteId, [FromBody] CreateInspectionZoneRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var error = new ValidationError();

            var existingZones = await _workflowApi.GetInspectionZones(siteId, false);

            if (existingZones.Any(x => x.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Duplicate zone name"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            var zone = await _workflowApi.CreateInspectionZone(siteId, request);
            return Ok(InspectionZoneDto.MapFromModel(zone));
        }

        [HttpGet("sites/{siteId}/inspectionZones")]
        [Authorize]
        [ProducesResponseType(typeof(List<InspectionZoneDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get list of Inspection Zones", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetInspectionZones([FromRoute] Guid siteId, [FromQuery] string scopeId = "")
        {
            var siteIds = await GetAuthorizedSiteIds(scopeId, siteId);
            var zoneTasks = siteIds.Select(c => _workflowApi.GetInspectionZones(c, true));
            var zones = (await Task.WhenAll(zoneTasks))?.ToList().SelectMany(c => c);
            return Ok(InspectionZoneDto.MapFromModels(zones));
        }

        [HttpPut("sites/{siteId}/inspectionZones/{inspectionZoneId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Update Inspection Zone by Id", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> UpdateInspectionZone([FromRoute] Guid siteId, [FromRoute] Guid inspectionZoneId, [FromBody] UpdateInspectionZoneRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var error = new ValidationError();

            var existingZones = await _workflowApi.GetInspectionZones(siteId, false);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), NameMandatoryErrorMessage));
            }
            else if (request.Name.Length > 200)
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Name exceeds the max length"));
            }
            else if (existingZones.Any(x => x.Name.Equals(request.Name, StringComparison.InvariantCultureIgnoreCase) && x.Id != inspectionZoneId))
            {
                error.Items.Add(new ValidationErrorItem(nameof(request.Name), "Duplicate zone name"));
            }

            if (error.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, error);
            }

            await _workflowApi.UpdateInspectionZone(siteId, inspectionZoneId, request);
            return NoContent();
        }

        [HttpGet("sites/{siteId}/inspectionZones/{inspectionZoneId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionZoneDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get list of Inspections in a zone", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetInspections([FromRoute] Guid siteId, [FromRoute] Guid inspectionZoneId, [FromQuery] string scopeId = "")
        {
            var siteIds = await GetAuthorizedSiteIds(scopeId, siteId);
            var zones = await _workflowApi.GetInspectionZones(siteIds);
            var zone = zones.FirstOrDefault(z => z.Id == inspectionZoneId);
            if (zone == null)
            {
                throw new NotFoundException().WithData(new { inspectionZoneId });
            }

            var zoneInspections = await _workflowApi.GetInspectionsByZone(zone.SiteId, zone.Id);
            var zoneInspectionDtos = await _inspectionService.EnrichInspections(zone.SiteId, zoneInspections);

            var zoneDto = InspectionZoneDto.MapFromModel(zone);
            zoneDto.Inspections = zoneInspectionDtos;
            return Ok(zoneDto);
        }

        #endregion

        [HttpPost("sites/{siteId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Create an Inspection", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> CreateInspection(
            [FromRoute] Guid siteId,
            [FromBody] CreateInspectionRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            if (!ValidateInspectionRequests(request, out ValidationError inspectionValidationError))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, inspectionValidationError);
            }

            if (!ValidateInspectionCheckRequests(request.Checks, out ValidationError validationError))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var inspection = await _workflowApi.CreateInspection(siteId, request);
            return Ok(InspectionDto.MapFromModel(inspection));
        }

        [HttpGet("sites/{siteId}/inspections")]
        [Authorize]
        [ProducesResponseType(typeof(List<InspectionDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Inspections of a site", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetSiteInspections([FromRoute] Guid siteId, [FromQuery] string scopeId="")
        {
            var siteIds = await GetAuthorizedSiteIds(scopeId, siteId);
            var inspectionTasks = siteIds.Select(GetSiteInspectionDtos);
            var inspectionDtos =( await Task.WhenAll(inspectionTasks)).SelectMany(c=>c);
            return Ok(inspectionDtos);
        }

        [HttpPut("sites/{siteId}/inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Update an Inspection", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> UpdateInspection(
            [FromRoute] Guid siteId,
            [FromRoute] Guid inspectionId,
            [FromBody] UpdateInspectionRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            var inspection = await _workflowApi.GetInspection(siteId, inspectionId);

            if (!ValidateInspectionRequests(request, out ValidationError inspectionValidationError))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, inspectionValidationError);
            }

            if (!ValidateInspectionCheckRequests(request.Checks, out ValidationError validationError))
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            PauseDependentChecks(request.Checks, inspection.Checks);
            inspection = await _workflowApi.UpdateInspection(siteId, inspectionId, request);
            return Ok(InspectionDto.MapFromModel(inspection));
        }

        [Obsolete("Use the endpoint without SiteId, inspections/{id}")]
        [HttpGet("sites/{siteId}/inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get an Inspection", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetInspection([FromRoute] Guid siteId, [FromRoute] Guid inspectionId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var inspection = await _workflowApi.GetInspection(siteId, inspectionId);

            if (inspection == null)
            {
                throw new NotFoundException().WithData(new { inspectionId });
            }

            var inspectionDto = await _inspectionService.EnrichInspection(siteId, inspection);
            return Ok(inspectionDto);
        }

        [HttpGet("inspections/{inspectionId}")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get an Inspection", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetInspection([FromRoute] Guid inspectionId, [FromQuery] string scopeId = "")
        {
            var userId = this.GetCurrentUserId();
            var siteIds = await GetAuthorizedSiteIds(scopeId);
           
            var inspection = await _workflowApi.GetInspection(inspectionId) ?? throw new NotFoundException().WithData(new { inspectionId });

            if(!siteIds.Contains(inspection.SiteId))
                throw new UnauthorizedAccessException().WithData(new { userId, inspection.SiteId });

            var inspectionDto = await _inspectionService.EnrichInspection(inspection.SiteId, inspection);
            return Ok(inspectionDto);
        }

        [HttpGet("inspections")]
        [Authorize]
        [ProducesResponseType(typeof(List<InspectionDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Inspections for all sites", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetAllSitesInspections([FromQuery] string scopeId = "")
        {
            var siteIds = await GetAuthorizedSiteIds(scopeId);
            var inspectionTasks = siteIds.Select(GetSiteInspectionDtos);
            var result = (await Task.WhenAll(inspectionTasks)).SelectMany(c => c);

            return Ok(result);
        }


        [HttpGet("sites/{siteId}/inspectionUsage")]
        [Authorize]
        [ProducesResponseType(typeof(InspectionUsageDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get an Inspection Usage by Zone", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetInspectionUsage([FromRoute] Guid siteId, [FromQuery] InspectionUsagePeriod period)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var inspectionUsage = await _inspectionService.GetInspectionUsageBySiteId(siteId, period);
            return Ok(inspectionUsage);
        }

        [HttpGet("inspections/{inspectionId}/checks/history")]
        [Authorize]
        [ProducesResponseType(typeof(CheckRecordReportDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Checks History", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> GetChecksHistory(
            [FromRoute] Guid inspectionId,
            [FromQuery] Guid siteId,
            [FromQuery] Guid? checkId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var now = _dateTimeService.UtcNow;
            var start = (startDate ?? now.AddDays(-7)).ToUniversalTime();
            var end = (endDate ?? now).ToUniversalTime();

            if (startDate > endDate)
            {
                throw new ArgumentException("The Start Date cannot be greater than the End Date");
            }
            var site = await _directoryApi.GetSite(siteId);
            var checkRecordsTask = _workflowApi.GetCheckHistory(siteId, inspectionId, site.CustomerId, checkId, start, end);
            var siteUsersTask = _directoryApi.GetSiteUsers(siteId);
            var siteUserDict = (await siteUsersTask).ToDictionary(k => k.Id, v => $"{v.FirstName} {v.LastName}");
            var checkRecords = (await checkRecordsTask).OrderByDescending(x => x.SubmittedDate).ToList();
            var checkRecordsDtos = CheckRecordReportDto.MapFromModels(checkRecords, _imageUrlHelper, siteUserDict);
           
            return Ok(checkRecordsDtos);
        }
        [HttpGet("inspections/{inspectionId}/checks/history/export")]
        [Authorize]
        [SwaggerOperation("Export Inspection Check History as csv file", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> ExportCheckHistory(
            [FromRoute] Guid inspectionId,
            [FromQuery] Guid siteId,
            [FromQuery] Guid? checkId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int timezoneOffset)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var now = _dateTimeService.UtcNow;
            var start = (startDate ?? now.AddDays(-7)).ToUniversalTime();
            var end = (endDate ?? now).ToUniversalTime();
            if (startDate > endDate)
            {
                throw new ArgumentException("The Start Date cannot be greater than the End Date");
            }
            var site = await _directoryApi.GetSite(siteId);
            var checkRecordsTask = _workflowApi.GetCheckHistory(siteId, inspectionId, site.CustomerId, checkId, start, end);
            var siteUsersTask = _directoryApi.GetSiteUsers(siteId);
            var siteUserDict = (await siteUsersTask).ToDictionary(k => k.Id, v => $"{v.FirstName} {v.LastName}");
            var checkRecords = (await checkRecordsTask).OrderByDescending(x => x.SubmittedDate).ToList();
            var reportName = GetReportName(checkRecords?.FirstOrDefault(), checkId);
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            {
                await writer.WriteLineAsync("Id,Date & Time,Floor,Inspection name,Asset name,Zone,Entered by,Entry,Unit,Note");
                foreach (var checkRecord in checkRecords)
                {
                    var enteredBy = siteUserDict.TryGetValue(checkRecord.SubmittedUserId ?? Guid.Empty, out var value) ? value : "Unknown";
                    await writer.WriteLineAsync(BuildCsvRow(checkRecord, enteredBy, timezoneOffset));
                }
                await writer.FlushAsync();
                memoryStream.Seek(0, SeekOrigin.Begin);
                Response.Headers.Append("x-file-name", $"{reportName}.csv");
                Response.Headers.Append("Access-Control-Expose-Headers", "x-file-name");
                return File(memoryStream.ToArray(), "application/octet-stream", $"{reportName}.csv");
            }
        }

      

        [HttpPost("sites/{siteId}/zones/{zoneId}/archive")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Mark zone as archived", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> ArchiveZone([FromRoute] Guid siteId, [FromRoute] Guid zoneId, [FromQuery] bool? isArchived)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var archived = isArchived.HasValue ? isArchived.Value : true;
            await _inspectionService.ArchiveZone(siteId, zoneId, archived);
            return NoContent();
        }

        [HttpPost("sites/{siteId}/inspections/{inspectionId}/archive")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Mark inspection as archived", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> ArchiveInspection([FromRoute] Guid siteId, [FromRoute] Guid inspectionId, [FromQuery] bool? isArchived)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var archived = isArchived.HasValue ? isArchived.Value : true;
            await _inspectionService.ArchiveInspection(siteId, inspectionId, archived);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/zones/{zoneId}/inspections/sortOrder")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Update sort order of zone inspections", Tags = new[] { "Inspection" })]
        public async Task<IActionResult> UpdateInspectionSortOrder([FromRoute] Guid siteId, [FromRoute] Guid zoneId, [FromBody] UpdateInspectionSortOrderRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            await _inspectionService.UpdateSortOrder(siteId, zoneId, request);
            return NoContent();
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
		[ProducesResponseType(typeof(InspectionDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
		[SwaggerOperation("Create Inspection for each asset in the AssetList", Tags = new[] { "Inspection" })]
		public async Task<ActionResult<List<InspectionDto>>> CreateInspections(
			[FromRoute] Guid siteId,
			[FromBody] CreateInspectionsRequest request)
		{
			await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

			var site = await _siteApi.GetSite(siteId);
			if (site == null)
			{
				throw new NotFoundException().WithData(new { siteId });
			}

			if (!ValidateInspectionRequests(request, out ValidationError inspectionValidationError))
			{
				return StatusCode(StatusCodes.Status422UnprocessableEntity, inspectionValidationError);
			}

			if (!ValidateInspectionCheckRequests(request.Checks, out ValidationError validationError))
			{
				return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
			}

			var inspections = await _workflowApi.CreateInspections(siteId, request);
			return Ok(InspectionDto.MapFromModels(inspections));
		}

        #region Private
        private static string GetReportName(CheckRecordReport checkRecord, Guid? checkId)
        {
            var reportName = "checkRecordHistory";
            return checkRecord==null? reportName:( checkId.HasValue ? $"{reportName}_{checkRecord.TwinName.Replace(' ','-')}_{checkRecord.CheckName.Replace(' ', '-')}" :
                $"{reportName}_{checkRecord.TwinName.Replace(' ', '-')}");
        }
        private async Task<List<InspectionDto>> GetSiteInspectionDtos(Guid siteId)
        {
            var inspections = await _workflowApi.GetSiteInspections(siteId);

            foreach (var inspection in inspections)
            {
                inspection.Checks = inspection.Checks.Where(c => c.Statistics.WorkableCheckStatus != CheckRecordStatus.NotRequired).ToList();
                foreach (var check in inspection.Checks.Where(C => C.Statistics.WorkableCheckStatus != CheckRecordStatus.Completed))
                {
                    check.Statistics.LastCheckSubmittedEntry = string.Empty;
                    check.Statistics.LastCheckSubmittedUserId = default;
                }
            }

            var inspectionDtos = await _inspectionService.EnrichInspections(siteId, inspections);
            inspectionDtos.RemoveAll(i => i.Checks.All(c => c.IsPaused));
            return inspectionDtos;
        }

        private static bool ValidateInspectionCheckRequests<T>(IList<T> checkRequests, out ValidationError validationError) where T : CheckRequest
        {
            validationError = new ValidationError();

            var listTypeCheckValueDict = checkRequests.Where( r=> r.Type == CheckType.List &&  !string.IsNullOrWhiteSpace(r.Name) && !string.IsNullOrWhiteSpace(r.TypeValue))
                                                             .ToDictionary(i=> i.Name, i=> i.TypeValue);

            foreach(var checkRequest in checkRequests)
            {
                if (!string.IsNullOrWhiteSpace(checkRequest.DependencyName))
                {
                    if (listTypeCheckValueDict.TryGetValue(checkRequest.DependencyName, out string dependencyValue))
                    {
                        if (!dependencyValue.Contains(checkRequest.DependencyValue, StringComparison.InvariantCulture))
                        {
                            validationError.Items.Add(new ValidationErrorItem(nameof(CreateInspectionRequest.Checks), "DependencyValue is invalid"));
                        }
                    }
                    else
                    {
                        validationError.Items.Add(new ValidationErrorItem(nameof(CreateInspectionRequest.Checks), "DependencyName is not specified in preceding list"));
                    }
                }
                if(checkRequest.Multiplier<=0)
                    validationError.Items.Add(new ValidationErrorItem(nameof(CreateInspectionRequest.Checks), "Multiplier is invalid"));

            }

            return !validationError.Items.Any();
        }
    
        // Set same pause start and end dates for all dependency checks as the dependent check
        private void PauseDependentChecks(List<UpdateCheckRequest> checks, List<Check> inspectionChecks)
        {
            var rootChecks = checks.Where(x => x.Id != null && x.Type == CheckType.List && x.DependencyId == null);
            foreach (var root in rootChecks)
            {
                var originalRoot = inspectionChecks.Find(x => x.Id == root.Id);
                var isRootModified = !(root.PauseStartDate == originalRoot?.PauseStartDate && root.PauseEndDate == originalRoot?.PauseEndDate);
                var children = GetChildren(checks, root.Id ?? Guid.Empty);
                children.ForEach(child =>
                {
                    if (isRootModified == true) // if root is modified apply all pause date to child items
                    {
                        child.PauseStartDate = root.PauseStartDate;
                        child.PauseEndDate = root.PauseEndDate;
                    }
                });
            }
        }

        private List<UpdateCheckRequest> GetChildren(List<UpdateCheckRequest> checks, Guid? id)
        {
            return checks.Where(x => x.DependencyId== id)
                         .Union(checks.Where(x => x.DependencyId == id)
                         .SelectMany(y => GetChildren(checks, y.Id)))
                         .ToList();
        }

        private string BuildCsvRow(CheckRecordReport checkRecord, string enteredBy, int timezoneOffset)
        {
            var notes = checkRecord.Notes?.Replace(@",", @" ", StringComparison.InvariantCulture);
            var csvRow = $"{checkRecord.Id},";
            csvRow += $"{checkRecord.SubmittedDate?.AddMinutes(timezoneOffset * -1)},";
            csvRow += $"{checkRecord.FloorCode},";
            csvRow += $"{checkRecord.InspectionName},";
            csvRow += $"{checkRecord.TwinName},";
            csvRow += $"{checkRecord.ZoneName},";
            csvRow += $"{enteredBy},";

            if (checkRecord.CheckType == CheckType.Numeric || checkRecord.CheckType == CheckType.Total)
            {
                csvRow += $"{checkRecord.NumberValue},";
            }
            else if (checkRecord.CheckType == CheckType.Date)
            {
                var dateValue = checkRecord.DateValue?.AddMinutes(timezoneOffset * -1);
                csvRow +=$"{dateValue?.ToString("M/d/yyyy")},";
            }
            else
            {
                csvRow += $"{checkRecord.StringValue},";
            }
            csvRow += $"{checkRecord.TypeValue},";
            csvRow += $"{notes}";

            return csvRow;
        }
        private static bool ValidateInspectionRequests<T>(T inspectionRequest, out ValidationError validationError) where T : InspectionRequest
        {
            validationError = new ValidationError();

            if ((inspectionRequest.FrequencyUnit == SchedulingUnit.Hours && !Enumerable.Range(1, 24).Contains(inspectionRequest.Frequency ?? 0))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Days && !Enumerable.Range(1, 7).Contains(inspectionRequest.Frequency ?? 0))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Weeks && !Enumerable.Range(1, 52).Contains(inspectionRequest.Frequency ?? 0))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Months && !Enumerable.Range(1, 12).Contains(inspectionRequest.Frequency ?? 0))
            || (inspectionRequest.FrequencyUnit == SchedulingUnit.Years && !Enumerable.Range(1, 10).Contains(inspectionRequest.Frequency ?? 0)))
            {
                validationError.Items.Add(new ValidationErrorItem(nameof(InspectionRequest), "Frequency is invalid"));
            }

            return !validationError.Items.Any();
        }

        private async Task<List<Guid>> GetAuthorizedSiteIds(string scopeId, Guid? siteId = null)
        {
            return await _siteService.GetAuthorizedSiteIds(
                this.GetCurrentUserId(),
                scopeId,
                siteId.HasValue ? new List<Guid>() { siteId.Value } : null,
                x => x.Features.IsInspectionEnabled);
        }

        #endregion
    }
}
