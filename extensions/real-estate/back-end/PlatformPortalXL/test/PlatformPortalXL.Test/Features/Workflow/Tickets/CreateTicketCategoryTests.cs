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
using PlatformPortalXL.Features.Workflow;
using System.Net.Http.Json;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class CreateTicketCategoryTests : BaseInMemoryTest
    {
        public CreateTicketCategoryTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateTicketCategory_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/categories", new CreateTicketCategoryRequest { });

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketCategoryExists_CreateTicketCategory_ReturnsThisTicketCategory()
        {
            var siteId = Guid.NewGuid();
            var ticketCategoryId = Guid.NewGuid();
            var expectedTicketCategory = Fixture.Create<TicketCategory>();
            var request = Fixture.Create<CreateTicketCategoryRequest>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets/categories")
                    .ReturnsJson(expectedTicketCategory);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/categories", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketCategoryDto>();
                result.Should().BeEquivalentTo(TicketCategoryDto.MapFrom(expectedTicketCategory));
            }
        }
    }
}