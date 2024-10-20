using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketStatisticsByTwinIdsTests : BaseInMemoryTest
    {
        public GetTicketStatisticsByTwinIdsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicketStatisticsByTwinIdsTests_RequiresAuthorization()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"tickets/twins/statistics", new TwinStatisticsRequest ());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task NoTwinIds_GetTicketStatisticsByTwinIdsTests_ReturnsBadRequest()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync("tickets/twins/statistics", new TwinStatisticsRequest () );
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("The twinIds are required");
            }
        }
        [Fact]
        public async Task GetTicketStatisticsByTwinIds_ExcludeScheduledTicket_TestsReturnsCountOfTicketlongingToTheGivenSite()
        {
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingTickets = Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Status, (int)TicketStatusEnum.Open)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(6).ToList();

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Status, (int)TicketStatusEnum.New)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(5));

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(4));
            // Add scheduled ticket
            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Occurrence, 1)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(6));

            var existingStatus = Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.ReadyForWork)
                .With(x => x.Tab, TicketTabs.OPEN)
                .CreateMany(1).ToList();

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.New)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.Open)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(existingStatus);
                db.Tickets.AddRange(existingTickets);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<TwinTicketStatisticsDto>()
                {
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[0] &&  c.Occurrence==0).Min(c=>c.Priority),
                        TicketCount = existingTickets.Count(c => c.TwinId==twinIds[0]  &&  c.Occurrence==0)
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[1]  &&  c.Occurrence==0).Min(c=>c.Priority),
                        TicketCount =  existingTickets.Count(c => c.TwinId==twinIds[1]  &&  c.Occurrence==0)
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[2]  &&  c.Occurrence==0).Min(c=>c.Priority),
                        TicketCount = existingTickets.Count(c => c.TwinId == twinIds[2]  &&  c.Occurrence==0)
                    }
                };

                var response = await client.PostAsJsonAsync($"tickets/twins/statistics", new TwinStatisticsRequest { TwinIds = twinIds });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetTicketStatisticsByTwinIdsTestsReturnsCountOfTicketlongingToTheGivenSite()
        {
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingTickets = Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Status, (int)TicketStatusEnum.Open)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(6).ToList();

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Status, (int)TicketStatusEnum.New)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(5));

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(4));

            var existingStatus = Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.ReadyForWork)
                .With(x => x.Tab, TicketTabs.OPEN)
                .CreateMany(1).ToList();

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.New)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.Open)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(existingStatus);
                db.Tickets.AddRange(existingTickets);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<TwinTicketStatisticsDto>()
                {
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[0]).Min(c=>c.Priority),
                        TicketCount = existingTickets.Count(c => c.TwinId==twinIds[0])
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[1]).Min(c=>c.Priority),
                        TicketCount =  existingTickets.Count(c => c.TwinId==twinIds[1])
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[2]).Min(c=>c.Priority),
                        TicketCount = existingTickets.Count(c => c.TwinId == twinIds[2])
                    }
                };

                var response = await client.PostAsJsonAsync($"tickets/twins/statistics", new TwinStatisticsRequest { TwinIds = twinIds });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }

        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_ReturnsCountOfInsightsBelongingToTheGivenSite()
        {
            var twinIds = new List<string> { "twin1", "twin2", "twin3" };
            var existingTickets = Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[0])
                .With(x => x.Status, (int)TicketStatusEnum.Open)
                .Without(x=>x.Occurrence)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(6).ToList();

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[1])
                .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(5));

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, twinIds[2])
                .With(x => x.Status, (int)TicketStatusEnum.New)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(4));

            var existingStatus = Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.InProgress)
                .With(x => x.Tab, TicketTabs.OPEN)
                .CreateMany(1).ToList();

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.New)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.Open)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(existingStatus);
                db.Tickets.AddRange(existingTickets);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<TwinTicketStatisticsDto>()
                {
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[0],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[0]).Min(c=>c.Priority),
                        TicketCount = 6
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[1],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[1]).Min(c=>c.Priority),
                        TicketCount = 5
                    },
                    new TwinTicketStatisticsDto()
                    {
                        TwinId = twinIds[2],
                        HighestPriority = existingTickets.Where(c=>c.TwinId==twinIds[2]).Min(c=>c.Priority),
                        TicketCount = 4
                    }
                };

                var response = await client.PostAsJsonAsync($"tickets/twins/statistics", new TwinStatisticsRequest{ TwinIds = twinIds});

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }


        [Fact]
        public async Task GetInsightStatisticsByTwinIdsTests_WithSourceTypeFilters_ReturnsStatistics()
        {
            var request = new TwinStatisticsRequest
            {
                TwinIds = new List<string> { "twin1", "twin2", "twin3" },
                SourceTypes = new List<SourceType>() { SourceType.Mapped }
            };

            var existingTickets = Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, request.TwinIds[0])
                .With(x => x.Status, (int)TicketStatusEnum.Open)
                .With(x => x.SourceType, SourceType.Dynamics)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(6).ToList();

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, request.TwinIds[1])
                .With(x => x.Status, (int)TicketStatusEnum.New)
                .With(x=>x.SourceType, SourceType.Mapped)
                .Without(x => x.Comments)
                .Without(x => x.JobType)
                .Without(x => x.Occurrence)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(5));

            existingTickets.AddRange(Fixture.Build<TicketEntity>()
                .With(x => x.TwinId, request.TwinIds[2])
                .With(x => x.Status, (int)TicketStatusEnum.ReadyForWork)
                .With(x=>x.SourceType,SourceType.App)
                .Without(x => x.Comments)
                .Without(x => x.Occurrence)
                .Without(x => x.JobType)
                .Without(x => x.Diagnostics)
                .Without(x => x.Attachments)
                .Without(x => x.Category)
                .Without(x => x.Tasks)
                .CreateMany(4));

            var existingStatus = Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.ReadyForWork)
                .With(x => x.Tab, TicketTabs.OPEN)
                .CreateMany(1).ToList();

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.New)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            existingStatus.Add(Fixture.Build<TicketStatusEntity>()
                .With(x => x.StatusCode, (int)TicketStatusEnum.Open)
                .With(x => x.Tab, TicketTabs.OPEN)
                .Create());

            using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatuses.AddRange(existingStatus);
                db.Tickets.AddRange(existingTickets);
                db.SaveChanges();

                var expectedInsightStatisticsDto = new List<TwinTicketStatisticsDto>()
                {
                    new TwinTicketStatisticsDto()
                    {
                        TwinId =  request.TwinIds[1],
                        HighestPriority = existingTickets.Where(c=>c.TwinId== request.TwinIds[1]).Min(c=>c.Priority),
                        TicketCount =  existingTickets.Count(c => c.TwinId== request.TwinIds[1])
                    }
                };

                var response = await client.PostAsJsonAsync($"tickets/twins/statistics",request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TwinTicketStatisticsDto>>();
                result.Should().BeEquivalentTo(expectedInsightStatisticsDto);
            }
        }
    }
}
