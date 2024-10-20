using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.Assets;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Platform.Users;
using Willow.Common;
using Willow.Workflow;
using Willow.Workflow.Models;
using Willow.ExceptionHandling.Exceptions;
using Microsoft.Extensions.Caching.Memory;
namespace PlatformPortalXL.Features.Workflow
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IAccessControlService _accessControl;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly IInsightApiService _insightApi;
        private readonly IFloorsApiService _floorApi;
        private readonly IDigitalTwinAssetService _digitalTwinService;
        private readonly ISiteService _siteService;
        private readonly IMemoryCache _memoryCache;
        public TicketsController(
            ITicketService ticketService,
            IAccessControlService accessControl,
            IImageUrlHelper imageUrlHelper,
            IDirectoryApiService directoryApi,
            IWorkflowApiService workflowApi,
            IInsightApiService insightApi,
            ISiteApiService siteApi,
            IFloorsApiService floorApi,
            IDigitalTwinAssetService digitalTwinService,
            ISiteService siteService,
            IMemoryCache memoryCache)
        {
            _ticketService = ticketService;
            _accessControl = accessControl;
            _imageUrlHelper = imageUrlHelper;
            _directoryApi = directoryApi;
            _workflowApi = workflowApi;
            _insightApi = insightApi;
            _siteApi = siteApi;
            _floorApi = floorApi;
            _digitalTwinService = digitalTwinService;
            _siteService = siteService;
            _memoryCache = memoryCache;
        }

        [HttpGet("sites/{siteId}/possibleTicketIssues")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketIssueDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of possible ticket issues", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetPossibleTicketIssues(
            [FromRoute] Guid siteId,
            [FromQuery] string floorCode,
            [FromQuery] Guid? floorId,
            [FromQuery, BindRequired] string keyword,
            [FromQuery] string scopeId="")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            if (!floorId.HasValue)
            {
                var floors = await _floorApi.GetFloorsAsync(siteId, null);
                var floor = floors.FirstOrDefault(f => f.Code == floorCode);
                if (floor == null)
                {
                    throw new NotFoundException().WithData(new { floorCode });
                }
                floorId = floor.Id;
            }

            var issueDtos = await _digitalTwinService.GetPossibleTicketIssuesAsync(siteId, floorId, keyword);
            return Ok(issueDtos);
        }

        [HttpGet("sites/{siteId}/possibleTicketAssignees")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketAssignee>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of possible ticket assignees", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetPossibleTicketAssignees([FromRoute] Guid siteId, [FromQuery] string scopeId = "")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            var ticketAssigneesData = await _workflowApi.GetTicketPossibleAssignees(siteId);

            var customerUsers = await _directoryApi.GetSiteUsers(siteId);

            var assignees = customerUsers
                .Where(x => x.Status == UserStatus.Active)
                .Select(TicketAssignee.FromCustomerUser)
                .Union(ticketAssigneesData.Workgroups.Select(TicketAssignee.FromWorkgroup))
                .Union(ticketAssigneesData.ExternalUserProfiles.Select(TicketAssignee.FromExternalUserProfile))
                .ToList();

            return Ok(assignees);
        }

        [HttpGet("sites/{siteId}/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of tickets", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetTicketsForSiteAsync(
            [FromRoute] Guid siteId,
            [FromQuery, BindRequired] TicketListTab tab,
            [FromQuery] bool scheduled,
            [FromQuery] string orderBy,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string scopeId = "")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            try
            {
                var ticketsTotalCount = await _ticketService.GetTotalTicketsCount(siteId, tab, scheduled);
                var ticketDtos = await _ticketService.GetTicketList(siteId, tab, scheduled, orderBy,page, pageSize);
                foreach (var ticketDto in ticketDtos)
                    ticketDto.GroupTotal = ticketsTotalCount;

                return Ok(ticketDtos);
            }
            catch(TicketService.InvalidTabException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        [HttpGet("tickets")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of tickets for all sites", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetTicketsAsync([FromQuery, BindRequired] TicketListTab tab, [FromQuery] bool scheduled, [FromQuery] string scopeId="")
        {
            var userId = this.GetCurrentUserId();
            var userSiteIds = await _siteService.GetAuthorizedSiteIds(userId, scopeId, predicate: c => !c.Features.IsTicketingDisabled);
            var user = await _directoryApi.GetUser(userId);
            var ticketStatuses = await _workflowApi.GetCustomerTicketStatus(user.CustomerId);

            int[] statuses;
            if (ticketStatuses?.Any() ?? false)
            {
                statuses = ticketStatuses.Where(x => x.Tab.Equals(tab.ToString(), StringComparison.InvariantCultureIgnoreCase)).Select(x => x.StatusCode).ToArray();
                if (!statuses.Any())
                {
                    throw new ArgumentException("").WithData(new { tab });
                }
            }
            else
            {
                statuses = tab switch
                {
                    TicketListTab.Open => new[] { (int)TicketStatus.Open, (int)TicketStatus.Reassign, (int)TicketStatus.InProgress, (int)TicketStatus.LimitedAvailability },
                    TicketListTab.Resolved => new[] { (int)TicketStatus.Resolved },
                    TicketListTab.Closed => new[] { (int)TicketStatus.Closed },
                    _ => throw new ArgumentException("").WithData(new { tab })
                };
            }

            List<Ticket> tickets = new List<Ticket>();
            foreach (var siteId in userSiteIds)
            {
                var siteTickets = await _workflowApi.GetTickets(siteId, statuses, null, null, null, scheduled);
	                if (siteTickets != null && siteTickets.Any())
	                {
		                tickets.AddRange(siteTickets);
	                }
            }
            return Ok(tickets.Any()? TicketSimpleDto.MapFromModels(tickets) : new List<TicketSimpleDto>());
        }

        [Obsolete("Use the endpoint without SiteId, tickets/{id}")]
        [HttpGet("sites/{siteId}/tickets/{ticketId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a specific ticket", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetTicket([FromRoute] Guid siteId, [FromRoute] Guid ticketId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var ticket = await _workflowApi.GetTicket(siteId, ticketId, true);
            await _ticketService.EnrichTicket(ticket, siteId);

            var ticketDto = TicketDetailDto.MapFromModel(ticket, _imageUrlHelper);

            return Ok(ticketDto);
        }

        [HttpGet("tickets/{ticketId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a specific ticket", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetTicket([FromRoute] Guid ticketId, [FromQuery] string scopeId = "")
        {
            var userId = this.GetCurrentUserId();
            var siteIds = await _siteService.GetAuthorizedSiteIds(userId, scopeId, predicate: c => !c.Features.IsTicketingDisabled);

            var ticket = await _workflowApi.GetTicket(ticketId, true);

            if (!siteIds.Contains(ticket.SiteId))
                throw new UnauthorizedAccessException().WithData(new { userId, ticket.SiteId, scopeId });

            await _ticketService.EnrichTicket(ticket, ticket.SiteId);

            var ticketDto = TicketDetailDto.MapFromModel(ticket, _imageUrlHelper);

            return Ok(ticketDto);
        }

        [HttpGet("sites/{siteId}/assets/{assetId}/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of tickets associated with the given asset", Tags = new[] { "Workflow" })]
       public async Task<ActionResult> GetAssetTickets([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromQuery, BindRequired] bool isClosed, [FromQuery] bool scheduled, [FromQuery] string scopeId="")
       {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            var site = await _siteApi.GetSite(siteId);
            var ticketStatuses = await _workflowApi.GetCustomerTicketStatus(site.CustomerId);

            var closedStatuses = ticketStatuses?.Where(x => x.Tab.Equals(Enum.GetName(TicketStatus.Closed), StringComparison.InvariantCultureIgnoreCase)).Select(x => x.StatusCode).ToArray();
            var nonClosedStatuses = ticketStatuses?.Where(x => !x.Tab.Equals(Enum.GetName(TicketStatus.Closed), StringComparison.InvariantCultureIgnoreCase)).Select(x => x.StatusCode).ToArray();

			int[] statuses;
			if (isClosed)
			{
				statuses = closedStatuses?.Any() ?? false ? closedStatuses : new[] { (int)TicketStatus.Closed };
			}
			else
			{
				statuses = nonClosedStatuses?.Any() ?? false ? nonClosedStatuses 
                                                    : new[] { (int)TicketStatus.Open, (int)TicketStatus.Reassign, (int)TicketStatus.InProgress, (int)TicketStatus.LimitedAvailability, (int)TicketStatus.Resolved };
			}

			var tickets = await GetAssetTicketList(siteId, assetId, statuses, scheduled);

            return Ok(tickets);
        }

        [HttpGet("sites/{siteId}/assets/{assetId}/tickets/history")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of tickets associated with the given asset", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetAssetTickets([FromRoute] Guid siteId, [FromRoute] Guid assetId, [FromQuery, BindRequired] TicketListTab tab, [FromQuery] bool scheduled, [FromQuery] string scopeId = "")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            var statuses = await _ticketService.GetTicketsStatuses(tab, siteId);
            var tickets = await GetAssetTicketList(siteId, assetId, statuses, scheduled);

            return Ok(tickets);

        }

        [HttpGet("sites/{siteId}/insights/{insightId}/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of tickets associated with the given insight", Tags = new[] { "Workflow" })]
        public async Task<ActionResult> GetInsightTickets([FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromQuery] bool scheduled,[FromQuery] string scopeId = "")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            var tickets = await _workflowApi.GetTickets(siteId, null, null, null, insightId, scheduled);
            tickets = tickets.OrderByDescending(x => x.CreatedDate).ToList();
            return Ok(TicketSimpleDto.MapFromModels(tickets));
        }
        [Obsolete("Use the endpoint without siteId")]
        [HttpPost("sites/{siteId}/tickets")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Creates a ticket", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> CreateTicket([FromRoute] Guid siteId, [FromForm] CreateTicketRequest request, [FromForm] IFormFileCollection attachmentFiles)
        {
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewSites, siteId);

            request.ReporterCompany ??= string.Empty;

            var (createTicketRequest, validationError) = await ValidateAndBuildRequestForCreateTicket(siteId, request, attachmentFiles, currentUserId);
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var createdTicket = await _ticketService.CreateTicket(siteId, createTicketRequest, attachmentFiles);

            var createdTicketDto = TicketDetailDto.MapFromModel(createdTicket, _imageUrlHelper);

            return Ok(createdTicketDto);
        }

        [HttpPost("tickets")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Creates a ticket", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketByScopeRequest request, [FromForm] IFormFileCollection attachmentFiles)
        {
            var siteId = await GetAuthorizedSitId(request.ScopeId);
            
            request.ReporterCompany ??= string.Empty;

            var (createTicketRequest, validationError) = await ValidateAndBuildRequestForCreateTicket(siteId, request, attachmentFiles, this.GetCurrentUserId());
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var createdTicket = await _ticketService.CreateTicket(siteId, createTicketRequest, attachmentFiles);

            var createdTicketDto = TicketDetailDto.MapFromModel(createdTicket, _imageUrlHelper);

            return Ok(createdTicketDto);
        }


        [Obsolete("Use the endpoint without siteId")]
        [HttpPut("sites/{siteId}/tickets/{ticketId:guid}")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Updates the ticket", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> UpdateTicket([FromRoute] Guid siteId, 
                                                      [FromRoute] Guid ticketId, 
                                                      [FromForm] UpdateTicketRequest request, 
                                                      [FromForm] IFormFileCollection newAttachmentFiles)
        {
			var userId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(userId, Permissions.ViewSites, siteId);

            request.ReporterCompany ??= string.Empty;
             
            if (request.StatusCode.HasValue && request.StatusCode.Value == (int)TicketStatus.Reassign && request.AssigneeId.HasValue)
            {
                request.StatusCode = (int)TicketStatus.Open;
            }

            var (updateTicketRequest, validationError) = await ValidateAndBuildRequestForUpdateTicket(siteId, request);
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var existTicket = await _workflowApi.GetTicket(siteId, ticketId, false);

            if (String.IsNullOrEmpty(updateTicketRequest.IssueName))
            {
                updateTicketRequest.IssueName = existTicket.IssueName;
            }
            updateTicketRequest.CustomerId = existTicket.CustomerId;
            updateTicketRequest.ExternalCreatedDate = existTicket.ExternalCreatedDate;
            updateTicketRequest.ExternalUpdatedDate = existTicket.ExternalUpdatedDate;
            updateTicketRequest.LastUpdatedByExternalSource = false;
			updateTicketRequest.SourceType = TicketSourceType.Platform;
			updateTicketRequest.SourceId = userId;
			var updatedTicket = await _workflowApi.UpdateTicket(siteId, ticketId, updateTicketRequest);

            var attachmentsUpdated = false;
            if (request.AttachmentIds != null)
            {
                foreach (var existAttachment in existTicket.Attachments.Where(existAttachment => !request.AttachmentIds.Contains(existAttachment.Id)))
                {
                    await _workflowApi.DeleteAttachment(siteId, ticketId, existAttachment.Id);
                    attachmentsUpdated = true;
                }
            }

            if (newAttachmentFiles.Any())
            {
                foreach (var attachmentFile in newAttachmentFiles)
                {
					var attachmentDto = new CreateStreamAttachmentDto
					{
						SiteId = siteId,
						TicketId = updatedTicket.Id,
						FileName = attachmentFile.FileName,
						FileStream = attachmentFile.OpenReadStream(),
						SourceId = userId
					};

					await _workflowApi.CreateAttachment(attachmentDto);
					attachmentsUpdated = true;
				}
            }
            if (attachmentsUpdated)
            {
                updatedTicket = await _workflowApi.GetTicket(siteId, updatedTicket.Id, true);
            }

          
            await _ticketService.EnrichTicket(updatedTicket, siteId);
            var updatedTicketDto = TicketDetailDto.MapFromModel(updatedTicket, _imageUrlHelper);

            return Ok(updatedTicketDto);
        }

        [HttpPut("tickets/{ticketId:guid}")]
        [Consumes("multipart/form-data")]
        [Authorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Updates the ticket", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> UpdateTicket(
                                                    [FromRoute] Guid ticketId,
                                                    [FromForm] UpdateTicketByScopeRequest request,
                                                    [FromForm] IFormFileCollection newAttachmentFiles)
        {
            var userId = this.GetCurrentUserId();
            var siteId = await GetAuthorizedSitId(request.ScopeId);
            request.ReporterCompany ??= string.Empty;

            if (request.StatusCode.HasValue && request.StatusCode.Value == (int)TicketStatus.Reassign && request.AssigneeId.HasValue)
            {
                request.StatusCode = (int)TicketStatus.Open;
            }

            var (updateTicketRequest, validationError) = await ValidateAndBuildRequestForUpdateTicket(siteId, request);
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            var existTicket = await _workflowApi.GetTicket(siteId, ticketId, false);

            if (string.IsNullOrEmpty(updateTicketRequest.IssueName))
            {
                updateTicketRequest.IssueName = existTicket.IssueName;
            }
            updateTicketRequest.CustomerId = existTicket.CustomerId;
            updateTicketRequest.ExternalCreatedDate = existTicket.ExternalCreatedDate;
            updateTicketRequest.ExternalUpdatedDate = existTicket.ExternalUpdatedDate;
            updateTicketRequest.LastUpdatedByExternalSource = false;
            updateTicketRequest.SourceType = TicketSourceType.Platform;
            updateTicketRequest.SourceId = userId;
            var updatedTicket = await _workflowApi.UpdateTicket(siteId, ticketId, updateTicketRequest);

            var attachmentsUpdated = false;
            if (request.AttachmentIds != null)
            {
                foreach (var existAttachment in existTicket.Attachments.Where(existAttachment => !request.AttachmentIds.Contains(existAttachment.Id)))
                {
                    await _workflowApi.DeleteAttachment(siteId, ticketId, existAttachment.Id);
                    attachmentsUpdated = true;
                }
            }

            if (newAttachmentFiles.Any())
            {
                foreach (var attachmentFile in newAttachmentFiles)
                {
                    var attachmentDto = new CreateStreamAttachmentDto
                    {
                        SiteId = siteId,
                        TicketId = updatedTicket.Id,
                        FileName = attachmentFile.FileName,
                        FileStream = attachmentFile.OpenReadStream(),
                        SourceId = userId
                    };

                    await _workflowApi.CreateAttachment(attachmentDto);
                    attachmentsUpdated = true;
                }
            }
            if (attachmentsUpdated)
            {
                updatedTicket = await _workflowApi.GetTicket(siteId, updatedTicket.Id, true);
            }


            await _ticketService.EnrichTicket(updatedTicket, siteId);
            var updatedTicketDto = TicketDetailDto.MapFromModel(updatedTicket, _imageUrlHelper);

            return Ok(updatedTicketDto);
        }

        [HttpGet("sites/{siteId}/tickets/categories")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketCategoryDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Return ticket categories", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> GetTicketCategories([FromRoute] Guid siteId, [FromQuery] string scopeId = "")
        {
            siteId = await GetAuthorizedSitId(scopeId, siteId);

            var ticketCategories = await _workflowApi.GetTicketCategories(siteId);
            return Ok(TicketCategoryDto.MapFrom(ticketCategories));
        }

        [HttpGet("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Return ticket category", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> GetTicketCategory([FromRoute] Guid siteId, [FromRoute] Guid ticketCategoryId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var ticketCategory = await _workflowApi.GetTicketCategory(siteId, ticketCategoryId);
            return Ok(TicketCategoryDto.MapFrom(ticketCategory));
        }

        [HttpPost("sites/{siteId}/tickets/categories")]
        [Authorize]
        [ProducesResponseType(typeof(TicketCategoryDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Create ticket category", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> CreateTicketCategory([FromRoute] Guid siteId, [FromBody] CreateTicketCategoryRequest createTicketCategoryRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var ticketCategory = await _workflowApi.CreateTicketCategory(siteId, createTicketCategoryRequest);
            return Ok(TicketCategoryDto.MapFrom(ticketCategory));
        }

        [HttpDelete("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Delete ticket category", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> DeleteTicketCategory([FromRoute] Guid siteId, [FromRoute] Guid ticketCategoryId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            await _workflowApi.DeleteTicketCategory(siteId, ticketCategoryId);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/tickets/categories/{ticketCategoryId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Update ticket category", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> UpdateTicketCategory([FromRoute] Guid siteId, [FromRoute] Guid ticketCategoryId, [FromBody] UpdateTicketCategoryRequest updateTicketCategoryRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            await _workflowApi.UpdateTicketCategory(siteId, ticketCategoryId, updateTicketCategoryRequest);
            return NoContent();
        }

        [HttpGet("tickets/ticketCategoricalData")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketCategoricalData>), StatusCodes.Status200OK)]
        [SwaggerOperation("Return ticket categorical data", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> GetTicketCategoricalData()
        {
            var result = await _workflowApi.GetTicketCategoricalData();
            return Ok(result);
        }

        /// <summary>
        /// returns ticket priority and status statistics by user's siteIds
        /// </summary>
        /// <param name="scopeIds">Ids of the requested ticket's locations</param>
        /// <returns>List of status and priority statistics for ticket</returns>
        [HttpPost("tickets/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(TicketStatisticsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserTicketsStatistics([FromBody] List<string> scopeIds)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(),scopeIds);
            return Ok(await _workflowApi.GetTicketStatisticsBySiteIdsAsync(siteIds));
        }

        /// <summary>
        /// Retrieves a list of ticket sub-statuses.
        /// </summary>
        /// <returns>A list of ticket sub-statuses.</returns>
        [HttpGet("tickets/subStatus")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketSubStatus>), StatusCodes.Status200OK)]
        [SwaggerOperation("Return ticket sub-statuses", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> GetTicketSubStatus()
        {
            return Ok(await _workflowApi.GetTicketSubStatus());
        }

        /// <summary>
        /// Get ticket category count by space twin Id based on the limit number of categories
        /// if the limit is not provided, it will return the top 5 categories
        /// and sum of the rest of the categories as other
        /// </summary>
        /// <param name="spaceTwinId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("tickets/twins/{spaceTwinId}/ticketCountsByCategory")]
        [Authorize]
        [SwaggerOperation("Get ticket Category counts by Space Twin Id", Tags = new[] { "Workflow" })]
        [ProducesResponseType(typeof(TicketCategoryCountResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketCategoryCountBySpaceTwinId([FromRoute] string spaceTwinId, [FromQuery] int? limit = 5)
        {
            var ticketCategoryCount = await _workflowApi.GetTicketCategoryCountBySpaceTwinId(spaceTwinId, limit);
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
        [SwaggerOperation("Retrieves the count of created tickets of each day within a specified date range. for a given space twin ID.", Tags = new[] { "Workflow" })]
        [ProducesResponseType(typeof(TicketCountsByDateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTicketsCountsByCreatedDate([FromRoute] string spaceTwinId, [FromQuery] DateTime? startDate, DateTime? endDate)
        {
            if (startDate is null || endDate is null)
            {
                return BadRequest("The start date and end date are required.");
            }
            if (startDate >= endDate)
            {
                return BadRequest("The end date must be greater than start date");
            }
            var ticketCounts = await _workflowApi.GetTicketsCountsByCreatedDate(spaceTwinId, startDate.Value, endDate.Value);
            return Ok(ticketCounts);
        }

        /// <summary>
        /// Returns the ticket status transitions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("tickets/statusTransitions")]
        [Authorize]
        [ProducesResponseType(typeof(TicketStatusTransitionsResponse), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns the ticket status transitions.", Tags = new[] { "Workflow" })]
        public async Task<IActionResult> GetTicketStatusTransitions()
        {
            var result = await _memoryCache.GetOrCreateAsync(nameof(GetTicketStatusTransitions), async  cache =>
            {
                cache.SetAbsoluteExpiration(TimeSpan.FromHours(6));
                return await _workflowApi.GetTicketStatusTransitionsAsync();
            });

            return Ok(result);
        }

        private async Task<(WorkflowCreateTicketRequest createTicketRequest, ValidationError error)> ValidateAndBuildRequestForCreateTicket(
            Guid siteId,
            CreateTicketRequest request,
            IFormFileCollection attachmentFiles,
            Guid currentUserId)
        {
            var requestToWorkflow = new WorkflowCreateTicketRequest();
            var error = new ValidationError();

            request.SourceType ??= TicketSourceType.Platform;
            if ((request.SourceType.Value != TicketSourceType.Platform) && (request.SourceType.Value != TicketSourceType.Dynamics))
            {
                error.Items.Add(new ValidationErrorItem("sourceType", "Unsupported source type"));
            }

            if (!Attachments.IsValid(attachmentFiles))
            {
                error.Items.Add(new ValidationErrorItem("attachmentFiles", "Invalid or unsupported image files"));
            }

            var insightName = string.Empty;
            if (request.InsightId.HasValue)
            {
	            var insight = await _insightApi.GetInsight(siteId, request.InsightId.Value);
	            if (insight.LastStatus == InsightStatus.Resolved || insight.LastStatus == InsightStatus.Ignored)
	            {
		            error.Items.Add(new ValidationErrorItem("insightStatus", "Cannot create ticket for the resolved/archived insight"));
	            }
	            insightName = insight.SequenceNumber;
                request.Description = request.Description + "\r\n" + insight.Recommendation;
            }

			if (error.Items.Any())
            {
                return (null, error);
            }

            var site = await _siteApi.GetSite(siteId);
            if (site == null)
            {
                throw new NotFoundException().WithData(new { siteId });
            }

            var issueName = await GetIssueName(siteId, request.IssueType, request.IssueId);

            requestToWorkflow.CustomerId = site.CustomerId;
            requestToWorkflow.FloorCode = request.FloorCode;
            requestToWorkflow.SequenceNumberPrefix = site.Code;
            requestToWorkflow.Priority = request.Priority;
            requestToWorkflow.IssueType = request.IssueType;
            requestToWorkflow.IssueId = request.IssueId;
            requestToWorkflow.IssueName = issueName;
            requestToWorkflow.InsightId = request.InsightId;
            requestToWorkflow.InsightName = insightName;
            requestToWorkflow.Diagnostics = request.Diagnostics;
            requestToWorkflow.Summary = request.Summary;
            requestToWorkflow.Description = request.Description;
            requestToWorkflow.Cause = request.Cause;
            requestToWorkflow.ReporterId = request.ReporterId;
            requestToWorkflow.ReporterName = request.ReporterName;
            requestToWorkflow.ReporterPhone = request.ReporterPhone;
            requestToWorkflow.ReporterEmail = request.ReporterEmail.CleanEmail();
            requestToWorkflow.ReporterCompany = request.ReporterCompany;
            requestToWorkflow.AssigneeType = request.AssigneeType;
            requestToWorkflow.AssigneeId = request.AssigneeId;
            requestToWorkflow.CreatorId = currentUserId;
            requestToWorkflow.DueDate = request.DueDate;
            requestToWorkflow.SourceType = request.SourceType.Value;
            requestToWorkflow.SourceId = null;
            requestToWorkflow.ExternalId = string.Empty;
            requestToWorkflow.ExternalStatus = string.Empty;
            requestToWorkflow.ExternalMetadata = string.Empty;
            requestToWorkflow.CategoryId = request.CategoryId;
            requestToWorkflow.Latitude = request.Latitude;
            requestToWorkflow.Longitude = request.Longitude;
            requestToWorkflow.TwinId = request.TwinId;
            requestToWorkflow.JobTypeId = request.JobTypeId;
            requestToWorkflow.ServiceNeededId = request.ServiceNeededId;
            
            return (requestToWorkflow, error);
        }

        private async Task<(WorkflowUpdateTicketRequest updateTicketRequest, ValidationError error)> ValidateAndBuildRequestForUpdateTicket(
            Guid siteId,
            UpdateTicketRequest request)
        {
            var requestToWorkflow = new WorkflowUpdateTicketRequest();
            var error = new ValidationError();
            var status = request.StatusCode;

            if (request.Template)
            {
                if (string.IsNullOrEmpty(request.Notes) && (status == (int)TicketStatus.LimitedAvailability || status == (int)TicketStatus.Resolved || status == (int)TicketStatus.Closed))
                {
                    error.Items.Add(new ValidationErrorItem("Notes", "Notes are required"));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(request.Cause) && (status == (int)TicketStatus.LimitedAvailability || status == (int)TicketStatus.Resolved || status == (int)TicketStatus.Closed))
                {
                    error.Items.Add(new ValidationErrorItem("Cause", "Cause is required"));
                }

                if (string.IsNullOrEmpty(request.Solution) && (status == (int)TicketStatus.LimitedAvailability || status == (int)TicketStatus.Resolved || status == (int)TicketStatus.Closed))
                {
                    error.Items.Add(new ValidationErrorItem("Solution", "Solution is required"));
                }
            }

            if (error.Items.Any())
            {
                return (null, error);
            }

            var issueName = String.Empty;
            if (request.IssueId.HasValue)
                issueName = await GetIssueName(siteId, request.IssueType, request.IssueId);

            requestToWorkflow.Priority = request.Priority;
            requestToWorkflow.Status = request.StatusCode;
            requestToWorkflow.FloorCode = request.FloorCode;
            requestToWorkflow.IssueType = request.IssueType;
            requestToWorkflow.IssueId = request.IssueId;
            requestToWorkflow.IssueName = issueName;
            requestToWorkflow.Summary = request.Summary;
            requestToWorkflow.Description = request.Description;
            requestToWorkflow.Notes = request.Notes;
            requestToWorkflow.Cause = request.Cause;
            requestToWorkflow.Solution = request.Solution;
            requestToWorkflow.ShouldUpdateReporterId = true;
            requestToWorkflow.ReporterId = request.ReporterId;
            requestToWorkflow.ReporterName = request.ReporterName;
            requestToWorkflow.ReporterPhone = request.ReporterPhone;
            requestToWorkflow.ReporterEmail = request.ReporterEmail.CleanEmail();
            requestToWorkflow.ReporterCompany = request.ReporterCompany;
            requestToWorkflow.AssigneeType = request.AssigneeType;
            requestToWorkflow.AssigneeId = request.AssigneeId;
            requestToWorkflow.DueDate = request.DueDate;
            requestToWorkflow.CategoryId = request.CategoryId;
            requestToWorkflow.Assets = request.Assets;
            requestToWorkflow.Tasks = request.Tasks;
            requestToWorkflow.JobTypeId = request.JobTypeId;
            requestToWorkflow.ServiceNeededId = request.ServiceNeededId;
            requestToWorkflow.SubStatusId = request.SubStatusId;
            requestToWorkflow.TwinId = request.TwinId;

            return (requestToWorkflow, error);
        }

        private async Task<string> GetIssueName(Guid siteId, TicketIssueType issueType, Guid? issueId)
        {
            var asset = (Asset)null;

            switch (issueType)
            {
                case TicketIssueType.NoIssue:
                    break;
                case TicketIssueType.Equipment:
                    asset = await _digitalTwinService.GetAssetDetailsByEquipmentIdAsync(siteId, issueId.Value);
                    break;
                case TicketIssueType.Asset:
                    asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, issueId.Value);
                    break;
                default:
                    throw new ArgumentException().WithData(new { requestIssueType = issueType });
            }

            return asset?.Name ?? string.Empty;
        }

        private async Task<IList<TicketSimpleDto>> GetAssetTicketList(Guid siteId, Guid assetId, int[] statuses, bool scheduled)
        {
            var tickets = await _workflowApi.GetTickets(siteId, statuses, TicketIssueType.Asset, assetId, null, scheduled);

           
            var asset = await _digitalTwinService.GetAssetDetailsAsync(siteId, assetId);

            if (!asset.EquipmentId.HasValue) 
            {
                return TicketSimpleDto.MapFromModels(tickets); 
            }

            var ticketsForEquipment = await _workflowApi.GetTickets(siteId, statuses, TicketIssueType.Equipment, asset.EquipmentId.Value, null, scheduled);

            tickets.AddRange(ticketsForEquipment);
            var uniqueTickets = tickets.Distinct(new TicketEqualityComparer()).ToList();
            return TicketSimpleDto.MapFromModels(uniqueTickets);
        }

        private async Task<Guid> GetAuthorizedSitId(string scopeId, Guid? siteId=null)
        {
            return (await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId,siteId.HasValue? new List<Guid>() { siteId.Value }:null,
                c => !c.Features.IsTicketingDisabled)).FirstOrDefault();
        }
    }
}
