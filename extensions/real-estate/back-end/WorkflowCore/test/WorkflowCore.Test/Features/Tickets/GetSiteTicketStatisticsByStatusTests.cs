using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using WorkflowCore.Models;
using System.Linq;
using Willow.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetSiteTicketStatisticsByStatusTests : BaseInMemoryTest
    {
        public GetSiteTicketStatisticsByStatusTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetSiteTicketStatisticsByStatus_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"ticketStatisticsByStatus/sites/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task SingleSiteExist_GetSiteTicketStatisticsByStatus_ReturnsCountOfTicketsByStatusBelongingToTheGivenSite()
        {
            var siteId = Guid.NewGuid();
            var openTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int) TicketStatusEnum.Open)
                                               .CreateMany(4);

            var closedTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                               .CreateMany(4);

            var resolvedTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.Resolved)
                                               .CreateMany(2);

            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Reassign, Tab = TicketTabs.OPEN, Status = "Reassign" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.LimitedAvailability, Tab = TicketTabs.OPEN, Status = "LimitedAvailability" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Resolved, Tab = TicketTabs.RESOLVED, Status = "Resolved" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(openTicketEntitiesForSite);
                db.Tickets.AddRange(closedTicketEntitiesForSite);
                db.Tickets.AddRange(resolvedTicketEntitiesForSite);
                db.TicketStatuses.AddRange(ticketStatus);
                db.SaveChanges();

                var expectedSiteTicketStatisticsByStatusDto = new SiteTicketStatisticsByStatusDto
                {
                    Id = siteId,
                    OpenCount = 4,
                    ResolvedCount = 2,
                    ClosedCount = 4
                };

                var response = await client.GetAsync($"ticketStatisticsByStatus/sites/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteTicketStatisticsByStatusDto>();
                result.Should().BeEquivalentTo(expectedSiteTicketStatisticsByStatusDto);
            }
        }
        
        [Fact]
        public async Task SingleSiteExist_GetSiteTicketStatisticsByStatus_SiteEmpty()
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .With(x => x.SiteId, siteId)
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.SaveChanges();

                var expectedSiteTicketStatisticsByStatusDto = new SiteTicketStatisticsByStatusDto
                {
                    Id = siteId,
                    OpenCount = 0,
                    ResolvedCount = 0,
                    ClosedCount = 0
                };

                var response = await client.GetAsync($"ticketStatisticsByStatus/sites/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteTicketStatisticsByStatusDto>();
                result.Should().BeEquivalentTo(expectedSiteTicketStatisticsByStatusDto);
            }
        }
    }
}
