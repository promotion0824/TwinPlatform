using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using Moq;
using Willow.Api.Client;
using Willow.Data;
using Willow.Platform.Users;
using Willow.Workflow;
using Willow.Data.Rest;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class TicketServiceTests
    {
        private readonly TicketService                      _service;
        private readonly Mock<IWorkflowApiService>          _workflowApi                    = new Mock<IWorkflowApiService>();
        private readonly Mock<IDigitalTwinApiService>       _digitalTwinApiService          = new Mock<IDigitalTwinApiService>();
        private readonly Mock<IUserService>                 _userService                    = new Mock<IUserService>();
        private readonly Mock<ILogger<TicketService>>       _logger                         = new Mock<ILogger<TicketService>>();
        private readonly Guid                               _siteId                         = Guid.NewGuid();
        private readonly Mock<ISiteApiService>              _siteApi                        = new Mock<ISiteApiService>();
        private readonly Mock<IDirectoryApiService> _directoryApiService = new Mock<IDirectoryApiService>();
		public TicketServiceTests()
        {
            // Valid request
            _service = new TicketService(
                _workflowApi.Object,
                _userService.Object,
                _logger.Object,
                _siteApi.Object,
                _directoryApiService.Object,
                _digitalTwinApiService.Object);
        }

		[Fact]
		public async Task TicketService_CreateTicket_success()
		{
			var request = new WorkflowCreateTicketRequest
			{
				InsightId = Guid.NewGuid(),
				AssigneeType = TicketAssigneeType.CustomerUser
			};

			_workflowApi.Setup(w => w.CreateTicket(_siteId, request)).ReturnsAsync(new Ticket());

			var ticket = await _service.CreateTicket(_siteId, request, null);

			Assert.NotNull(ticket);

			_workflowApi.Verify(w => w.CreateTicket(_siteId, request), Times.Once);
			
		}

		[Fact]
        public async Task TicketService_EnrichTicket_success()
        {
			var userId = Guid.NewGuid();
            var ticket = new Ticket
            {
				InsightId = Guid.NewGuid(),
                AssigneeId = userId,
                AssigneeType = TicketAssigneeType.CustomerUser
            };

            _userService.Setup( u=> u.GetUser(_siteId, userId, It.IsAny<UserType>())).ReturnsAsync(new User { FirstName = "Fred", LastName = "Flintstone" } );

            await _service.EnrichTicket(ticket, _siteId);

            Assert.NotNull(ticket.Assignee);
            Assert.Equal("Fred Flintstone", ticket.Assignee.Name);
            Assert.Equal(TicketAssigneeType.CustomerUser, ticket.Assignee.Type);
        }

        [Fact]
        public async Task TicketService_EnrichTicket_UserService_success()
        {
            var userId = Guid.NewGuid();
            var ticket = new Ticket
            {
	            InsightId = Guid.NewGuid(),
				Id = Guid.NewGuid(),
                AssigneeId = userId,
                AssigneeType = TicketAssigneeType.CustomerUser
            };

            var directoryApi    = new Mock<IRestApi>();
            var userRepo        = new Mock<IReadRepository<Guid, User>>();
            var workgroupRepo   = new Mock<IReadRepository<SiteObjectIdentifier, Workgroup>>();

			userRepo.Setup( d=> d.Get(userId)).ReturnsAsync(new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" } );
            userRepo.Setup( d=> d.Get(It.Is<IEnumerable<Guid>>( e=> e.Any( id=> id == userId)))).Returns(GetAsyncList(new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" }));

            var userSvc = new UserService(userRepo.Object, workgroupRepo.Object, directoryApi.Object);
            var svc = new TicketService(_workflowApi.Object, userSvc, _logger.Object, _siteApi.Object,_directoryApiService.Object, _digitalTwinApiService.Object);

            await svc.EnrichTicket(ticket, _siteId);

            Assert.NotNull(ticket.Assignee);
            Assert.Equal("Fred Flintstone", ticket.Assignee.Name);
            Assert.Equal(TicketAssigneeType.CustomerUser, ticket.Assignee.Type);
        }

        [Fact]
        public async Task TicketService_EnrichTicket_workgroup_assignee_success()
        {
            var workgroupId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var ticket = new Ticket
            {
	            InsightId = Guid.NewGuid(),
				Id = Guid.NewGuid(),
                AssigneeId = workgroupId,
                AssigneeType = TicketAssigneeType.WorkGroup,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        CreatorId = userId,
                        CreatorType = CommentCreatorType.CustomerUser
                    }
                }
            };
            
            var directoryApi    = new Mock<IRestApi>();
            var userRepo        = new Mock<IReadRepository<Guid, User>>();
            var workflowApi     = new Mock<IRestApi>();
            var cache           = new MemoryCache(new MemoryCacheOptions());
            userRepo.Setup( d=> d.Get(userId)).ReturnsAsync(new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" } );
            userRepo.Setup( d=> d.Get(It.Is<IEnumerable<Guid>>( e=> e.Any( id=> id == userId)))).Returns(GetAsyncList(new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" }));

            var workgroupRepo = new CachedRepository<SiteObjectIdentifier, Workgroup>( new RestRepositoryReader<SiteObjectIdentifier, Workgroup>( workflowApi.Object,  (SiteObjectIdentifier id)=> $"sites/{id.SiteId}/workgroups/{id.Id}", null), cache, TimeSpan.FromHours(1), "Workgroup_");

            workflowApi.Setup( d=> d.Get<Workgroup>($"sites/{_siteId}/workgroups/{workgroupId}", null)).ReturnsAsync(new Workgroup { Id = workgroupId, Name = "The Flintstones" });

            var userSvc = new UserService(userRepo.Object, workgroupRepo, directoryApi.Object);
            var svc = new TicketService(_workflowApi.Object, userSvc, _logger.Object, _siteApi.Object, _directoryApiService.Object, _digitalTwinApiService.Object);

            await svc.EnrichTicket(ticket, _siteId);

            Assert.NotNull(ticket.Assignee);
            Assert.Equal("The Flintstones", ticket.Assignee.Name);
            Assert.Equal(TicketAssigneeType.WorkGroup, ticket.Assignee.Type);
            Assert.Equal("Fred", ticket.Comments[0].Creator.FirstName);
            Assert.Equal("Flintstone", ticket.Comments[0].Creator.LastName);
        }

        [Fact]
        public async Task TicketService_EnrichTicket_wComments_success()
        {
            var userId = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var ticket = new Ticket
            {
	            InsightId = Guid.NewGuid(),
				AssigneeId = userId,
                AssigneeType = TicketAssigneeType.CustomerUser,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        CreatorId = userId
                    },
                    new Comment
                    {
                        CreatorId = userId2
                    }
                }
            };

            _userService.Setup( u=> u.GetUser(_siteId, userId, UserType.Customer)).ReturnsAsync(new User { FirstName = "Fred", LastName = "Flintstone" } );
            _userService.Setup( u=> u.GetUser(_siteId, userId2, UserType.Customer)).ReturnsAsync(new User { FirstName = "Barney", LastName = "Rubble" } );
            _userService.Setup(u => u.GetUsers(_siteId, It.IsAny<IEnumerable<(Guid, UserType)>>(), UserType.All))
                .ReturnsAsync(new List<IUser> { new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" },
                                                new User { Id = userId2, FirstName = "Barney", LastName = "Rubble" } });

            _directoryApiService.Setup(u => u.GetUser(It.IsAny<Guid>(), false)).ReturnsAsync(new User { FirstName = "Fred", LastName = "Flintstone" });

            await _service.EnrichTicket(ticket, _siteId);

            Assert.NotNull(ticket.Assignee);
            Assert.Equal("Fred Flintstone", ticket.Assignee.Name);
            Assert.Equal(TicketAssigneeType.CustomerUser, ticket.Assignee.Type);
            Assert.Equal("Barney", ticket.Comments[1].Creator.FirstName);
            Assert.Equal(CommentCreatorType.CustomerUser, ticket.Comments[1].Creator.Type);
        }

		[Fact]
		public async Task TicketService_EnrichTicket_withoutInsightId_success()
		{
			
			var userId = Guid.NewGuid();
			var userId2 = Guid.NewGuid();
			var ticket = new Ticket
			{
				AssigneeId = userId,
				AssigneeType = TicketAssigneeType.CustomerUser,
				Comments = new List<Comment>
				{
					new Comment
					{
						CreatorId = userId
					},
					new Comment
					{
						CreatorId = userId2
					}
				}
			};

			_userService.Setup(u => u.GetUser(_siteId, userId, UserType.Customer)).ReturnsAsync(new User { FirstName = "Fred", LastName = "Flintstone" });
			_userService.Setup(u => u.GetUser(_siteId, userId2, UserType.Customer)).ReturnsAsync(new User { FirstName = "Barney", LastName = "Rubble" });

            _userService.Setup(u => u.GetUsers(_siteId, It.IsAny<IEnumerable<(Guid, UserType)>>(), UserType.All))
               .ReturnsAsync(new List<IUser> { new User { Id = userId, FirstName = "Fred", LastName = "Flintstone" },
                                                new User { Id = userId2, FirstName = "Barney", LastName = "Rubble" } });

            _directoryApiService.Setup(u => u.GetUser(It.IsAny<Guid>(), false)).ReturnsAsync(new User { FirstName = "Fred", LastName = "Flintstone" });
            await _service.EnrichTicket(ticket, _siteId);

			Assert.NotNull(ticket.Assignee);
			Assert.Equal(ticket.TwinId, null);
			Assert.Equal("Fred Flintstone", ticket.Assignee.Name);
			Assert.Equal(TicketAssigneeType.CustomerUser, ticket.Assignee.Type);
			Assert.Equal("Barney", ticket.Comments[1].Creator.FirstName);
			Assert.Equal(CommentCreatorType.CustomerUser, ticket.Comments[1].Creator.Type);
		}
		private async IAsyncEnumerable<T> GetAsyncList<T>(T item)
        {
            yield return await Task.FromResult(item);
        }


    }
}
