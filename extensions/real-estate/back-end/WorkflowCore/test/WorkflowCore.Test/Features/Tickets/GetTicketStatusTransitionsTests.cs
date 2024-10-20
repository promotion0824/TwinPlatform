using AutoFixture;
using FluentAssertions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets;

public class GetTicketStatusTransitionsTests : BaseInMemoryTest
{
    public GetTicketStatusTransitionsTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task TokenIsNotGiven_GetTicketStatusTransitions_RequiresAuthorization()
    {
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient())
        {
            var result = await client.GetAsync($"tickets/statusTransitions");
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [Fact]
    public async Task TicketStatusTransitionExists_GetTicketStatusTransitions_ReturnTheList()
    {
        var ticketStatusTransitions = Fixture.CreateMany<TicketStatusTransitionsEntity>(3).ToList();
        var expectedResults = new TicketStatusTransitionsDto();
        expectedResults.TicketStatusTransitionList.AddRange(ticketStatusTransitions.Select(x => new TicketStatusTransition(x.FromStatus, x.ToStatus)));
        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.TicketStatusTransitions.AddRange(ticketStatusTransitions);
            db.SaveChanges();
            var response = await client.GetAsync($"tickets/statusTransitions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<TicketStatusTransitionsDto>();
            result.Should().BeEquivalentTo(expectedResults);

        }
    }

    [Fact]
    public async Task TicketStatusTransitionNOTExists_GetTicketStatusTransitions_ReturnEmptyList()
    {

        await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var response = await client.GetAsync($"tickets/statusTransitions");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<TicketStatusTransitionsDto>();
            result.TicketStatusTransitionList.Should().BeEmpty();

        }
    }
}

