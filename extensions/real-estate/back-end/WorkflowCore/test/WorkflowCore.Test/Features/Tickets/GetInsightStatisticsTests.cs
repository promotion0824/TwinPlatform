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
using WorkflowCore.Controllers.Request;
using System.Net.Http.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetInsightStatisticsTests : BaseInMemoryTest
    {
        public GetInsightStatisticsTests(ITestOutputHelper output) : base(output)
        {
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
        
        [Fact]
        public async Task TokenIsNotGiven_GetInsightStatistics_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest()));
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoInsightIdIsProvided_GetInsightStatistics_ReturnsBadRequest()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest()));
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The insightIds are empty");
            }
        }

        [Fact]
        public async Task SingleInsightExist_GetInsightStatistics_ReturnsCountOfTicketsBelongingToTheGivenInsight()
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
			var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
											   .Without(x => x.Attachments)
											   .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
											   .With(x => x.InsightId, insightId)
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
													 .With(x => x.InsightId, insightId)
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
													 .With(x => x.InsightId, insightId)
													 .With(x => x.SiteId, siteId)
                                                     .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.SaveChanges();

				var expectedInsightStatisticsDto = new List<InsightStatisticsDto>()
				{
                    new InsightStatisticsDto()
                    {
                        Id = insightId,
                        ScheduledCount = 0,
                        OverdueCount = 20,
                        TotalCount = 30
                    }
                };

				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = new List<Guid> { insightId }
				}));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task SingleInsightExist_GetInsightStatistics_ReturnsEmpty()
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<InsightStatisticsDto>()
                {
                    new InsightStatisticsDto()
                    {
                        Id = insightId,
                        OverdueCount = 0,
                        TotalCount = 0
                    }
                };

				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = new List<Guid> { insightId }
				}));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Theory]
        [InlineData(9, 7)]
        [InlineData(12, 4)]
        public async Task SingleSiteExist_GetSiteTicketStatistics_ReturnsCountOfTicketsBelongingToTheGivenSite(int overdue, int scheduled)
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSiteScheduled = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 2)
                                               .With(x => x.SiteId, siteId)
											   .With(x => x.InsightId, insightId)
                                               .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.Priority, 4)
                                               .With(x => x.DueDate, utcNow.AddDays(1))
                                               .CreateMany(scheduled);
            var ticketEntitiesForSiteOverDue = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SiteId, siteId)
													 .With(x => x.InsightId, insightId)
													 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                                     .With(x => x.Priority, 1)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(overdue);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSiteScheduled);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<InsightStatisticsDto>()
                {
                    new InsightStatisticsDto()
                    {
                        Id = insightId,
                        OverdueCount = overdue,
                        ScheduledCount = scheduled,
                        TotalCount = overdue + scheduled

                    }
                };

				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = new List<Guid> { insightId }
				}));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }       
                
        [Fact]
        public async Task SingleInsightDifferentStatusExist_GetInsightStatistics_InsightEmpty()
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSiteOverDueClosed = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
													 .With(x => x.InsightId, insightId)
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

                var expectedInsightStatisticsDto = new List<InsightStatisticsDto> { new InsightStatisticsDto
				{
					Id = insightId,
					ScheduledCount = 0,
					OverdueCount = 0,
					TotalCount = 0
				}};

				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = new List<Guid> { insightId },
					Statuses = new List<int> { (int)TicketStatusEnum.Open }
				}));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task MultiInsightsExist_GetInsightStatistics_ReturnsCountOfTicketsBelongingToTheGivenInsight()
        {
            var siteIds = new List<Guid>{ Guid.NewGuid(), Guid.NewGuid() };
			var insightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
			var utcNow = DateTime.UtcNow;

            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.SiteId, siteIds[0])
											   .With(x => x.InsightId, insightIds[0])
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
													 .With(x => x.InsightId, insightIds[0])
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
													 .With(x => x.InsightId, insightIds[0])
													 .With(x => x.Priority, 4)
                                                     .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(10);
            var ticketEntitiesForSiteII = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.Occurrence, 1)
                                               .With(x => x.SiteId, siteIds[1])
											   .With(x => x.InsightId, insightIds[1])
											   .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                               .With(x => x.DueDate, utcNow.AddMonths(1))
                                               .CreateMany(10);
            var ticketEntitiesForSiteOverDueII = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 1)
                                                     .With(x => x.SiteId, siteIds[1])
													 .With(x => x.InsightId, insightIds[1])
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
													 .With(x => x.InsightId, insightIds[1])
													 .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                                     .With(x => x.DueDate, utcNow.AddMonths(-1))
                                                     .CreateMany(5);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDue);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosed);
                db.Tickets.AddRange(ticketEntitiesForSiteII);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueII);
                db.Tickets.AddRange(ticketEntitiesForSiteOverDueClosedII);
                db.SaveChanges();
                var insightStatisticsList = await db.Tickets.Where(x => insightIds.Contains(x.InsightId.Value)).GroupBy(x => x.InsightId)
                            .Select(g => new InsightStatistics
                            {
                                Id = g.Key.Value,
								OverdueCount = g.Sum(x => x.DueDate.HasValue && x.DueDate.Value < utcNow.Date ? 1 : 0),
								ScheduledCount = g.Sum(x => (x.Occurrence > 0) ? 1 : 0),
                                TotalCount = g.Sum(x => 1)
                            }).ToListAsync();
                var expectedInsightStatisticsDtos = new List<InsightStatisticsDto>();
                var insight1Stat = insightStatisticsList.Find(x => x.Id == insightIds[0]);
                expectedInsightStatisticsDtos.Add(new InsightStatisticsDto
                {
                    Id = insightIds[0],
					OverdueCount = insight1Stat.OverdueCount,
					ScheduledCount = insight1Stat.ScheduledCount,
                    TotalCount = insight1Stat.TotalCount
                });
                var insight2Stat = insightStatisticsList.Find(x => x.Id == insightIds[1]);
                expectedInsightStatisticsDtos.Add(new InsightStatisticsDto
                {
                    Id = insightIds[1],
					OverdueCount = insight2Stat.OverdueCount,
					ScheduledCount = insight2Stat.ScheduledCount,
					TotalCount = insight2Stat.TotalCount
				});
                
				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = insightIds,
				}));

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDtos);
            }
        }

        [Fact]
        public async Task NoTicketsExistsForInsight_GetInsightStatistics_ReturnsEmpty()
        {
            var siteId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
            var siteIdWithoutData = Guid.NewGuid();
			var insightIdWithoutData = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
											   .With(x => x.InsightId, insightId)
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
													 .With(x => x.InsightId, insightId)
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
													 .With(x => x.InsightId, insightId)
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

				var response = await client.PostAsync($"insightStatistics", JsonContent.Create(new GetInsightStatisticsRequest
				{
					InsightIds = new List<Guid> { insightIdWithoutData },
				}));

				var expectedInsightStatisticsDtos = new List<InsightStatisticsDto>()
                {
                    new InsightStatisticsDto()
                    {
                        Id = insightIdWithoutData,
                        OverdueCount = 0,
                        ScheduledCount = 0,
                        TotalCount = 0
                    }
                };

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<InsightStatistics>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDtos);
            }
        }
    }
}
