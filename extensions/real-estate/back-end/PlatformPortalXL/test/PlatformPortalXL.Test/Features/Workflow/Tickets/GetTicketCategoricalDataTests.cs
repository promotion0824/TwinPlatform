using AutoFixture;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Platform.Users;
using Willow.Tests.Infrastructure;
using Willow.Workflow.Models;
using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets;

public class GetTicketCategoricalDataTests : BaseInMemoryTest
{
    public GetTicketCategoricalDataTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async Task UserDoesNotHavePermission_GetTicketCategoricalData_ReturnsUnauthorized()
    {

        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient())
        {
            var response = await client.GetAsync($"tickets/ticketCategoricalData");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketCategoricalDataExists_GetTicketCategoricalData_ReturnsTicketCategoricalData()
    {
        var expectedResult = Fixture.Create<TicketCategoricalData>();
        var user = Fixture.Create<User>();
        using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
        using (var client = server.CreateClient(null, user.Id))
        {

            server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{user.Id}")
                    .ReturnsJson(user);
            server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"api/mapped/categoricalData")
                    .ReturnsJson(expectedResult);
            var response = await client.GetAsync($"tickets/ticketCategoricalData");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<TicketCategoricalData>();
            result.JobTypes.Should().BeEquivalentTo(expectedResult.JobTypes);
            foreach (var item in result.ServicesNeeded)
            {
                var servicesNeeded = result.ServicesNeeded.FirstOrDefault(x => x.SpaceTwinId == item.SpaceTwinId);
                servicesNeeded.Should().BeEquivalentTo(item);

            }
        }
    }
}

