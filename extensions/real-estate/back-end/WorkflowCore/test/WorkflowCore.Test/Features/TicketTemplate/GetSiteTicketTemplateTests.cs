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
using Willow.Calendar;

using Newtonsoft.Json;
using Moq;
using Willow.Common;

namespace WorkflowCore.Test.Features.TicketTemplates
{
    public class GetSiteTicketTemplateTests : BaseInMemoryTest
    {
        public GetSiteTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetSiteTickets_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickettemplate");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Theory]
        [InlineData(10, 0, false, 10)]
        [InlineData(10, 5, false, 10)]
        [InlineData(10, 0, true,  0)]
        [InlineData(10, 5, true,  5)]
        [InlineData(10, 0, null,  10)]
        [InlineData(10, 5, null,  15)]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsBelongingToTheGivenSite(int active, int closed, bool? archived, int expectedCount)
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketTemplateEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Assets)
                                               .Without(x => x.Twins)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.DataValue)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Status, (int)TicketStatusEnum.Open)
                                               .With(x => x.OverdueThreshold, "3;3")
                                               .With(x => x.Recurrence, JsonConvert.SerializeObject(_eventMonthly))
                                               .CreateMany(active).ToList();

            if(closed > 0)
            {
                var closedTickets = Fixture.Build<TicketTemplateEntity>()
                                           .Without(x => x.Attachments)
                                           .Without(x => x.Assets)
                                           .Without(x => x.Twins)
                                           .Without(x => x.Tasks)
                                           .Without(x => x.DataValue)
                                           .With(x => x.SiteId, siteId)
                                           .With(x => x.Status, (int)TicketStatusEnum.Closed)
                                           .With(x => x.OverdueThreshold, "3;3")
                                           .With(x => x.Recurrence, JsonConvert.SerializeObject(_eventMonthly))
                                           .CreateMany(closed);

                ticketEntitiesForSite.AddRange(closedTickets);
            }

            var ticketEntitiesForOtherSites = Fixture.Build<TicketTemplateEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Assets)
                                                     .Without(x => x.Twins)
                                                     .Without(x => x.Tasks)
                                                     .CreateMany(10);

            var ticketStatusEntities = new List<TicketStatusEntity>
            {
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.CLOSED)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Closed)
                        .With(x=>x.Status, "Closed")
                       .Create(),
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.OPEN)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Open)
                        .With(x=>x.Status, "Open")
                       .Create(),
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketTemplates.AddRange(ticketEntitiesForSite);
                db.TicketTemplates.AddRange(ticketEntitiesForOtherSites);
                db.TicketStatuses.AddRange(ticketStatusEntities);
                db.SaveChanges();

                var url = archived.HasValue ? $"sites/{siteId}/tickettemplate?archived={archived}" : $"sites/{siteId}/tickettemplate";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<TicketTemplateDto>>();

                Assert.Equal(expectedCount, result.Count);

                //result.Should().BeEquivalentTo(TicketTemplateEntity.MapToModels(ticketEntitiesForSite).Select(m=> TicketTemplateDto.MapFromModel(m));
            }
        }

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] { _eventMonthly };
            yield return new object[] { _eventMonthlyDays };
            yield return new object[] { _eventYearly };
            yield return new object[] { _eventWeekly };
            yield return new object[] { _eventDaily };
            yield return new object[] { _eventHourly };
            yield return new object[] { _eventMinutely };
            yield return new object[] { _eventOnce };
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task TicketTemplateExistsWithNextOccurrenceDate_GetTicketTemplates_ReturnsTicketTemplates(Event recurrenceEvent)
        {
            var siteId = Guid.NewGuid();

            var ticketEntity = Fixture.Build<TicketTemplateEntity>()
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Assets)
                                      .Without(x => x.Twins)
                                      .Without(x => x.DataValue)
                                      .Without(x => x.AssigneeId)
                                      .Without(x => x.Tasks)
                                      .With(x => x.SiteId, siteId)
                                      .With(x => x.Recurrence, JsonConvert.SerializeObject(recurrenceEvent))
                                      .With(x => x.OverdueThreshold, "3;3")
                                      .CreateMany(2);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketTemplates.AddRange(ticketEntity);
                db.SaveChanges();

                var url = $"sites/{siteId}/tickettemplate";
                var response = await client.GetAsync(url);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var result = await response.Content.ReadAsAsync<List<TicketTemplateDto>>();

                result.Should().BeEquivalentTo(TicketTemplateDto.MapFromModel(TicketTemplateEntity.MapToModel(ticketEntity),
                    server.Arrange().GetImagePathHelper(),
                    server.Arrange().GetDateTimeService()));

                result.FirstOrDefault().NextTicketDate.Should().NotBeNullOrEmpty();
            }
        }


        #region Sample Events

        private static Event _eventMonthly = new Event
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

        private static Event _eventMonthlyDays = new Event
        {
            StartDate = DateTime.Parse("2021-01-14T00:00:00"),
            Occurs = Event.Recurrence.Monthly,
            Timezone = "Pacific Standard Time",
            Days = new List<int> { 15 }
        };

        private static Event _eventYearly = new Event
        {
            StartDate = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs = Event.Recurrence.Yearly,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };

        private static Event _eventHourly = new Event
        {
            StartDate = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs = Event.Recurrence.Hourly,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };

        private static Event _eventWeekly = new Event
        {
            StartDate = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs = Event.Recurrence.Weekly,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };

        private static Event _eventDaily = new Event
        {
            StartDate = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs = Event.Recurrence.Daily,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };

        private static Event _eventMinutely = new Event
        {
            StartDate = DateTime.Parse("2021-05-30T00:00:00"),
            Occurs = Event.Recurrence.Minutely,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };

        private static Event _eventOnce = new Event
        {
            StartDate = DateTime.Now.AddDays(1),
            Occurs = Event.Recurrence.Once,
            Interval = 3,
            Timezone = "Eastern Standard Time"
        };
        #endregion        
    }
}
