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

public class GetTicketStatusTransitionsTests : BaseInMemoryTest
{
    public GetTicketStatusTransitionsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UnauthorizedUser_GetTicketStatusTransitions_ReturnUnauthorized()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync("tickets/statusTransitions");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        }
    }

    [Fact]
    public async Task TicketStatusTransitionExists_GetTicketStatusTransitions_ReturnTheList()
    {
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            var expectedResponse = Fixture.Build<TicketStatusTransitionsResponse>().Create();
            server.Arrange().GetWorkflowApi()
                 .SetupRequest(HttpMethod.Get, "tickets/statusTransitions")
                 .ReturnsJson(expectedResponse);
            var response = await client.GetAsync("tickets/statusTransitions");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var actualResponse = await response.Content.ReadAsAsync<TicketStatusTransitionsResponse>();
            actualResponse.Should().BeEquivalentTo(expectedResponse);

        }
    }
}

