using AutoFixture;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets;

public class GetTicketCategoryCountBySpaceTwinIdTests : BaseInMemoryTest
{
    public GetTicketCategoryCountBySpaceTwinIdTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetTicketCategoryCountBySpaceTwinId_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("tickets/twins/spaceTwinId/ticketCountsByCategory");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }

    [Fact]
    public async Task TicketCategoryBySpaceTwinIdExists_GetTicketCategoryCountByTwinId_ReturnCategoryCount()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var expectedResponse = Fixture.Build<TicketCategoryCountResponse>().Create();
            server.Arrange().GetWorkflowApi()
                 .SetupRequest(HttpMethod.Get, "tickets/twins/spaceTwinId/ticketCountsByCategory?limit=6")
                 .ReturnsJson(expectedResponse);
            var response = await client.GetAsync("tickets/twins/spaceTwinId/ticketCountsByCategory?limit=6");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualResponse = await response.Content.ReadAsAsync<TicketCategoryCountResponse>();
            actualResponse.Should().BeEquivalentTo(expectedResponse);

        }
    }

    [Fact]
    public async Task TicketCategoryCountByTwinIdExists_GetTicketCategoryCountByTwinIdWithDefaultLimit_ReturnCategoryCount()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var expectedResponse = Fixture.Build<TicketCategoryCountResponse>().Create();
            server.Arrange().GetWorkflowApi()
                 .SetupRequest(HttpMethod.Get, "tickets/twins/twinId/ticketCountsByCategory?limit=5")
                 .ReturnsJson(expectedResponse);
            var response = await client.GetAsync("tickets/twins/twinId/ticketCountsByCategory");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualResponse = await response.Content.ReadAsAsync<TicketCategoryCountResponse>();
            actualResponse.Should().BeEquivalentTo(expectedResponse);

        }
    }
}



