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
using WorkflowCore.Services.Apis;

using Moq;

using Newtonsoft.Json;
using WorkflowCore.Models;
using Willow.Calendar;
using Willow.Common;

namespace WorkflowCore.Test.Features.TicketTemplates
{
    public class GetTicketTemplateTests : BaseInMemoryTest
    {
        private readonly Mock<IDateTimeService> _datetimeService;

        public GetTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
            _datetimeService = new Mock<IDateTimeService>();
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickettemplate/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketNotExists_GetTicket_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickettemplate/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task TicketTemplateExistsWithStringArrayTasks_GetTicket_ReturnsTicket()
        {
            var utcNow = DateTime.UtcNow;

            _datetimeService.Setup(w => w.UtcNow).Returns(utcNow);

            var ticketEntity = Fixture.Build<TicketTemplateEntity>()
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Assets)
                                      .Without(x => x.Twins)
                                      .Without(x => x.DataValue)
                                      .With(x => x.Tasks, JsonConvert.SerializeObject(new string[] { "abd", "cde" }))
                                      .With(x => x.OverdueThreshold, "3;3")
                                      .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                      .Create();

            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketTemplateEntity.MapToModel(ticketEntity);
            var expectedTicketDto = TicketTemplateDto.MapFromModel(expectedTicket, imagePathHelper, _datetimeService.Object);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(ticketEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickettemplate/{ticketEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        [Fact]
        public async Task TicketTemplateExistsWithStringArrayTasks_GetTicket_ReturnsTicket2()
        {
            _datetimeService.Setup(w => w.UtcNow).Returns(DateTime.UtcNow);

            var ticketEntity = Fixture.Build<TicketTemplateEntity>()
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Assets)
                                      .Without(x => x.Twins)
                                      .Without(x => x.DataValue)
                                      .Without(x => x.AssigneeId)
                                      .With(x => x.Tasks, JsonConvert.SerializeObject(new string[] { "abd", "cde" }))
                                      .With(x => x.OverdueThreshold, "3;3")
                                      .With(x => x.Recurrence, JsonConvert.SerializeObject(_event2))
                                      .Create();

            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketTemplateEntity.MapToModel(ticketEntity);
            var expectedTicketDto = TicketTemplateDto.MapFromModel(expectedTicket, imagePathHelper, _datetimeService.Object);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(ticketEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickettemplate/{ticketEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        [Fact]
        public async Task TicketTemplateExistsWithTemplatedTasks_GetTicket_ReturnsTicket()
        {
            var utcNow = DateTime.UtcNow;

            _datetimeService.Setup(w => w.UtcNow).Returns(utcNow);

            var tasks = Fixture.CreateMany<TicketTaskTemplate>();
            var ticketEntity = Fixture.Build<TicketTemplateEntity>()
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Assets)
                                      .Without(x => x.Twins)
                                      .Without(x => x.DataValue)
                                      .With(x => x.Tasks, JsonConvert.SerializeObject(tasks))
                                      .With(x => x.OverdueThreshold, "3;3")
                                      .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                      .Create();

            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketTemplateEntity.MapToModel(ticketEntity);
            var expectedTicketDto = TicketTemplateDto.MapFromModel(expectedTicket, imagePathHelper, _datetimeService.Object);
            
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(ticketEntity);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickettemplate/{ticketEntity.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        #region Sample Events

        private static Event _event1 = new Event
        {
            StartDate      = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Timezone       = "Pacific Standard Time",
            DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Wednesday
                }
            }
        };

        private static Event _event2 = new Event
        {
            StartDate      = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs         = Event.Recurrence.Monthly,
            Interval       = 3,
            Timezone       = "Eastern Standard Time",
            Days           = new List<int> { 30 }
        };

        #endregion
    }
}
