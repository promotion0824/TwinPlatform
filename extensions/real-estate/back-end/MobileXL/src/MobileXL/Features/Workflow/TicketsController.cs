using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using MobileXL.Dto;
using MobileXL.Models;
using MobileXL.Security;
using MobileXL.Services;
using MobileXL.Services.Apis.DirectoryApi;
using MobileXL.Services.Apis.SiteApi;
using MobileXL.Services.Apis.WorkflowApi;
using Swashbuckle.AspNetCore.Annotations;
using MobileXL.Services.Apis.WorkflowApi.Requests;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Notifications;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;

namespace MobileXL.Features.Workflow
{
	[ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private static readonly IReadOnlyList<(TicketStatus FromStatus, TicketStatus ToStatus)> AllowedStatusChanges = new List<(TicketStatus, TicketStatus)>
        {
            ( TicketStatus.Open, TicketStatus.InProgress ),
            ( TicketStatus.Open, TicketStatus.Reassign ),
            ( TicketStatus.InProgress, TicketStatus.LimitedAvailability ),
            ( TicketStatus.InProgress, TicketStatus.Resolved ),
            ( TicketStatus.LimitedAvailability, TicketStatus.Resolved ),
        };
		private const string _onHoldStatus = "OnHold";

        private readonly IAccessControlService _accessControl;
        private readonly IImageUrlHelper _imageUrlHelper;
        private readonly INotificationService _notificationService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;
        private readonly IWorkflowApiService _workflowApi;
        private readonly IUserCache _userCache;

        private readonly string _commandPortalBaseUrl;

        public TicketsController(
            IConfiguration configuration, 
            IAccessControlService accessControl, 
            IImageUrlHelper imageUrlHelper,
            INotificationService notificationService, 
            IDirectoryApiService directoryApi, 
            ISiteApiService siteApi, 
            IWorkflowApiService workflowApi, 
            IUserCache userCache)
        {
                _accessControl = accessControl ?? throw new ArgumentNullException(nameof(accessControl));
                _imageUrlHelper = imageUrlHelper ?? throw new ArgumentNullException(nameof(imageUrlHelper));
                _notificationService =
                    notificationService ?? throw new ArgumentNullException(nameof(notificationService));
                _directoryApi = directoryApi ?? throw new ArgumentNullException(nameof(directoryApi));
                _siteApi = siteApi ?? throw new ArgumentNullException(nameof(siteApi));
                _workflowApi = workflowApi ?? throw new ArgumentNullException(nameof(workflowApi));
                _userCache = userCache ?? throw new ArgumentNullException(nameof(userCache));
                _commandPortalBaseUrl = configuration.GetValue<string>("CommandPortalBaseUrl");
        }

        [HttpGet("me/tickets")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(List<TicketSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the list of tickets assigned to the current user", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> GetCurrentUserTickets([FromQuery] Guid siteId, [FromQuery, BindRequired] TicketListTab tab, [FromQuery] bool scheduled=false)
        {
            var currentUserId = this.GetCurrentUserId();
            var currentUserType = this.GetCurrentUserType();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

			var site = await _siteApi.GetSite(siteId);
			var ticketStatuses = await _workflowApi.GetCustomerTicketStatus(site.CustomerId);

			int[] statuses;
			if (ticketStatuses?.Any() ?? false)
			{
				statuses = ticketStatuses.Where(x => x.Tab.Equals(tab.ToString(), StringComparison.InvariantCultureIgnoreCase)).Select(x => x.StatusCode).ToArray();
				if (!statuses.Any())
				{
					throw new ArgumentException($"Unknown tab: {tab}");
				}
			}
			else
			{
				switch (tab)
				{
					case TicketListTab.Open:
						statuses = new[] { (int)TicketStatus.Open, (int)TicketStatus.InProgress, (int)TicketStatus.Reassign, (int)TicketStatus.LimitedAvailability };
						break;
					case TicketListTab.Resolved:
						statuses = new[] { (int)TicketStatus.Resolved };
						break;
					case TicketListTab.Closed:
						statuses = new[] { (int)TicketStatus.Closed };
						break;
					default:
						throw new ArgumentException($"Unknown tab: {tab}");
				}
			}

            var isCustomerAdmin = currentUserType == UserTypeNames.CustomerUser && await IsCustomerAdmin(currentUserId);

            var tickets = await _workflowApi.GetSiteTickets(siteId, currentUserId, statuses, scheduled, isCustomerAdmin);
            await AddUnassignedTickets(siteId, scheduled, currentUserId, statuses, tickets, isCustomerAdmin);
            if (!isCustomerAdmin)
            {
                // Only add workgroup tickets if the user is not a customer admin, since already get all tickets for customer admins
                await AddWorkgroupsTickets(siteId, scheduled, currentUserId, statuses, tickets);
            }

            tickets = tickets.GroupBy(x => x.Id).Select(x => x.First()).OrderBy(x => x.Priority).ThenBy(x => x.Status).ThenByDescending(x => x.CreatedDate).ToList();
            return Ok(TicketSimpleDto.MapFromModels(tickets));
        }

        [HttpGet("sites/{siteId}/tickets/{ticketId}")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the specific ticket", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> GetTicket(Guid siteId, Guid ticketId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var ticket = await _workflowApi.GetTicket(siteId, ticketId, true);

            await EnrichTicket(ticket);

            await EnrichTicketAssignee(ticket);

            return Ok(TicketDetailDto.MapFromModel(ticket, _imageUrlHelper));
        }

        [HttpGet("sites/{siteId}/possibleTicketAssignees")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(List<TicketAssignee>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of possible ticket assignees", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> GetPossibleTicketAssignees([FromRoute] Guid siteId)
        {
            var currentUserType = this.GetCurrentUserType();
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var workGroups = await _workflowApi.GetWorkgroups(siteId);

            var customerUsers = await _directoryApi.GetSiteUsers(siteId);

            var assignees = customerUsers
                .Where(x => x.Status == UserStatus.Active)
                .Select(x => TicketAssignee.FromCustomerUser(x))
                .Union(workGroups.Select(x => TicketAssignee.FromWorkgroup(x)))
                .ToList();

            return Ok(assignees);
        }

        [HttpPut("sites/{siteId}/tickets/{ticketId}/status")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Update the status of a specific ticket", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> UpdateTicketStatus(Guid siteId, Guid ticketId, [FromBody] UpdateTicketStatusRequest request)
        {
            var currentUserType = this.GetCurrentUserType();
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var currentTicket = await _workflowApi.GetTicket(siteId, ticketId, false);
			var site = await _siteApi.GetSite(siteId);
			var ticketStatuses = await _workflowApi.GetCustomerTicketStatus(site.CustomerId);

			var requestToWorkflowApi = ValidateRequestAndCreateWorkflowRequest(currentTicket, currentUserType, currentUserId, request, ticketStatuses);
			requestToWorkflowApi.CustomerId = site.CustomerId;
			var updatedTicket = await _workflowApi.UpdateTicket(siteId, ticketId, requestToWorkflowApi);
            if (request.StatusCode == (int)TicketStatus.Reassign)
            {
                await _workflowApi.CreateComment(siteId, ticketId, new WorkflowCreateCommentRequest
                {
                    Text = request.Open2Reassign.RejectComment,
                    CreatorType = CommentCreatorType.CustomerUser,
                    CreatorId = currentUserId
                });
                updatedTicket = await _workflowApi.GetTicket(siteId, ticketId, true);
            }

            if (request.StatusCode == (int)TicketStatus.Resolved)
            {
                await SendEmailToCreator(updatedTicket, currentUserType);
            }

            await EnrichTicket(updatedTicket);

            return Ok(TicketDetailDto.MapFromModel(updatedTicket, _imageUrlHelper));
        }

        [HttpPut("sites/{siteId}/tickets/{ticketId}/assignee")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Update the assignee of a specific ticket", Tags = new [] { "Workflow" })]
        public async Task<ActionResult> UpdateTicketAssignee(Guid siteId, Guid ticketId, [FromBody] UpdateTicketAssigneeRequest request)
        {
            var currentUserType = this.GetCurrentUserType();
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var currentTicket = await _workflowApi.GetTicket(siteId, ticketId, false);

            if (currentTicket.Status != (int)TicketStatus.Open
                && currentTicket.Status != (int)TicketStatus.Reassign
                && currentTicket.Status != (int)TicketStatus.InProgress
                && currentTicket.Status != (int)TicketStatus.LimitedAvailability)
            {
                throw new ArgumentException($"Ticket of '{currentTicket.Status}' status cannot be reassigned.");
            }

            var requestToWorkflowApi = new WorkflowUpdateTicketRequest
            {
                AssigneeType = request.AssigneeType,
                AssigneeId = request.AssigneeId,
                CategoryId = currentTicket.CategoryId,
                ExternalCreatedDate = currentTicket.ExternalCreatedDate,
                ExternalUpdatedDate = currentTicket.ExternalUpdatedDate,
                LastUpdatedByExternalSource = false,
				SourceId = currentUserId
            };
            if (currentTicket.Status == (int)TicketStatus.Reassign && request.AssigneeId.HasValue)
            {
                requestToWorkflowApi.Status = (request.AssigneeId.Value == currentUserId) ? (int)TicketStatus.InProgress : (int)TicketStatus.Open;
            }
            var updatedTicket = await _workflowApi.UpdateTicket(siteId, ticketId, requestToWorkflowApi);

            var commentText = string.Empty;
            if (updatedTicket.AssigneeType == TicketAssigneeType.NoAssignee)
            {
                commentText = "Assigned to workgroup";
            }
            else if (updatedTicket.AssigneeType == TicketAssigneeType.CustomerUser)
            {
                var newAssignee = await _userCache.GetCustomerUser(updatedTicket.CustomerId, updatedTicket.AssigneeId.Value);
                commentText = $"Assigned to {newAssignee.FirstName} {newAssignee.LastName}";
            }
            else if (updatedTicket.AssigneeType == TicketAssigneeType.WorkGroup)
            {
                var workgroups = await _workflowApi.GetWorkgroups(siteId);
                var assignedWorkgroup = workgroups.Single(x => x.Id == updatedTicket.AssigneeId.Value);
                commentText = $"Assigned to {assignedWorkgroup.Name}";
            }
            await _workflowApi.CreateComment(siteId, ticketId, new WorkflowCreateCommentRequest
            {
                Text = commentText,
                CreatorType = CommentCreatorType.CustomerUser,
                CreatorId = currentUserId
            });
            updatedTicket = await _workflowApi.GetTicket(siteId, ticketId, true);
                        
            await EnrichTicket(updatedTicket);

            return Ok(TicketDetailDto.MapFromModel(updatedTicket, _imageUrlHelper));
        }

        [HttpPut("sites/{siteId}/tickets/{ticketId}/tasks")]
        [MobileAuthorize]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Update scheduled ticket tasks", Tags = new [] { "Workflow" })]
        public async Task<IActionResult> UpdateTicketTasks([FromRoute] Guid siteId, [FromRoute] Guid ticketId, [FromBody] UpdateTicketTasksRequest request)
        {
            var currentUserType = this.GetCurrentUserType();
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var scheduledTicket = await _workflowApi.GetTicket(siteId, ticketId, false);

            if (scheduledTicket.TemplateId == null)
            {
                throw new ArgumentException("Ticket is not scheduled");
            }

            foreach (var task in request.Tasks)
            {
                var scheduledTicketTask = scheduledTicket.Tasks.Find(x => x.Id == task.Id);
                if (scheduledTicketTask == null)
                {
                    throw new ArgumentException("Updated task doesn't match with the scheduled ticket tasks");
                }

                scheduledTicketTask.IsCompleted = task.IsCompleted;
                scheduledTicketTask.NumberValue = task.NumberValue;
            }

            scheduledTicket.Notes = request.Notes;

            var updateTicketRequest = new WorkflowUpdateTicketRequest
            {
                Status = scheduledTicket.Status,
                AssigneeType = scheduledTicket.AssigneeType,
                AssigneeId = scheduledTicket.AssigneeId,
                CategoryId = scheduledTicket.CategoryId,
                Tasks =  scheduledTicket.Tasks,
                Notes = request.Notes
            };

            await _workflowApi.UpdateTicket(siteId, ticketId, updateTicketRequest);
            var updatedScheduledTicket = await _workflowApi.GetTicket(siteId, ticketId, true);
            await EnrichTicket(updatedScheduledTicket);

            return Ok(TicketDetailDto.MapFromModel(updatedScheduledTicket, _imageUrlHelper));
        }

		[HttpGet("customers/{customerId}/ticketStatuses")]
		[MobileAuthorize]
		[ProducesResponseType(typeof(List<CustomerTicketStatusDto>), StatusCodes.Status200OK)]
		[SwaggerOperation("Gets ticket statuses for specified customerid", Tags = new[] { "Workflow" })]
		public async Task<IActionResult> GetCustomerTicketStatuses(Guid customerId)
		{
			var result = await _workflowApi.GetCustomerTicketStatus(customerId);
			return Ok(result);
		}

		private async Task AddUnassignedTickets(Guid siteId, bool scheduled, Guid currentUserId, int[] statuses, List<Ticket> tickets, bool isCustomerAdmin)
        {
            if (isCustomerAdmin || (await _workflowApi.GetNotificationReceiverIds(siteId)).Any(x => x == currentUserId))
            {
                var unassignedTickets = await _workflowApi.GetSiteUnassignedTickets(siteId, statuses, scheduled);
                tickets.AddRange(unassignedTickets);
            }
        }

        private async Task AddWorkgroupsTickets(Guid siteId, bool scheduled, Guid currentUserId, int[] statuses, List<Ticket> tickets)
        {
            var workGroups = await _workflowApi.GetWorkgroups(siteId);
            var userWorkgroups = workGroups.Where(x => x.MemberIds.Contains(currentUserId)).ToList();

            foreach(var workGroup in userWorkgroups)
            {
                var workGroupsTickets = await _workflowApi.GetSiteTickets(siteId, workGroup.Id, statuses, scheduled, isCustomerAdmin:false);
                tickets.AddRange(workGroupsTickets);
            }
        }

        private static WorkflowUpdateTicketRequest ValidateRequestAndCreateWorkflowRequest(Ticket currentTicket,
																					string currentUserType,
																					Guid currentUserId,
																					UpdateTicketStatusRequest request,
																					List<CustomerTicketStatus> ticketStatuses)
        {
            var currentStatus = currentTicket.Status;
            var newStatus = request.StatusCode;

			// TODO Check the source or destination status is valid for OnHold
			bool IsAllowedStatusesForOnHold() {
				var allowedStatuses = new[] { (int)TicketStatus.Open, (int)TicketStatus.InProgress };
				var onHoldStatus = ticketStatuses.SingleOrDefault(s => _onHoldStatus.Equals(s.Status, StringComparison.InvariantCultureIgnoreCase));
				if (onHoldStatus == null)
				{
					return false;
				}
				if ((onHoldStatus.StatusCode == currentStatus || onHoldStatus.StatusCode == newStatus) &&
						(allowedStatuses.Contains(newStatus) || allowedStatuses.Contains(currentStatus)))
				{
					return true;
				}
				return false;
			}

            if (!AllowedStatusChanges.Any(s => (int)s.FromStatus == currentStatus && (int)s.ToStatus == newStatus) && !IsAllowedStatusesForOnHold())
            {
                throw new ArgumentException($"The ticket status change from '{currentStatus}' to '{newStatus}' is not allowed.");
            }

            UpdateTicketStatusRequest.RepairForm repairForm = null;
            if (currentStatus == (int)TicketStatus.InProgress && newStatus == (int)TicketStatus.LimitedAvailability)
            {
                repairForm = request.InProgress2LimitedAvailability;
            }
            else if (currentStatus == (int)TicketStatus.InProgress && newStatus == (int)TicketStatus.Resolved)
            {
                repairForm = request.InProgress2Resolved;
            }
            else if (currentStatus == (int)TicketStatus.LimitedAvailability && newStatus == (int)TicketStatus.Resolved)
            {
                repairForm = request.LimitedAvailability2Resolved;
            }

            var requestToWorkflowApi = new WorkflowUpdateTicketRequest
            {
                Status = newStatus,
                Cause = repairForm?.Cause,
                Solution = repairForm?.Solution,
                Notes = repairForm?.Notes,
                CategoryId = currentTicket.CategoryId,
                LastUpdatedByExternalSource = false,
                ExternalCreatedDate = currentTicket.ExternalCreatedDate,
                ExternalUpdatedDate = currentTicket.ExternalUpdatedDate,
				SourceId = currentUserId
            };

            if (currentStatus == (int)TicketStatus.Open && newStatus == (int)TicketStatus.InProgress)
            {
                requestToWorkflowApi.AssigneeType = TicketAssigneeType.CustomerUser;
                requestToWorkflowApi.AssigneeId = currentUserId;
            }

            if (currentStatus == (int)TicketStatus.Open && newStatus == (int)TicketStatus.Reassign)
            {
                var form = request.Open2Reassign;
                if (string.IsNullOrEmpty(form?.RejectComment))
                {
                    throw new ArgumentException($"'{nameof(request.Open2Reassign)}' is missed or does not contain the required information");
                }
                requestToWorkflowApi.AssigneeType = TicketAssigneeType.NoAssignee;
                requestToWorkflowApi.AssigneeId = null;
            }

            return requestToWorkflowApi;
        }

        private async Task SendEmailToCreator(Ticket ticket,string currentUserType)
        {
            if (ticket.CreatorId == Guid.Empty)
            {
                return;
            }

            var site = await _siteApi.GetSite(ticket.SiteId);
            var parameters = new 
            {
                TicketSequenceNumber = ticket.SequenceNumber,
                TicketSummary        = ticket.Summary,
                SiteName             = site.Name,
                TicketUrl            = $"{_commandPortalBaseUrl}/dashboard/sites/{ticket.SiteId}/tickets/{ticket.Id}",
            };
            await _notificationService.SendNotificationAsync(new Willow.Notifications.Models.Notification
            {
                CorrelationId = Guid.NewGuid(),
                CommunicationType = CommunicationType.Email,
                CustomerId = ticket.CustomerId,
                Data = parameters.ToDictionary(),
                Tags = null,
                TemplateName = "TicketResolved",
                UserId = ticket.CreatorId,
                UserType = currentUserType

            });

        }

        private async Task EnrichTicket(Ticket ticket)
        {
            ticket.Comments = ticket.Comments.OrderByDescending(x => x.CreatedDate).ToList();
            foreach (var comment in ticket.Comments)
            {
                switch (comment.CreatorType)
                {
                    case CommentCreatorType.CustomerUser:
                        var customerUser = await _userCache.GetCustomerUser(ticket.CustomerId, comment.CreatorId, true);
                        if (customerUser == null)
                        {
                            customerUser = new CustomerUser
                            {
                                Id = comment.CreatorId,
                                FirstName = "Unknown",
                                LastName = string.Empty,
                                Email = string.Empty
                            };
                        }
                        comment.Creator = CommentCreator.FromCustomerUser(customerUser);
                        break;
                    default:
                        throw new ArgumentException($"The comment {comment.Id} in ticket {ticket.Id} has unknown creator type {comment.CreatorType}");
                }
            }
        }

        private async Task EnrichTicketAssignee(Ticket ticket)
        {
            if (ticket.AssigneeType == TicketAssigneeType.NoAssignee || !ticket.AssigneeId.HasValue)
                return;

            switch (ticket.AssigneeType)
            {
                case TicketAssigneeType.CustomerUser:
                    var customerUser = await _userCache.GetCustomerUser(ticket.CustomerId, ticket.AssigneeId.Value, true);
                    if (customerUser == null)
                    {
                        customerUser = new CustomerUser
                        {
                            Id = ticket.AssigneeId.Value,
                            FirstName = "Unknown",
                            LastName = string.Empty,
                            Email = string.Empty
                        };
                    }
                    ticket.Assignee = TicketAssignee.FromCustomerUser(customerUser);
                    break;
                case TicketAssigneeType.WorkGroup:
                    var workGroups = await _workflowApi.GetWorkgroups(ticket.SiteId);
                    var workGroup = workGroups.Single(x => x.Id == ticket.AssigneeId.Value);
                    if (workGroup == null)
                    {
                        workGroup = new Workgroup
                        {
                            Id = ticket.AssigneeId.Value,
                            Name = "Unknown"
                        };
                    }
                    ticket.Assignee = TicketAssignee.FromWorkgroup(workGroup);
                    break;
                default:
                    throw new ArgumentException($"The assignee {ticket.AssigneeId.Value} in ticket {ticket.Id} has unknown assignee type {ticket.AssigneeType}");
            }
        }

        private async Task<bool> IsCustomerAdmin(Guid userId)
        {
            var roleAssignments = await _directoryApi.GetUserRoleAssignments(userId);
            return roleAssignments.Any(ra => ra.ResourceType == RoleResourceType.Customer
                                             && ra.RoleId == WellKnownRoleIds.CustomerAdmin);
        }
    }
}
