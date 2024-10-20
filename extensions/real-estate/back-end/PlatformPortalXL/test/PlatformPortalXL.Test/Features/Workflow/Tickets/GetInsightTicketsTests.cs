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
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetInsightTicketsTests : BaseInMemoryTest
    {
        public GetInsightTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TicketsAreAssociatedToAsset_GetInsightTickets_ReturnsThoseTickets()
        {
            var userId = Guid.NewGuid();
            var siteId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Id, siteId)
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var insightId = Guid.NewGuid();
            var expectedTickets = Fixture.Build<Ticket>()
	            .With(x => x.InsightId, insightId).CreateMany(10).ToList();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets?insightId={insightId}")
                    .ReturnsJson(expectedTickets);

				var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/tickets");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(expectedTickets));
            }
        }
        [Fact]
        public async Task GetInsightTickets_ByScopeId_ReturnsThoseTickets()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();
            var expectedTickets = Fixture.Build<Ticket>()
                .With(x => x.InsightId, insightId).CreateMany(10).ToList();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = userSites[0].Id;
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{expectedTwinDto[0].SiteId}/tickets?insightId={insightId}")
                    .ReturnsJson(expectedTickets);

                var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/tickets?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(expectedTickets));
            }
        }

        [Fact]
        public async Task GetInsightTickets_ByScopeId_UserHasNoAccess_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/tickets?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetInsightTickets_ByInvalidScopeId_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var insightId = Guid.NewGuid();
            var scopeId = Guid.NewGuid().ToString();
            var userId = Guid.NewGuid();

            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures() { IsTicketingDisabled = false })
                .CreateMany(2).ToList();

            var expectedTwinDto = Fixture.Build<TwinDto>().CreateMany(2).ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient(null, userId))
            {
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);

                var response = await client.GetAsync($"sites/{siteId}/insights/{insightId}/tickets?scopeId={scopeId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetInsightTickets_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/insights/{Guid.NewGuid()}/tickets");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}
