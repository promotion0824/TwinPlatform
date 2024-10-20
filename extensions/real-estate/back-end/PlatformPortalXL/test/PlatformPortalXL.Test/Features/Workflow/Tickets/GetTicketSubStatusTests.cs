using AutoFixture;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets;

public class GetTicketSubStatusTests : BaseInMemoryTest
{
    public GetTicketSubStatusTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task UserDoesNotHavePermission_GetTicketSubStatus_ReturnsUnauthorized()
    {

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync($"tickets/subStatus");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketCategoricalDataExists_GetTicketCategoricalData_ReturnsTicketCategoricalData()
    {
        var expectedResult = Fixture.CreateMany<TicketSubStatus>().ToList();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null))
        {
            server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, "ticketsSubStatus")
                    .ReturnsJson(expectedResult);
            var response = await client.GetAsync("tickets/subStatus");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<TicketSubStatus>>();
            result.Should().BeEquivalentTo(expectedResult);
           
        }
    }

}

