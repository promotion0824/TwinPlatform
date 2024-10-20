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
    public class GetSiteStatisticsTests : BaseInMemoryTest
    {
        public GetSiteStatisticsTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetSiteStatistics_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"siteStatistics");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoSiteIdIsNotProvided_GetSiteStatistics_ReturnsBadRequest()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"siteStatistics");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The siteIds are empty");
            }
        }

        [Fact]
        public async Task SingleSiteExist_GetSiteStatistics_ReturnsCountOfTicketsBelongingToTheGivenSite()
        {
            var siteId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddMonths(1))
                                               .CreateMany(10);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.Priority, 1)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);

            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(ticketStatus);
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.SaveChanges();

                var expectedSiteStatisticsDto = new List<SiteStatisticsDto>()
                {
                    new SiteStatisticsDto()
                    {
                        Id = siteId,
                        OverdueCount = 10,
                        UrgentCount = 10,
                        HighCount = 0,
                        MediumCount = 0,
                        LowCount = 10,
                        OpenCount = 20
                    }
                };

                var response = await client.GetAsync($"siteStatistics?siteIds={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteStatistics>>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDto);
            }
        }

        [Fact]
        public async Task SingleSiteExist_GetSiteStatistics_ReturnsEmpty()
        {
            var siteId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.SaveChanges();

                var expectedSiteStatisticsDto = new List<SiteStatisticsDto>()
                {
                    new SiteStatisticsDto()
                    {
                        Id = siteId,
                        OverdueCount = 0,
                        UrgentCount = 0,
                        HighCount = 0,
                        MediumCount = 0,
                        LowCount = 0,
                        OpenCount = 0
                    }
                };

                var response = await client.GetAsync($"siteStatistics?siteIds={siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteStatistics>>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDto);
            }
        }

        [Theory]
        [InlineData(9, 7, 19, 14)]
        [InlineData(12, 4, 33, 8)]
        public async Task SingleSiteExist_GetSiteTicketStatistics_ReturnsCountOfTicketsBelongingToTheGivenSite(int lowInProgress, 
                                                                                                               int lowClosed, 
                                                                                                               int lowResolved, 
                                                                                                               int lowReassign)
        {
            var siteId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddDays(1))
                                               .CreateMany(lowInProgress);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.Priority, 1)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(1))
                                                     .CreateMany(lowClosed);
            var ticketsResolved = Fixture.Build<TicketEntity>()
                                         .Without(x => x.Attachments)
                                         .Without(x => x.Comments)
                                         .Without(x => x.JobType)
                                         .Without(x => x.Diagnostics)
                                         .With(x => x.Occurrence, 0)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.Priority, 4)
                                         .With(x => x.Status, (int)TicketStatusEnum.Resolved)
                                         .With(x => x.DueDate, utcNow.AddDays(1))
                                         .CreateMany(lowResolved);
            var ticketsReassign = Fixture.Build<TicketEntity>()
                                         .Without(x => x.Attachments)
                                         .Without(x => x.Comments)
                                         .Without(x => x.JobType)
                                         .Without(x => x.Diagnostics)
                                         .With(x => x.Occurrence, 0)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.Priority, 4)
                                         .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                         .With(x => x.DueDate, utcNow.AddDays(1))
                                         .CreateMany(lowReassign);

            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Reassign, Tab = TicketTabs.OPEN, Status = "Reassign" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Resolved, Tab = TicketTabs.RESOLVED, Status = "Resolved" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(ticketStatus);
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.Tickets.AddRange(ticketsResolved);
                db.Tickets.AddRange(ticketsReassign);
                db.SaveChanges();

                var expectedSiteStatisticsDto = new SiteStatisticsDto
                {
                    Id = siteId,
                    OverdueCount = 10,
                    UrgentCount = 10,
                    HighCount = 0,
                    MediumCount = 0,
                    LowCount = lowInProgress + lowReassign,
                    OpenCount = 10 + lowInProgress + lowReassign
                };

                var response = await client.GetAsync($"statistics/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteStatistics>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDto);
            }
        }       
        
        [Theory]
        [InlineData(9, 7, 19, 14)]
        [InlineData(12, 4, 33, 8)]
        public async Task SingleSiteExist_GetSiteTicketStatistics_ReturnsCountOfTicketsBelongingToTheGivenFloor(int lowInProgress, 
                                                                                                               int lowClosed, 
                                                                                                               int lowResolved, 
                                                                                                               int lowReassign)
        {
            var siteId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.FloorCode, "L5")
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddDays(1))
                                               .CreateMany(lowInProgress);
            var ticketsForDifferentFloor = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.FloorCode, "L3")
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddDays(1))
                                               .CreateMany(lowInProgress);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.FloorCode, "L5")
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.Priority, 1)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.FloorCode, "L5")
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(1))
                                                     .CreateMany(lowClosed);
            var ticketsResolved = Fixture.Build<TicketEntity>()
                                         .Without(x => x.Attachments)
                                         .Without(x => x.Comments)
                                         .Without(x => x.JobType)
                                         .Without(x => x.Diagnostics)
                                         .With(x => x.Occurrence, 0)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.FloorCode, "L5")
                                         .With(x => x.Priority, 4)
                                         .With(x => x.Status, (int)TicketStatusEnum.Resolved)
                                         .With(x => x.DueDate, utcNow.AddDays(1))
                                         .CreateMany(lowResolved);
            var ticketsReassign = Fixture.Build<TicketEntity>()
                                         .Without(x => x.Attachments)
                                         .Without(x => x.Comments)
                                         .Without(x => x.JobType)
                                         .Without(x => x.Diagnostics)
                                         .With(x => x.Occurrence, 0)
                                         .With(x => x.SiteId, siteId)
                                         .With(x => x.FloorCode, "L5")
                                         .With(x => x.Priority, 4)
                                         .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                         .With(x => x.DueDate, utcNow.AddDays(1))
                                         .CreateMany(lowReassign);

            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Reassign, Tab = TicketTabs.OPEN, Status = "Reassign" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Resolved, Tab = TicketTabs.RESOLVED, Status = "Resolved" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(ticketStatus);
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketsForDifferentFloor);                
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.Tickets.AddRange(ticketsResolved);
                db.Tickets.AddRange(ticketsReassign);
                db.SaveChanges();

                var expectedSiteStatisticsDto = new SiteStatisticsDto
                {
                    Id = siteId,
                    OverdueCount = 10,
                    UrgentCount = 10,
                    HighCount = 0,
                    MediumCount = 0,
                    LowCount = lowInProgress + lowReassign,
                    OpenCount = 10 + lowInProgress + lowReassign
                };

                var response = await client.GetAsync($"statistics/site/{siteId}?floorId=L5");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteStatistics>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDto);
            }
        }       
        
        [Fact]
        public async Task SingleSiteExist_GetSiteTicketStatistics_SiteEmpty()
        {
            var siteId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(1))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.SaveChanges();

                var expectedSiteStatisticsDto = new SiteStatisticsDto
                {
                    Id = siteId,
                    OverdueCount = 0,
                    UrgentCount = 0,
                    HighCount = 0,
                    MediumCount = 0,
                    LowCount = 0,
                    OpenCount = 0
                };

                var response = await client.GetAsync($"statistics/site/{siteId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<SiteStatistics>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDto);
            }
        }

        [Fact]
        public async Task MultiSiteExist_GetSiteStatistics_ReturnsCountOfTicketsBelongingToTheGivenSite()
        {
            var siteIds = new List<Guid>{ Guid.NewGuid(), Guid.NewGuid() };
            var utcNow = DateTime.UtcNow;

            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteIds[0])
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.DueDate, utcNow.AddMonths(1))
                                               .CreateMany(10);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[0])
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[0])
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteII = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteIds[1])
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.DueDate, utcNow.AddMonths(1))
                                               .CreateMany(10);
            var ticketEntitiesForSiteOverDueII = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteIds[1])
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(8);
            var ticketEntitiesForSiteOverDueClosedII = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.SiteId, siteIds[1])
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(5);


            var ticketStatus = new List<TicketStatusEntity>();
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Open, Tab = TicketTabs.OPEN, Status = "Open" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.InProgress, Tab = TicketTabs.OPEN, Status = "InProgress" });
            ticketStatus.Add(new TicketStatusEntity { StatusCode = (int)TicketStatusEnum.Closed, Tab = TicketTabs.CLOSED, Status = "Closed" });
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(ticketStatus);
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.Tickets.AddRange(ticketEntitiesForSiteII);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueII);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosedII);
                db.SaveChanges();
                var siteStatisticsList = await db.Tickets.Where(x => siteIds.Contains(x.SiteId)).GroupBy(x => x.SiteId)
                            .Select(g => new SiteStatistics
                            {
                                Id = g.Key,
                                OverdueCount = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.DueDate.HasValue && x.DueDate.Value < utcNow.Date) ? 1 : 0),
                                UrgentCount  = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.Priority == 1) ? 1 : 0),
                                HighCount    = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.Priority == 2) ? 1 : 0),
                                MediumCount  = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.Priority == 3) ? 1 : 0),
                                LowCount     = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.Priority == 4) ? 1 : 0),
                                OpenCount    = g.Sum(x => (x.Status != (int)TicketStatusEnum.Closed && x.Status != (int)TicketStatusEnum.Resolved) ? 1 : 0)
                            }).ToListAsync();
                var expectedSiteStatisticsDtos = new List<SiteStatisticsDto>();
                var site1Stat = siteStatisticsList.Find(x => x.Id == siteIds[0]);
                expectedSiteStatisticsDtos.Add(new SiteStatisticsDto
                {
                    Id = siteIds[0],
                    OverdueCount = site1Stat.OverdueCount,
                    UrgentCount = site1Stat.UrgentCount,
                    HighCount = site1Stat.HighCount,
                    MediumCount = site1Stat.MediumCount,
                    LowCount = site1Stat.LowCount,
                    OpenCount = site1Stat.OpenCount
                });
                var site2Stat = siteStatisticsList.Find(x => x.Id == siteIds[1]);
                expectedSiteStatisticsDtos.Add(new SiteStatisticsDto
                {
                    Id = siteIds[1],
                    OverdueCount = site2Stat.OverdueCount,
                    UrgentCount = site2Stat.UrgentCount,
                    HighCount = site2Stat.HighCount,
                    MediumCount = site2Stat.MediumCount,
                    LowCount = site2Stat.LowCount,
                    OpenCount = site2Stat.OpenCount
                });
                
                var response = await client.GetAsync($"siteStatistics?siteIds={siteIds[0]}&siteIds={siteIds[1]}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteStatistics>>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDtos);
            }
        }

        [Fact]
        public async Task NoTicketsExistsInSite_GetSiteStatistics_ReturnsZeorWithSite()
        {
            var siteId = Guid.NewGuid();
            var siteIdWithoutData = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddMonths(1))
                                               .CreateMany(1);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(1);
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(1);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.SaveChanges();

                var response = await client.GetAsync($"siteStatistics?siteIds={siteIdWithoutData}");

                var expectedSiteStatisticsDtos = new List<SiteStatisticsDto>()
                {
                    new SiteStatisticsDto()
                    {
                        Id = siteIdWithoutData,
                        OverdueCount = 0,
                        UrgentCount = 0,
                        HighCount = 0,
                        MediumCount = 0
                    }
                };

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<SiteStatistics>>();
                result.Should().BeEquivalentTo(expectedSiteStatisticsDtos);
            }
        }
    }
}
