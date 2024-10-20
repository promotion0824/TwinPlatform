using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class GetTicketCategoryTests : BaseInMemoryTest
    {
        public GetTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTicketCategory_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_GetTicketCategory_ReturnsThisTicketCategory()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            var expectedTicketCategory = Fixture.Create<TicketCategory>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/categories/{ticketCategoryId}")
                    .ReturnsJson(expectedTicketCategory);

                var response = await client.GetAsync($"sites/{siteId}/tickets/categories/{ticketCategoryId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketCategoryDto>();
                result.Should().BeEquivalentTo(TicketCategoryDto.MapFrom(expectedTicketCategory));
            }
        }
    }
}