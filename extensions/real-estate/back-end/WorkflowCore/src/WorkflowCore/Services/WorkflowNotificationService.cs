using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Directory.Models;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Services
{
    public interface IWorkflowNotificationService
    {
        Task NotifyAssignees(Ticket ticket, string siteName, string language, bool isNewTicket = true, bool reAssigned = false);
        Task NotifyAssignees(UpdateTicketRequest updateTicketRequest, Ticket originalTicket, string siteName, string language);
    }

    public class WorkflowNotificationService : IWorkflowNotificationService
    {
        private readonly INotificationService _notificationService;
        private readonly IPushNotificationServer _pushNotificationServer;
        private readonly IDirectoryApiService _directoryApiService;
        private readonly INotificationReceiverService _receiversService;
        private readonly string _commandPortalBaseUrl;
        private readonly string _mobilePortalBaseUrl;
        private readonly IWorkgroupService _workgroupService;
        private readonly List<Guid> _notificationConfigurations;

        public WorkflowNotificationService(INotificationService notificationService, 
                                   IPushNotificationServer pushNotificationServer, 
                                   IDirectoryApiService directoryApiService, 
                                   INotificationReceiverService receiversService, 
                                   IConfiguration configuration, 
                                   IWorkgroupService workgroupService)
                            : this(notificationService,
                                   pushNotificationServer,
                                   directoryApiService,
                                   receiversService,
                                   workgroupService,
                                   configuration.GetValue<string>("CommandPortalBaseUrl"),
                                   configuration.GetValue<string>("MobilePortalBaseUrl"),
                                   configuration.GetSection("NotificationExcludedCustomers").Get<List<Guid>>())
        {
        }

        public WorkflowNotificationService(INotificationService notificationService, 
                                   IPushNotificationServer pushNotificationServer, 
                                   IDirectoryApiService directoryApiService, 
                                   INotificationReceiverService receiversService, 
                                   IWorkgroupService workgroupService, 
                                   string commandPortalBaseUrl, 
                                   string mobilePortalBaseUrl,
                                   List<Guid> notificationExcludedCustomers)
        {
            _notificationService            = notificationService            ?? throw new ArgumentNullException(nameof(notificationService));
            _pushNotificationServer = pushNotificationServer ?? throw new ArgumentNullException(nameof(pushNotificationServer));
            _directoryApiService    = directoryApiService    ?? throw new ArgumentNullException(nameof(directoryApiService));
            _receiversService       = receiversService       ?? throw new ArgumentNullException(nameof(receiversService));
            _workgroupService       = workgroupService       ?? throw new ArgumentNullException(nameof(workgroupService));             
            _commandPortalBaseUrl   = commandPortalBaseUrl;
            _mobilePortalBaseUrl    = mobilePortalBaseUrl;
            _notificationConfigurations = notificationExcludedCustomers ?? new List<Guid>();
        }

        public async Task NotifyAssignees(Ticket ticket, string siteName, string language, bool isNewTicket = true, bool reAssigned = false)
        {
            if (_notificationConfigurations.Contains(ticket.CustomerId))
            {
                return;
            }
               
            var userIds = await GetAssigneeIds(ticket);
            var correlationId = Guid.NewGuid();
           
            await Task.WhenAll( SendEmail(correlationId, siteName, ticket, userIds, language, isNewTicket, reAssigned),
                                SendPushNotification(correlationId, siteName, ticket, userIds, language, isNewTicket, reAssigned));
        }

        public async Task NotifyAssignees(UpdateTicketRequest updateTicketRequest, 
                                          Ticket originalTicket, 
                                          string siteName, 
                                          string language)
        {
            if (_notificationConfigurations.Contains(updateTicketRequest.CustomerId))
            {
                return;
            }
            if (!updateTicketRequest.AssigneeType.HasValue)
                return;

            var ticketReassigned = originalTicket.AssigneeId != updateTicketRequest.AssigneeId || originalTicket.AssigneeType != updateTicketRequest.AssigneeType;
            var ticketChangedStatus = updateTicketRequest.Status != originalTicket.Status;

            if (!ticketReassigned && !ticketChangedStatus)
                return;

            await NotifyAssignees(new Ticket
            {
                SiteId = originalTicket.SiteId,
                CustomerId = originalTicket.CustomerId,
                SequenceNumber = originalTicket.SequenceNumber,
                Summary = updateTicketRequest.Summary,
                Id = originalTicket.Id,
                AssigneeId = updateTicketRequest.AssigneeId,
                AssigneeType = updateTicketRequest.AssigneeType.Value,
                TemplateId = originalTicket.TemplateId
            }, siteName, language, false, ticketReassigned);
        }

        private Task SendPushNotification(Guid correlationId, string siteName, Ticket ticket, List<Guid> userIds, string language, bool isNewTicket = true, bool reAssigned = false)
        {
            if (_notificationConfigurations.Contains(ticket.CustomerId))
            {
                return Task.CompletedTask;
            }
            var parameters = new
            {
                TicketSequenceNumber = ticket.SequenceNumber,
                TicketSummary = ticket.Summary,
                SiteName = siteName,
                TicketUrl = GetTicketUrl(ticket),
            };

            var template = GetTemplate(ticket.AssigneeType, isNewTicket, reAssigned);
            var tasks   = new List<Task>();

            foreach(var userId in userIds)
                tasks.Add(SendPushNotification(correlationId, ticket.CustomerId, userId, template, language, parameters));

            return Task.WhenAll(tasks); 
        }

        #region Private

        private string GetTemplate(AssigneeType assigneeType, bool isNewTicket, bool reAssigned)
        {                              
            return isNewTicket ? (assigneeType == AssigneeType.NoAssignee ? CommSvc.Templates.Email.Tickets.Created 
                                                                          : CommSvc.Templates.Email.Tickets.Assigned)
                               : (reAssigned ? CommSvc.Templates.Email.Tickets.Reassigned 
                                             : CommSvc.Templates.Email.Tickets.Updated);
        }

        public async Task SendPushNotification(Guid correlationId, Guid customerId, Guid userId, string template, string language, object data)
        {
            var userPreferences = await _directoryApiService.GetUserPreferences(customerId, userId);

            if(userPreferences.MobileNotificationEnabled)
            {
                await _pushNotificationServer.SendNotification(correlationId, customerId, userId, template, language, data);
            }
        }

        private Task SendEmail(Guid correlationId, string siteName, Ticket ticket, List<Guid> userIds, string language, bool isNewTicket = true, bool reAssigned = false)
        {
            var parameters = new
            {
                TicketSequenceNumber = ticket.SequenceNumber,
                TicketSummary        = ticket.Summary,
                SiteName             = siteName,
                TicketUrl            = GetTicketUrl(ticket),
            };

            var tasks    = new List<Task>();
            var template = GetTemplate(ticket.AssigneeType, isNewTicket, reAssigned);

            foreach(var userId in userIds)
                tasks.Add(SendEmail(correlationId, ticket.CustomerId, userId, template, language, parameters));

            return Task.WhenAll(tasks); 
         }

        private Task SendEmail(Guid correlationId, Guid customerId, Guid userId, string template, string language, object parameters)
        {
            return _notificationService.SendNotificationAsync(new Notification
            {
                CorrelationId = correlationId,
                CommunicationType = CommunicationType.Email,
                CustomerId = customerId,
                Data = parameters.ToDictionary(),
                Tags = null,
                TemplateName = template,
                UserId = userId
            });
        }
        
        private string GetTicketUrl(Ticket ticket)
        {
            if (ticket.AssigneeType == AssigneeType.NoAssignee) 
                return ticket.TemplateId.HasValue 
                    ? $"{_commandPortalBaseUrl}/sites/{ticket.SiteId}/tickets/scheduled-tickets/{ticket.Id}" 
                    : $"{_commandPortalBaseUrl}/sites/{ticket.SiteId}/tickets/{ticket.Id}";

            return ticket.TemplateId.HasValue 
                ? $"{_mobilePortalBaseUrl}/sites/{ticket.SiteId}/scheduled-tickets/view/{ticket.Id}" 
                : $"{_mobilePortalBaseUrl}/tickets/sites/{ticket.SiteId}/view/{ticket.Id}";
        }

        private async Task<List<Guid>> GetAssigneeIds(Ticket ticket)
        {
            if (ticket.AssigneeType == AssigneeType.NoAssignee)
            {
                var notificationReceivers = await _receiversService.GetReceivers(ticket.SiteId);
                return notificationReceivers.Select(x => x.UserId).ToList();
            }

            if(!ticket.AssigneeId.HasValue)
            { 
                return new List<Guid>();
            }

            if (ticket.AssigneeType == AssigneeType.WorkGroup)
            {
                var workGroup = await _workgroupService.GetWorkgroup(ticket.SiteId, ticket.AssigneeId.Value, true);
                return workGroup.MemberIds;
            }

            return new List<Guid> { ticket.AssigneeId.Value };
        }

        #endregion
    }
}
