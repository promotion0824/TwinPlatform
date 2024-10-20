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
using System.Net.Http.Json;
using Willow.Infrastructure;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketStatisticsBySiteIdsTests : BaseInMemoryTest
    {
        public GetTicketStatisticsBySiteIdsTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetTicketStatisticsBySiteIds_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"tickets/statistics",new List<Guid>{});
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoSiteIds_GetTicketStatisticsBySiteIds_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"tickets/statistics", new List<Guid>());
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
              
            }
        }
        [Fact]
        public async Task GetTicketStatisticsBySiteIds_ReturnsSiteTicketsStatistics()
        {
            var siteIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var expectedData = GenerateTickets(siteIds);
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
                db.TicketStatuses.AddRange(ticketStatus);
                db.Tickets.AddRange(expectedData.Item2);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"tickets/statistics",siteIds);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketStatisticsDto>();
                result.Should().BeEquivalentTo(expectedData.Item1);
            }
        }

        private (TicketStatisticsDto, List<TicketEntity>) GenerateTickets(List<Guid> siteIds)
        {
            var utcNow = DateTime.UtcNow;
            var ticketEntities = Fixture.Build<TicketEntity>()
                                           .Without(x => x.Attachments)
                                           .Without(x => x.Comments)
                                           .Without(x => x.JobType)
                                           .Without(x => x.Diagnostics)
                                           .With(x => x.Occurrence, 0)
                                           .With(x => x.SiteId, siteIds[0])
                                           .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                           .With(x => x.Priority, 4)
                                           .With(x => x.DueDate, utcNow.AddMonths(1))
                                           .CreateMany(3).ToList();
            ticketEntities.AddRange( Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[0])
                                                     .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                                     .With(x => x.Priority, 1)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(4));
            ticketEntities.AddRange(Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[0])
                                                     .With(x => x.Priority, 3)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(2));


            ticketEntities.AddRange(Fixture.Build<TicketEntity>()
                                              .Without(x => x.Attachments)
                                              .Without(x => x.Comments)
                                              .Without(x => x.JobType)
                                              .Without(x => x.Diagnostics)
                                              .With(x => x.Occurrence, 0)
                                              .With(x => x.SiteId, siteIds[1])
                                              .With(x => x.Status, (int)TicketStatusEnum.Open)
                                              .With(x => x.Priority,3)
                                              .With(x => x.DueDate, utcNow.AddMonths(1))
                                              .CreateMany(6));
            ticketEntities.AddRange(Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[1])
                                                     .With(x => x.Status, (int)TicketStatusEnum.LimitedAvailability)
                                                     .With(x => x.Priority,2)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(5));
            ticketEntities.AddRange(Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[1])
                                                     .With(x => x.Priority, 2)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Resolved)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(2));

            var expectedresponse = new TicketStatisticsDto()
            {
                StatisticsByPriority = new List<SiteStatistics>()
                {
                    new()
                    {
                        Id = siteIds[0],
                        OpenCount = 7,
                        LowCount = 3,
                        UrgentCount =4,
                        OverdueCount = 4
                    },
                    new()
                    {
                        Id = siteIds[1],
                        OpenCount = 11,
                        MediumCount = 6,
                        HighCount = 5,
                        OverdueCount = 5
                    },
                    new()
                    {
                        Id = siteIds[2]
                    }
                },
                StatisticsByStatus = new List<SiteTicketStatisticsByStatus>()
                {
                    new()
                    {
                        Id = siteIds[0],
                        // open count include ticket status that belongs to open tab
                        OpenCount = 7,
                        // closed count include ticket status that belongs to closed tab
                        ClosedCount =2
                    },
                    new()
                    {
                        Id = siteIds[1],
                          // open count include ticket status that belongs to open tab
                        OpenCount = 11,
                        // resolved count include ticket status that belongs to resolved tab
                        ResolvedCount =2
                    },
                    new()
                    {
                        Id = siteIds[2]
                    }
                }
            };

            return (expectedresponse, ticketEntities);
        }
    }
}
