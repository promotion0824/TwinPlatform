using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Platform.Models;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetTicketsTests : BaseInMemoryTest
    {
        public GetTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData("open", "statuses=0&statuses=5&statuses=10&statuses=15", TicketStatus.Open)]
        [InlineData("resolved", "statuses=20", TicketStatus.Resolved)]
        [InlineData("closed", "statuses=30", TicketStatus.Closed)]
        public async Task SitesHaveTickets_ReturnsTickets(string tab, string queryString, TicketStatus status)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                                       .CreateMany();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedTickets = new Dictionary<Guid, List<Ticket>>();

			foreach (var site in expectedSites)
            {
               var tickets = Fixture.Build<Ticket>()
                    .With(x => x.SiteId, site.Id)
                    .With(x => x.CustomerId, customerId)
                    .With(x => x.Status, (int)status)
                    .With(x => x.SourceType, TicketSourceType.Platform)
                    .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                    .With(x => x.Comments, new List<Comment>())
                    .Without(x=>x.TwinId)
                    .CreateMany()
                    .ToList();

               expectedTickets[site.Id]=tickets;

			}
            var ticketStatuses = new List<CustomerTicketStatus>() {
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 0).With(x => x.Status, "Open").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 5).With(x => x.Status, "Reassign").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 10).With(x => x.Status, "InProgress").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 15).With(x => x.Status, "LimitedAvailability").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 20).With(x => x.Status, "Resolved").With(x => x.Tab, "Resolved").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 30).With(x => x.Status, "Closed").With(x => x.Tab, "Closed").Create(),
                                };


			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                workflowApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/ticketstatus")
                    .ReturnsJson(ticketStatuses);
                foreach (var site in expectedSites)
                {
                    workflowApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/tickets?{queryString}")
                        .ReturnsJson(expectedTickets[site.Id]);
                }

                var response = await client.GetAsync($"tickets?tab={tab}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();

				var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets.Values.SelectMany(l => l).ToList());
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Theory]
        [InlineData("open", "statuses=0&statuses=5&statuses=10&statuses=15", TicketStatus.Open)]
        [InlineData("resolved", "statuses=20", TicketStatus.Resolved)]
        [InlineData("closed", "statuses=30", TicketStatus.Closed)]
        public async Task SitesHaveTickets_ByScopeId_ReturnsTickets(string tab, string queryString, TicketStatus status)
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                                       .CreateMany(10).ToList();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();
            expectedTwinDto[0].SiteId = expectedSites[0].Id;
            var expectedTickets = Fixture.Build<Ticket>()
                .With(x => x.SiteId, expectedTwinDto[0].SiteId)
                .With(x => x.CustomerId, customerId)
                .With(x => x.Status, (int)status)
                .With(x => x.SourceType, TicketSourceType.Platform)
                .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                .With(x => x.Comments, new List<Comment>())
                .Without(x => x.TwinId)
                .CreateMany()
                .ToList();


            var ticketStatuses = new List<CustomerTicketStatus>() {
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 0).With(x => x.Status, "Open").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 5).With(x => x.Status, "Reassign").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 10).With(x => x.Status, "InProgress").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 15).With(x => x.Status, "LimitedAvailability").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 20).With(x => x.Status, "Resolved").With(x => x.Tab, "Resolved").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 30).With(x => x.Status, "Closed").With(x => x.Tab, "Closed").Create(),
                                };



            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                workflowApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/ticketstatus")
                    .ReturnsJson(ticketStatuses);

               workflowApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/tickets?{queryString}")
                        .ReturnsJson(expectedTickets);

                var response = await client.GetAsync($"tickets?tab={tab}&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();

                var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets.ToList());
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task SitesHaveTickets_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                                       .CreateMany(10).ToList();


            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();



            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);


                var response = await client.GetAsync($"tickets?tab=open&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }


        [Fact]
        public async Task SitesHaveTickets_ByInvalidScopeId_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var expectedSites = Fixture.Build<Site>()
                .With(x => x.CustomerId, customerId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(10).ToList();


            var expectedTwinDto = new List<TwinDto>();



            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);


                var response = await client.GetAsync($"tickets?tab=open&scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Fact]
        public async Task SitesHaveTickets_ReturnsTickets_wComments()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var expectedSites = Fixture.Build<Site>()
                                       .With(x => x.CustomerId, customerId)
                                       .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                                       .CreateMany();

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            var expectedTickets = new Dictionary<Guid, List<Ticket>>();
            foreach (var site in expectedSites)
            {
                expectedTickets[site.Id] = Fixture.Build<Ticket>()
                    .With(x => x.SiteId, site.Id)
                    .With(x => x.CustomerId, customerId)
                    .With(x => x.Status, (int)TicketStatus.Open)
                    .With(x => x.SourceType, TicketSourceType.Platform)
                    .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                    .With(x => x.AssigneeId, userId)
                    .With(x => x.AssigneeType, TicketAssigneeType.WorkGroup)
                    .With(x => x.AssigneeName, "")
                    .With(x => x.Comments, new List<Comment>())
                    .Without(x=>x.InsightId)
                    .Without(x=>x.TwinId)
                    .CreateMany()
                    .ToList();
            }
            var ticketStatuses = new List<CustomerTicketStatus>() {
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 0).With(x => x.Status, "Open").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 5).With(x => x.Status, "Reassign").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 10).With(x => x.Status, "InProgress").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 15).With(x => x.Status, "LimitedAvailability").With(x => x.Tab, "Open").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 20).With(x => x.Status, "Resolved").With(x => x.Tab, "Resolved").Create(),
                                    Fixture.Build<CustomerTicketStatus>().With(x => x.StatusCode, 30).With(x => x.Status, "Closed").With(x => x.Tab, "Closed").Create(),
                                };

            var expectedResult = TicketSimpleDto.MapFromModels(expectedTickets.Values.SelectMany(l => l).ToList());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var directoryApiHandler = server.Arrange().GetDirectoryApi();
                var workflowApiHandler = server.Arrange().GetWorkflowApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                workflowApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/ticketstatus")
                    .ReturnsJson(ticketStatuses);

                foreach (var site in expectedSites)
                {
                    workflowApiHandler
                        .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/tickets?statuses=0&statuses=5&statuses=10&statuses=15")
                        .ReturnsJson(expectedTickets[site.Id]);
                }

                var response = await client.GetAsync($"tickets?tab=open");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermissionForSites_ReturnsEmptyList()
        {
            var userId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var expectedSites = new List<Site>{};

            var expectedUser = Fixture.Build<User>()
                .With(x => x.Id, userId)
                .With(x => x.CustomerId, customerId)
                .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                var workflowApiHandler = server.Arrange().GetWorkflowApi();
                var directoryApiHandler = server.Arrange().GetDirectoryApi();

                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(expectedSites);
                directoryApiHandler
                    .SetupRequest(HttpMethod.Get, $"users/{userId}")
                    .ReturnsJson(expectedUser);
                workflowApiHandler
                    .SetupRequest(HttpMethod.Get, $"customers/{expectedUser.CustomerId}/ticketstatus")
                    .ReturnsJson(new List<CustomerTicketStatus>());

                var response = await client.GetAsync($"tickets?tab=open");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }
    }
}
