using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Willow.Common;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Http;
using WorkflowCore.Infrastructure.Configuration;
using WorkflowCore.Models;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using Willow.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Http;
namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TicketsController : TranslationController
    {
        private readonly IImagePathHelper _imagePathHelper;
        private readonly IReportersService _reportersService;
        private readonly IWorkflowService _workflowCoreService;
		private readonly ISessionService _sessionService;
        private readonly ISiteStatisticsService _siteStatisticsService;
        private readonly AppSettings _appSettings;
        private readonly ITicketSubStatusService _ticketSubStatusService;
        private readonly IWorkgroupService _workgroupService;
        public readonly IExternalProfileService _externalProfileService;
        private readonly ITicketStatusTransitionsService _ticketStatusTransitionsService;
        public TicketsController(
            IImagePathHelper imagePathHelper,
            IWorkflowService workflowCoreService,
            IReportersService reportersService,
            IHttpRequestHeaders headers,
            ISessionService sessionService,
            ISiteStatisticsService siteStatisticsService,
            IConfiguration configuration,
            ITicketSubStatusService ticketSubStatusService,
            IWorkgroupService workgroupService,
            IExternalProfileService externalProfileService,
            ITicketStatusTransitionsService ticketStatusTransitionsService) :
            base(headers)
        {
            _imagePathHelper = imagePathHelper;
            _workflowCoreService = workflowCoreService;
            _reportersService = reportersService;
            _sessionService = sessionService;
            _siteStatisticsService = siteStatisticsService;
            _appSettings = configuration.Get<AppSettings>();
            _ticketSubStatusService = ticketSubStatusService;
            _workgroupService = workgroupService;
            _externalProfileService = externalProfileService;
            _ticketStatusTransitionsService = ticketStatusTransitionsService;
        }

        /// <summary>
        /// returns ticket priority and count statistics by twinIds
        /// </summary>
        /// <param name="request">list of twinIds and sourceType filter</param>
        /// <returns>List of ticket priority and count statistics by twinIds</returns>
        [HttpPost("tickets/twins/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<TwinTicketStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightStatisticsByTwinIds([FromBody] TwinStatisticsRequest request)
        {
            return Ok(await _workflowCoreService.GetTicketStatisticsByTwinIds(request));
        }

        /// <summary>
        /// Returns ticket status statistics by twinIds
        /// </summary>
        /// <param name="request">List of twinIds and sourceType filter</param>
        /// <returns>List of ticket status statistics by twinIds</returns>
        [HttpPost("tickets/twins/statistics/status")]
        [Authorize]
        [ProducesResponseType(typeof(List<TwinTicketStatisticsByStatus>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightStatusStatisticsByTwinIds([FromBody] TwinStatisticsRequest request)
        {
            return Ok(await _workflowCoreService.GetTicketStatusStatisticsByTwinIds(request));
        }

        [HttpGet("sites/{siteId}/tickets/categories")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketCategoryDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketCategories([FromRoute] Guid siteId)
        {
            var ticketCategories = await _workflowCoreService.GetTicketCategories(siteId);
            return Ok(TicketCategoryDto.MapFromModels(ticketCategories));
        }

        [HttpGet("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCategoryDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicketCategory([FromRoute] Guid siteId, [FromRoute] Guid ticketCategoryId)
        {
            var ticketCategory = await _workflowCoreService.GetTicketCategory(siteId, ticketCategoryId);
            return Ok(TicketCategoryDto.MapFromModel(ticketCategory));
        }

        [HttpPost("sites/{siteId}/tickets/categories")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCategoryDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateTicketCategory([FromRoute] Guid siteId, [FromBody] CreateTicketCategoryRequest request)
        {
            var ticketCategory = await _workflowCoreService.CreateTicketCategory(siteId, request);
            return Ok(TicketCategoryDto.MapFromModel(ticketCategory));
        }

        [HttpDelete("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteTicketCategory([FromRoute] Guid siteId, [FromRoute] Guid ticketCategoryId)
        {
            await _workflowCoreService.DeleteTicketCategory(siteId, ticketCategoryId);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTicketCategory(
            [FromRoute] Guid siteId,
            [FromRoute] Guid ticketCategoryId,
            [FromBody] UpdateTicketCategoryRequest request)
        {
            await _workflowCoreService.UpdateTicketCategory(siteId, ticketCategoryId, request);
            return NoContent();
        }

        [HttpGet("sites/{siteId}/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteTickets(
            [FromRoute] Guid siteId,
            [FromQuery] int[] statuses,
            [FromQuery] IssueType? issueType,
            [FromQuery] Guid? issueId,
            [FromQuery] Guid? insightId,
            [FromQuery] Guid? assigneeId,
            [FromQuery] bool? unassigned,
            [FromQuery] bool? scheduled,
            [FromQuery] string orderBy,
            [FromQuery] string externalId,
            [FromQuery] string floorId,
            [FromQuery] Guid? sourceId,
            [FromQuery] SourceType? sourceType,
            [FromQuery] DateTime? createdAfter,
            [FromQuery] int page,
            [FromQuery] int pageSize)
        {
            if (issueType.HasValue && !issueId.HasValue)
            {
                throw new ArgumentNullException("issueType is provided, but issueId is missing").WithData(new { SiteId = siteId });
            }

            var request = new GetSiteTicketsRequest()
            {
                Statuses = statuses,
                IssueType = issueType,
                IssueId = issueId,
                InsightId = insightId,
                AssigneeId = assigneeId,
                Unassigned = unassigned,
                IsScheduled = scheduled ?? false,
                OrderBy = orderBy,
                ExternalId = externalId,
                FloorId = floorId,
                SourceId = sourceId,
                SourceType = sourceType,
                CreatedAfter = createdAfter,
                Page = page,
                PageSize = pageSize
            };

            var tickets = await _workflowCoreService.GetSiteTickets(siteId, request);
            var dtos = TicketSimpleDto.MapFromModels(tickets, _appSettings.MappedIntegrationConfiguration?.SourceName);
            return Ok(dtos);
        }

        [HttpGet("sites/{siteId}/tickets/count")]
        [Authorize]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTotalTicketsCount(
            [FromRoute] Guid siteId,
            [FromQuery] int[] statuses,
            [FromQuery] bool? scheduled)
        {
            var isScheduled = scheduled.HasValue ? scheduled.Value : false;
            var totalCount = await _workflowCoreService.GetTicketsCount(siteId, statuses, isScheduled);
            return Ok(totalCount);
        }
        [Obsolete("Use the endpoint without SiteId, tickets/{id}")]
        [HttpGet("sites/{siteId}/tickets/{ticketId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicket([FromRoute] Guid siteId, [FromRoute] Guid ticketId, bool includeAttachments = false, bool includeComments = false)
        {
            var ticket = await _workflowCoreService.GetTicket(ticketId, includeAttachments, includeComments);
            if (ticket == null)
            {
                throw new NotFoundException(new { TicketId = ticketId });
            }
            var dto = TicketDetailDto.MapFromModel(ticket, _imagePathHelper, _appSettings.MappedIntegrationConfiguration?.SourceName);
            return Ok(dto);
        }

        [HttpGet("tickets/{ticketId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicket([FromRoute] Guid ticketId, bool includeAttachments = false, bool includeComments = false)
        {
            var ticket = await _workflowCoreService.GetTicket(ticketId, includeAttachments, includeComments);
            if (ticket == null)
            {
                throw new NotFoundException(new { TicketId = ticketId });
            }
            var dto = TicketDetailDto.MapFromModel(ticket, _imagePathHelper, _appSettings.MappedIntegrationConfiguration?.SourceName);
            return Ok(dto);
        }


        [HttpPost("sites/{siteId}/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateTicket([FromRoute] Guid siteId, [FromBody] CreateTicketRequest request)
        {
			var sourceId = request.SourceId is null ? request.CreatorId : request.SourceId;
			_sessionService.SetSessionData(request.SourceType, sourceId);
			if (string.IsNullOrEmpty(request.SequenceNumberPrefix))
            {
                throw new ArgumentNullException($"{nameof(CreateTicketRequest.SequenceNumberPrefix)} should be provided").WithData(new { SiteId = siteId });
            }

            if (!request.ReporterId.HasValue)
            {
                var createReportRequest = new CreateReporterRequest
                {
                    CustomerId = request.CustomerId,
                    Name = request.ReporterName,
                    Phone = request.ReporterPhone,
                    Email = request.ReporterEmail,
                    Company = request.ReporterCompany
                };
                var reporter = await _reportersService.CreateReporter(siteId, createReportRequest);
                request.ReporterId = reporter.Id;
            }
            // Mapped new ticket status set to New instead of Open
            // this code will be refactored to use dynamic status configuration
            // as part of this spike https://dev.azure.com/willowdev/Unified/_workitems/edit/127468
            var status = (int)TicketStatusEnum.Open;
            if(_appSettings.MappedIntegrationConfiguration?.IsEnabled ?? false)
            {
                status = (int)TicketStatusEnum.New;
            }
            var createdTicket = await _workflowCoreService.CreateTicket(siteId, request, (int)status, Language);
            var createdTicketDto = TicketDetailDto.MapFromModel(createdTicket, _imagePathHelper);
            return Ok(createdTicketDto);
        }

        [HttpPost("sites/{siteId}/tickets/batch")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketDetailDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateTickets([FromRoute] Guid siteId, [FromBody] CreateTicketsRequest request)
        {			
            if (string.IsNullOrEmpty(request.SequenceNumberPrefix))
            {
                throw new ArgumentNullException($"{nameof(CreateTicketRequest.SequenceNumberPrefix)} should be provided").WithData(new { SiteId = siteId });
            }

            var result = await _workflowCoreService.CreateTickets(siteId, request,   Language);

            return Ok(TicketDetailDto.MapFromModels(result, _imagePathHelper) );
        }

        [HttpPut("sites/{siteId}/tickets/{ticketId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTicket([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromBody] UpdateTicketRequest request)
        {
			_sessionService.SetSessionData(request.SourceType, request.SourceId);
			var ticketExists = await _workflowCoreService.GetTicketExistence(siteId, ticketId, false);
            if (!ticketExists)
            {
                throw new NotFoundException(new { TicketId = ticketId });
            }

            if (!request.ReporterId.HasValue && !string.IsNullOrEmpty(request.ReporterName))
            {
                var createReportRequest = new CreateReporterRequest
                {
                    CustomerId = request.CustomerId,
                    Name = request.ReporterName,
                    Phone = request.ReporterPhone,
                    Email = request.ReporterEmail,
                    Company = request.ReporterCompany
                };
                var reporter = await _reportersService.CreateReporter(siteId, createReportRequest);
                request.ReporterId = reporter.Id;
            }

            await _workflowCoreService.UpdateTicket(siteId, ticketId, request, Language);

            var ticket = await _workflowCoreService.GetTicket(ticketId, includeAttachments: true, includeComments: true);
            var dto = TicketDetailDto.MapFromModel(ticket, _imagePathHelper);
            return Ok(dto);
        }

        [HttpPost("customers/{customerId}/ticketstatus")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketStatusDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateOrUpdateCustomerTicketStatus([FromRoute] Guid customerId, [FromBody] CreateTicketStatusRequest request)
        {
            if (!(request.TicketStatuses?.Any() ?? false))
            {
                return BadRequest("Request body not provided.");
            }

            var ticketStatus = await _workflowCoreService.CreateOrUpdateTicketStatus(customerId, request);

            return Ok(TicketStatusDto.MapFromModels(ticketStatus));
        }


        [HttpGet("customers/{customerId}/ticketstatus")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketStatusDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCustomerTicketStatus([FromRoute] Guid customerId)
        {
            var ticketStatus = await _workflowCoreService.GetTicketStatus(customerId);

            return Ok(TicketStatusDto.MapFromModels(ticketStatus));
        }


        /// <summary>
        /// returns ticket priority and status statistics by siteIds
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns>List of status and priority ticket statistics</returns>
        [HttpPost("tickets/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(TicketStatisticsDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketStatisticsBySiteIds([FromBody] List<Guid> siteIds)
        {
            if (siteIds == null || !siteIds.Any())
            {
               return BadRequest("The siteIds are required");
            }
            return Ok(await _siteStatisticsService.GetTicketStatisticsBySiteIdsAsync(siteIds));
        }

        [HttpGet("ticketsSubStatus")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSubStatus>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketSubStatus()
        {
            var subStatus =  await _ticketSubStatusService.GetTicketSubStatusAsync();
            return Ok(subStatus);
        }


        /// <summary>
        /// Retrieves the possible assignees for a ticket based on the site ID.
        /// External profiles included only if mapped enabled with full sync
        /// </summary>
        /// <param name="siteId">The ID of the site.</param>
        /// <returns>the possible assignees for the ticket.</returns>
        [HttpGet("sites/{siteId}/possibleTicketAssignees")]
        [Authorize]
        [ProducesResponseType(typeof(TicketAssigneeData), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketPossibleAssignee([FromRoute] Guid siteId)
        {
            var possibleAssignees = new TicketAssigneeData();
            var workgroups = await _workgroupService.GetWorkgroups(siteId, true);
            if ((_appSettings.MappedIntegrationConfiguration?.IsEnabled ?? false)
                && !(_appSettings.MappedIntegrationConfiguration?.IsReadOnly ?? true))
            {
                var externalProfiles = await _externalProfileService.GetAssigneeExternalProfiles();
                possibleAssignees.ExternalUserProfiles = externalProfiles;
            }
            possibleAssignees.Workgroups = WorkgroupDto.MapFromModels(workgroups);
            return Ok(possibleAssignees);
        }
        /// <summary>
        /// Get the ticket category counts in order descending for a given space twin Id based the limit number of categories
        /// and sum of the rest of the categories as other
        /// </summary>
        /// <param name="spaceTwinId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("tickets/twins/{spaceTwinId}/ticketCountsByCategory")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCategoryCountDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketCategoryCountBySpaceTwinId([FromRoute] string spaceTwinId, [FromQuery] int limit)
        {
            var ticketCategoryCount = await _workflowCoreService.GetTicketCategoryCountBySpaceTwinId(spaceTwinId, limit);
            return Ok(ticketCategoryCount);
        }


        /// <summary>
        /// Retrieves the count of created tickets of each day within a specified date range. for a given space twin ID.
        /// </summary>
        /// <param name="spaceTwinId">The ID of the space twin.</param>
        /// <param name="startDate">The start date of the range.</param>
        /// <param name="endDate">The end date of the range.</param>
        /// <returns>The count of created tickets of each day within a specified date range.</returns>
        [HttpGet("tickets/twins/{spaceTwinId}/ticketCountsByDate")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCountsByDateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTicketsCountsByCreatedDate([FromRoute] string spaceTwinId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            if (startDate is null || endDate is null)
            {
                return BadRequest("The start date and end date are required.");
            }
            if(startDate >= endDate)
            {
                return BadRequest("The end date must be greater than start date");
            }
            var ticketCounts = await _workflowCoreService.GetTicketsCountsByCreatedDate(spaceTwinId, startDate.Value, endDate.Value);
            return Ok(ticketCounts);
        }

        /// <summary>
        /// Returns the ticket status transitions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/statusTransitions")]
        [Authorize]
        [ProducesResponseType(typeof(TicketStatusTransitionsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketStatusTransitions()
        {
            var result = await _ticketStatusTransitionsService.GetTicketStatusTransitionsAsync();
            return Ok(result);
        }

    }
}
