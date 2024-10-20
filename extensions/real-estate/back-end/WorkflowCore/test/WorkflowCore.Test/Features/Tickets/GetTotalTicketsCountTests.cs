using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTotalTicketsCountTests : BaseInMemoryTest
    {
        public GetTotalTicketsCountTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTotalTicketsCount_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/count");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketNotExists_GetTotalTicketsCount_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/count");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<int>();
                result.Should().Be(0);
            }
        }

        [Fact]
        public async Task TicketsExist_GetTotalTicketsCount_ReturnsTotalTicketsCount()
        {
            var siteId = Guid.NewGuid();
            var status = (int)Fixture.Create<TicketStatusEnum>();
            var ticketEntitiesWithGivenStatus = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Status, status)
                                                       .With(x => x.Occurrence, 0)
                                                       .CreateMany(10);
            var ticketEntitiesForOtherStatus = Fixture.Build<TicketEntity>()
                                                      .Without(x => x.Attachments)
                                                      .Without(x => x.Comments)
                                                      .Without(x => x.Category)
                                                      .Without(x => x.Tasks)
                                                      .Without(x => x.JobType)
                                                      .Without(x => x.Diagnostics)
                                                      .With(x => x.SiteId, siteId)
                                                      .With(x => x.Occurrence, 0)
                                                      .CreateMany(10)
                                                      .Where(x => x.Status != status);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesWithGivenStatus);
                db.Tickets.AddRange(ticketEntitiesForOtherStatus);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/tickets/count?statuses={status}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<int>();
                result.Should().Be(10);
            }
        }
    }
}
