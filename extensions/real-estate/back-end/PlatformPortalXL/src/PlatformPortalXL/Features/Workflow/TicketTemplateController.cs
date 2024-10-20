using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using Willow.Platform.Users;
using Willow.Workflow;

namespace PlatformPortalXL.Features.Workflow
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TicketTemplateController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWorkflowApiService _workflowApi;
        private readonly ISiteApiService _siteApi;
        private readonly IAccessControlService _accessControl;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly ILogger<TicketTemplateController> _logger;

        public TicketTemplateController(
            IWorkflowApiService workflowApi,
            ISiteApiService siteApi,
            IUserService userService,
            IAccessControlService accessControl,
            IImageUrlHelper imageUrlHelper,
            ILogger<TicketTemplateController> logger)
        {
            _workflowApi = workflowApi;
            _siteApi = siteApi;
            _userService = userService;
            _accessControl = accessControl;
            _imageUrlHelper = imageUrlHelper;
            _logger = logger;
        }

        [HttpGet("sites/{siteId}/tickettemplate")]
        [Authorize]
        [ProducesResponseType(typeof(List<TicketTemplateDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetTicketTemplates([FromRoute] Guid siteId, [FromQuery] bool? archived)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var ticketTemplates = await _workflowApi.GetTicketTemplates(siteId, archived);
            var assignees = await _userService.GetAssignees(siteId, ticketTemplates);

            foreach (var ticketTemplate in ticketTemplates)
            {
                if(ticketTemplate.AssigneeId.HasValue)
                { 
                    if(!assignees.TryGetValue(ticketTemplate.AssigneeId.Value, out IUser user))
                        _logger.LogError("Unable to find TicketTemplate [{TicketTemplateId}] assignee [{AssigneeId}] of type {AssigneeType}",
                            ticketTemplate.Id, ticketTemplate.AssigneeId, ticketTemplate.AssigneeType);
                    else
                        ticketTemplate.Assignee = user.ToAssignee();
                }
            }

            return Ok(TicketTemplateDto.MapFromModels(ticketTemplates, _imageUrlHelper));
        }

        [HttpGet("sites/{siteId}/tickettemplate/{ticketTemplateId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTicketTemplate([FromRoute] Guid siteId, [FromRoute] Guid ticketTemplateId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var ticketTemplate = await _workflowApi.GetTicketTemplate(siteId, ticketTemplateId);

            var assignee = await GetTicketTemplateAssignee(ticketTemplate);
            
            var templateDto = TicketTemplateDto.MapFromModel(ticketTemplate, _imageUrlHelper);

            if(templateDto.AssigneeName == null)
                templateDto.AssigneeName = assignee == null ? default(string) : assignee.Name;

            return Ok(templateDto);
        }

        [HttpPut("sites/{siteId}/tickettemplate/{ticketTemplateId}")]
        [Authorize]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateTicketTemplate(
            [FromRoute] Guid siteId,
            [FromRoute] Guid ticketTemplateId,
            [FromBody] UpdateTicketTemplateRequest updateTicketTemplateRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var request = await BuildRequestForUpdateTicketTemplate(siteId, updateTicketTemplateRequest);
            var ticketTemplate = await _workflowApi.UpdateTicketTemplate(siteId, ticketTemplateId, request);

            return Ok(TicketTemplateDto.MapFromModel(ticketTemplate, _imageUrlHelper));
        }

        [HttpPost("sites/{siteId}/tickettemplate")]
        [Authorize]
        [ProducesResponseType(typeof(TicketTemplateDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        public async Task<IActionResult> CreateTicketTemplate(
            [FromRoute] Guid siteId,
            [FromBody] CreateTicketTemplateRequest createTicketTemplateRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var request = await BuildRequestForCreateTicketTemplate(siteId, createTicketTemplateRequest);
            var ticketTemplate = await _workflowApi.CreateTicketTemplate(siteId, request);

            return Ok(TicketTemplateDto.MapFromModel(ticketTemplate, _imageUrlHelper));
        }

        private async Task<WorkflowCreateTicketTemplateRequest> BuildRequestForCreateTicketTemplate(Guid siteId, CreateTicketTemplateRequest request)
        {
            var requestToWorkflow = new WorkflowCreateTicketTemplateRequest();
            var site = await _siteApi.GetSite(siteId);

            if (site == null)
            {
                throw new NotFoundException().WithData(new { SiteId  = siteId });
            }

            if (string.IsNullOrWhiteSpace(request.Recurrence.Timezone))
                request.Recurrence.Timezone = site.TimeZoneId;

            requestToWorkflow.CustomerId = site.CustomerId;
            requestToWorkflow.FloorCode = request.FloorCode;
            requestToWorkflow.SequenceNumberPrefix = site.Code;
            requestToWorkflow.Priority = request.Priority;
            requestToWorkflow.Summary = request.Summary;
            requestToWorkflow.Description = request.Description;
            requestToWorkflow.ReporterId = request.ReporterId;
            requestToWorkflow.ReporterName = request.ReporterName;
            requestToWorkflow.ReporterPhone = request.ReporterPhone;
            requestToWorkflow.ReporterEmail = request.ReporterEmail;
            requestToWorkflow.ReporterCompany = request.ReporterCompany;
            requestToWorkflow.AssigneeType = request.AssigneeType;
            requestToWorkflow.AssigneeId = request.AssigneeId;
            requestToWorkflow.CategoryId = request.CategoryId;
            requestToWorkflow.Category = request.Category;
            requestToWorkflow.Recurrence = request.Recurrence;
            requestToWorkflow.OverdueThreshold = request.OverdueThreshold;
            requestToWorkflow.Assets = request.Assets;
            requestToWorkflow.Twins = request.Twins;
            requestToWorkflow.Tasks = request.Tasks;
            requestToWorkflow.DataValue = request.DataValue;

            return requestToWorkflow;
        }

        private async Task<WorkflowUpdateTicketTemplateRequest> BuildRequestForUpdateTicketTemplate(
            Guid siteId,
            UpdateTicketTemplateRequest request)
        {
            var requestToWorkflow = new WorkflowUpdateTicketTemplateRequest();
            var site = await _siteApi.GetSite(siteId);

            if (string.IsNullOrWhiteSpace(request.Recurrence.Timezone))
                request.Recurrence.Timezone = site.TimeZoneId;

            requestToWorkflow.CustomerId = site.CustomerId;
            requestToWorkflow.Priority = request.Priority;
            requestToWorkflow.Status = request.Status;
            requestToWorkflow.FloorCode = request.FloorCode;
            requestToWorkflow.Summary = request.Summary;
            requestToWorkflow.Description = request.Description;
            requestToWorkflow.ShouldUpdateReporterId = request.ShouldUpdateReporterId;
            requestToWorkflow.ReporterId = request.ReporterId;
            requestToWorkflow.ReporterName = request.ReporterName;
            requestToWorkflow.ReporterPhone = request.ReporterPhone;
            requestToWorkflow.ReporterEmail = request.ReporterEmail.CleanEmail();
            requestToWorkflow.ReporterCompany = request.ReporterCompany;
            requestToWorkflow.AssigneeType = request.AssigneeType;
            requestToWorkflow.AssigneeId = request.AssigneeId;
            requestToWorkflow.Recurrence = request.Recurrence;
            requestToWorkflow.OverdueThreshold = request.OverdueThreshold;
            requestToWorkflow.Attachments = request.Attachments;
            requestToWorkflow.CategoryId = request.CategoryId;
            requestToWorkflow.Assets = request.Assets;
            requestToWorkflow.Twins = request.Twins;
            requestToWorkflow.Tasks = request.Tasks;
            requestToWorkflow.PerformScheduleHitOnAddedAssets = request.PerformScheduleHitOnAddedAssets;

            return requestToWorkflow;
        }

        private async Task<TicketAssignee> GetTicketTemplateAssignee(TicketTemplate ticketTemplate)
        {
            if(ticketTemplate.AssigneeType != TicketAssigneeType.NoAssignee && ticketTemplate.AssigneeId.HasValue)
            {
                try
                { 
                    var userId = ticketTemplate.AssigneeUserId();
                    var user   = await _userService.GetUser(ticketTemplate.SiteId, userId.UserId, userId.UserType);

                    return user.ToAssignee();
                }
                catch(NotFoundException)
                {
                    _logger.LogError("Ticket assignee not found");
                }
               catch(Exception ex)
                {
                    _logger.LogError(ex, "Unable to set the ticket assignee");
                }
            }

            return null;
        }
    }
}
