using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AutoFixture;
using FluentAssertions;

using Xunit;
using Xunit.Abstractions;

using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;


namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTwinTicketStatisticsByStatusTests : BaseInMemoryTest
    {
        public GetTwinTicketStatisticsByStatusTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTwinTicketStatisticsByStatus_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsync("tickets/twins/statistics/status", null);
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }
        const int AllOpenTickets = 34;// = Open 10 + Reassign 9 + InProgress = 8 + LimitedAvailability = 7 
        const int CountOpen = 10;
        const int CountReassign = 9;
        const int CountInProgress = 8;
        const int CountLimitedAvailability = 7;
        const int CountResolved = 6;
        const int CountClosed = 5;

        [Fact]
        public async Task SingleSiteExist_GetTwinTicketStatisticsByStatus_ReturnsCountOfTicketsByStatusForTheTwin()
        {

            var twinId = "twin1";
            var twinIds = new List<string> { twinId };
            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.LimitedAvailability, Tab = TicketTabs.OPEN, Status = "LimitedAvailability" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Reassign, Tab = TicketTabs.OPEN, Status = "Reassign" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Resolved, Tab = TicketTabs.RESOLVED, Status = "Resolved" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });
            await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb);
            var db = server.Arrange().CreateDbContext<WorkflowContext>();
            db.TicketStatuses.AddRange(ticketStatus);
            BuildTickets(twinId, db);
            db.SaveChanges();

            var expectedSiteTicketStatisticsByStatusDto = new TwinTicketStatisticsByStatus
            {
                TwinId = twinId,
                OpenCount = AllOpenTickets,
                ResolvedCount = CountResolved,
                ClosedCount = CountClosed
            };

            using var client = server.CreateClient(null);
            var response = await client.PostAsJsonAsync("tickets/twins/statistics/status", new TwinStatisticsRequest { TwinIds = twinIds });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsByStatus>>();
            result.Count.Should().Be(1);
            result[0].Should().BeEquivalentTo(expectedSiteTicketStatisticsByStatusDto);
        }

       

        private void BuildTickets(
            string twinId,
            WorkflowContext db)
        {
            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.Open)
            .CreateMany(CountOpen));

            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.Reassign)
            .CreateMany(CountReassign));

            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.InProgress)
            .CreateMany(CountInProgress));

            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.LimitedAvailability)
            .CreateMany(CountLimitedAvailability));

            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.Resolved)
            .CreateMany(CountResolved));

            db.Tickets.AddRange(Fixture.Build<TicketEntity>()
            .Without(x => x.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .With(x => x.TwinId, twinId)
            .With(x => x.Status, (int)TicketStatusEnum.Closed)
            .CreateMany(CountClosed));
        }
    }
}
