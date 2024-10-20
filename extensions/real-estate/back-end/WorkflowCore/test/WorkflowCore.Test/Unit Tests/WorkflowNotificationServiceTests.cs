using System;
using System.Threading.Tasks;

using Moq;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services;
using WorkflowCore.Models;
using WorkflowCore.Controllers.Request;
using System.Collections.Generic;
using Willow.Notifications.Interfaces;
using Willow.Notifications.Models;
using Xunit;

namespace WorkflowCore.Test.UnitTests
{
    [Trait("Category", "FrequencyUnit")]
    [Trait("Category", "CommSvc")]
    public class WorkflowNotificationServiceTests
    {
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly Mock<INotificationService>                 _notificationService            = new Mock<INotificationService>();
        private readonly Mock<IPushNotificationServer>      _pushNotificationServer = new Mock<IPushNotificationServer>();
        private readonly Mock<IDirectoryApiService>         _directoryApiService    = new Mock<IDirectoryApiService>();
        private readonly Mock<INotificationReceiverService> _receiversService       = new Mock<INotificationReceiverService>();
        private readonly Mock<IWorkgroupService>            _workgroupService       = new Mock<IWorkgroupService>();
        private readonly Guid                               _siteId                 = Guid.NewGuid();   
        private readonly Guid                               _customerId             = Guid.NewGuid();
        private readonly Guid                               _userId                 = Guid.NewGuid();

        public WorkflowNotificationServiceTests()
        {
            _workflowNotificationService = new WorkflowNotificationService(_notificationService.Object, 
                                                           _pushNotificationServer.Object,
                                                           _directoryApiService.Object,
                                                           _receiversService.Object,
                                                           _workgroupService.Object,
                                                           "http://blah",
                                                           "http://foo",
                                                           new List<Guid> {Guid.NewGuid() });

           // _siteRepo.Setup( s=> s.Get(_siteId)).ReturnsAsync(new Site { Id = _siteId, CustomerId = _customerId, Name = "Quarry 1"});
            _directoryApiService.Setup( d=> d.GetUserPreferences(_customerId, _userId)).ReturnsAsync(new UserPreferences { Language = "en", MobileNotificationEnabled = true });
        }

        [Fact]
        public async Task NotificationService_NotifyAssignees_TicketAssigned()
        {
            var ticketId = Guid.NewGuid();

            await _workflowNotificationService.NotifyAssignees
            (
                new UpdateTicketRequest
                {
                    CustomerId   = _customerId,
                    AssigneeType = AssigneeType.CustomerUser,
                    AssigneeId   = _userId,
                    Status       = (int)TicketStatusEnum.InProgress
                },
                new Ticket
                { 
                    Id           = ticketId,
                    SiteId       = _siteId,
                    CustomerId   = _customerId,
                    AssigneeType = AssigneeType.CustomerUser,
                    AssigneeId   = _userId,
                    Status       = (int)TicketStatusEnum.Open
                },
                "bob",
                "en"
            );

            _notificationService.Verify( e=> e.SendNotificationAsync(It.IsAny<Notification>()), Times.Once);
            _pushNotificationServer.Verify( e=> e.SendNotification(It.IsAny<Guid>(), _customerId, _userId, "TicketUpdated", "en", It.IsAny<object>()), Times.Once);
        }
    }
}
