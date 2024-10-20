using AutoFixture;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketSubStatusTests : BaseInMemoryTest
    {
        public GetTicketSubStatusTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicketSubStatus_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"ticketsSubStatus");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task SubStatusExists_GetTicketSubStatus_ReturnSubStatus()
        {
            var ticketSubStatus = Fixture.Build<TicketSubStatusEntity>()
                                         .CreateMany(3)
                                        .ToList();
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketSubStatus.AddRange(ticketSubStatus);
                db.SaveChanges();
                var result = await client.GetAsync($"ticketsSubStatus");
                result.StatusCode.Should().Be(HttpStatusCode.OK);
                var response = await result.Content.ReadAsAsync<List<TicketSubStatus>>();
                response.Should().BeEquivalentTo(TicketSubStatusEntity.MapTo(ticketSubStatus));

            }
        }
    }
}
