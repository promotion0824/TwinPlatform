using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Willow.Calendar;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using Xunit;
using Xunit.Abstractions;
using Moq;

using Newtonsoft.Json;
using WorkflowCore.Models;
using WorkflowCore.Entities;
using WorkflowCore.Controllers.Request;
using Willow.Common;

namespace WorkflowCore.Test.Features.TicketTemplates
{
    public class UpdateTicketTemplateTests : BaseInMemoryTest
    {
        private readonly Mock<IDateTimeService> _datetimeService = new Mock<IDateTimeService>();

        public UpdateTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_UpdateTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate/{Guid.NewGuid()}", new UpdateTicketTemplateRequest());
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_UpdateTicket_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate/{Guid.NewGuid()}", new UpdateTicketTemplateRequest() { AssigneeType = AssigneeType.NoAssignee });
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenInvalidStatus_UpdateTicket_ReturnsBadRequest()
        {
            var existingTicket = Fixture.Build<TicketTemplateEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Assets)
                                        .With(x => x.OverdueThreshold, "3;3")
                                        .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                        .Without(x => x.Tasks)
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{existingTicket.SiteId}/tickettemplate/{existingTicket.Id}", new UpdateTicketTemplateRequest { Status = 999, AssigneeType = AssigneeType.NoAssignee });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Status", out _));
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdateTicket_ReturnsUpdatedTicket()
        {
            var utcNow = DateTime.UtcNow;

            _datetimeService.Setup(w => w.UtcNow).Returns(utcNow);

            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var dataValue = Fixture.Create<DataValue>();
            var request = Fixture.Build<UpdateTicketTemplateRequest>()
                                 .With(x => x.CustomerId, customerId)
                                 .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.OverdueThreshold, new Duration("3;3"))
                                 .With(x => x.Recurrence, _event1)
                                 .With(x => x.DataValue, dataValue)
                                 .Create();
            var existingTicket = Fixture.Build<TicketTemplateEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Assets)
                                        .Without(x => x.Tasks)
                                        .With(x => x.CustomerId, customerId)
                                        .With(x => x.DataValue, "{}")
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.OverdueThreshold, "3;3")
                                        .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                        .With(x => x.SiteId, siteId)
                                        .Create();

            var customerTicketStatuses = new List<TicketStatusEntity> {
                                                Fixture.Build<TicketStatusEntity>().With(x => x.CustomerId, customerId).With(x => x.StatusCode, request.Status).Create(),
                                                Fixture.Build<TicketStatusEntity>().With(x => x.CustomerId, customerId).With(x => x.StatusCode, existingTicket.Status).Create()};

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(existingTicket);
                db.TicketStatuses.AddRange(customerTicketStatuses);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickettemplate/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketTemplates.Should().HaveCount(1);

                var updatedTicket = db.TicketTemplates.First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(request.Priority);
                updatedTicket.Status.Should().Be(request.Status);
                updatedTicket.Summary.Should().Be(request.Summary);
                updatedTicket.Description.Should().Be(request.Description);
                updatedTicket.AssigneeType.Should().Be(request.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(request.AssigneeId);
                updatedTicket.UpdatedDate.Should().Be(utcNow);
                updatedTicket.Recurrence.Should().Be(JsonConvert.SerializeObject(request.Recurrence));
                updatedTicket.OverdueThreshold.Should().Be(request.OverdueThreshold.ToString());

                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                var expectedTicketDto = TicketTemplateDto.MapFromModel(TicketTemplateEntity.MapToModel(updatedTicket), server.Assert().GetImagePathHelper(), _datetimeService.Object);

                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        [Fact]
        public async Task GivenNoUpdatedProperty_UpdateTicket_ReturnsNothingChanges()
        {
            var utcNow = DateTime.UtcNow;

            _datetimeService.Setup(w => w.UtcNow).Returns(utcNow);

            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var existingTicket = Fixture.Build<TicketTemplateEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Assets)
                                        .Without(x => x.Twins)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.DataValue)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                        .With(x => x.OverdueThreshold, "3;3")
                                        .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.UpdatedDate, utcNow)
                                        .With(x => x.CategoryName, "Chevy")
                                        .With(x => x.CategoryId, categoryId)
                                        .Create();
            var request = new UpdateTicketTemplateRequest { CategoryId = categoryId, AssigneeType = existingTicket.AssigneeType, AssigneeId = existingTicket.AssigneeId };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickettemplate/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Should().HaveCount(1);
                var updatedTicket = db.TicketTemplates.First();

                updatedTicket.Should().BeEquivalentTo(existingTicket, config => config.Excluding(x => x.Attachments).Excluding(x => x.Assets).Excluding(x => x.Tasks).Excluding(x => x.CategoryName));

                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                var expectedTicketDto = TicketTemplateDto.MapFromModel(TicketTemplateEntity.MapToModel(updatedTicket), server.Assert().GetImagePathHelper(), _datetimeService.Object);

                result.Should().BeEquivalentTo(expectedTicketDto, config => config.Excluding(x => x.Attachments).Excluding(x => x.Assets).Excluding(x => x.Tasks).Excluding(x => x.Category));
            }
        }

        [Fact]
        public async Task ReporterDoesNotExist_UpdateTicket_ReporterIsCreated()
        {
            var utcNow = DateTime.UtcNow;
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketTemplateRequest>()
                                 .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                 .With(x => x.ReporterId, (Guid?)null)
                                 .With(x => x.Recurrence, _event1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .Create();
            var existingTicket = Fixture.Build<TicketTemplateEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Assets)
                                        .Without(x => x.Tasks)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.OverdueThreshold, "3;3")
                                        .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.UpdatedDate, utcNow)
                                        .With(x => x.CategoryName, "Chevy")
                                        .With(x => x.CategoryId, request.CategoryId)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketTemplates.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickettemplate/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var reporter = db.Reporters.First();
                db.TicketTemplates.Should().HaveCount(1);
                var udpatedTicket = db.TicketTemplates.First();
                udpatedTicket.ReporterId.Should().Be(reporter.Id);
            }
        }


        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate = "2021-01-14T00:00:00",
            Occurs = Event.Recurrence.Monthly,
            Timezone = "Pacific Standard Time",
            DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Wednesday
                }
            }
        };

        #endregion
    }
}
