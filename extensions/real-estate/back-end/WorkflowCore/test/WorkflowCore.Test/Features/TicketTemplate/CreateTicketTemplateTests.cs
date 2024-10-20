using AutoFixture;
using WorkflowCore.Entities;
using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Models;
using WorkflowCore.Controllers.Request;
using Willow.Calendar;
using Willow.Infrastructure;
using System.Collections.Generic;
using System.Net.Http.Json;
using Moq;
using Willow.Common;

namespace WorkflowCore.Test.Features.TicketTemplates
{
    public class CreateTicketTemplateTests : BaseInMemoryTest
    {
        private readonly Mock<IDateTimeService> _datetimeService = new Mock<IDateTimeService>();

        public CreateTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate", new CreateTicketTemplateRequest());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenValidInput_CreateTicket_ReturnsCreatedTicket()
        {
            var utcNow = DateTime.UtcNow;

            _datetimeService.Setup(w => w.UtcNow).Returns(utcNow);

            var siteId = Guid.NewGuid();
            var assets = Fixture.Build<TicketAsset>().CreateMany(3).ToList();
            var tasks = Fixture.Build<TicketTaskTemplate>().CreateMany(3).ToList();
            var dataValue = Fixture.Create<DataValue>();
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                            .Without(x => x.AssigneeId)
                            .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x => x.Recurrence, _event1)
                            .With(x => x.Assets, assets)
                            .With(x => x.Tasks, tasks)
                            .With(x => x.DataValue, dataValue)
                            .With(x => x.OverdueThreshold, new Duration("1;1"))
                            .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.TicketTemplates.Should().HaveCount(1);
                var createdTicket = db.TicketTemplates.First();
                createdTicket.Should().NotBeNull();
                createdTicket.Id.Should().NotBe(Guid.Empty);
                createdTicket.CustomerId.Should().Be(request.CustomerId);
                createdTicket.SiteId.Should().Be(siteId);
                createdTicket.FloorCode.Should().Be(request.FloorCode);
                createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
                createdTicket.Priority.Should().Be(request.Priority);
                createdTicket.Status.Should().Be(request.Status);
                createdTicket.Description.Should().Be(request.Description);
                createdTicket.ReporterId.Should().Be(request.ReporterId);
                createdTicket.ReporterName.Should().Be(request.ReporterName);
                createdTicket.ReporterPhone.Should().Be(request.ReporterPhone);
                createdTicket.ReporterEmail.Should().Be(request.ReporterEmail);
                createdTicket.ReporterCompany.Should().Be(request.ReporterCompany);
                createdTicket.AssigneeType.Should().Be(request.AssigneeType);
                createdTicket.AssigneeId.Should().Be(request.AssigneeId);
                createdTicket.CreatedDate.Should().Be(utcNow);
                createdTicket.UpdatedDate.Should().Be(utcNow);
                createdTicket.ClosedDate.Should().BeNull();
                createdTicket.SourceType.Should().Be(request.SourceType);

                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                var expectedTicket = TicketTemplateEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = null; // new List<Attachment>();
                expectedTicket.Assets = assets;
                expectedTicket.Tasks = tasks;
                expectedTicket.DataValue = dataValue;

                var expectedTicketDto = TicketTemplateDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper(), _datetimeService.Object);
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        [Fact]
        public async Task ReporterDoesNotExist_CreateTicket_ReporterIsCreated()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.ReporterId, (Guid?)null)
                                 .With(x => x.Recurrence, _event1)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var reporter = db.Reporters.First();
                db.TicketTemplates.Should().HaveCount(1);
                var createdTicket = db.TicketTemplates.First();
                createdTicket.ReporterId.Should().Be(reporter.Id);
            }
        }

        [Fact]
        public async Task ThereIsNoTicketForGivenSite_CreateTicket_ReturnsTicketWithFirstSequenceNumber()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Recurrence, _event1)
                                 .Create();
            var expectedSequenceNumber = $"{request.SequenceNumberPrefix}-S-1";

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketTemplates.Should().HaveCount(1);
                var createdTicket = db.TicketTemplates.First();
                createdTicket.SequenceNumber.Should().Be(expectedSequenceNumber);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.SequenceNumber.Should().Be(expectedSequenceNumber);
            }
        }

        [Fact]
        public async Task ThereAreTicketsForGivenSite_CreateTicket_ReturnsTicketWithNextSequenceNumber()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Recurrence, _event1)
                                 .Create();

            var expectedSequenceNumber = $"{request.SequenceNumberPrefix}-S-1";

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketTemplates.Should().HaveCount(1);
                var createdTicket = db.TicketTemplates.First();
                createdTicket.SequenceNumber.Should().Be(expectedSequenceNumber);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.SequenceNumber.Should().Be(expectedSequenceNumber);
            }
        }

        [Fact]
        public async Task SequenceNumberPrefixIsNotProvided_CreateTicket_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.SequenceNumberPrefix, string.Empty)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain(nameof(CreateTicketTemplateRequest.SequenceNumberPrefix));
            }
        }

        [Fact]
        public async Task GivenNoAssigneeTypeAndAssigneeId_CreateTicketTemplate_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenAssigneeTypeAndNoAssigneeId_CreateTicketTemplate_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketTemplateRequest>()
                .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                .Without(x => x.AssigneeId)
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickettemplate", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate      = "2021-01-14T00:00:00",
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

        #endregion        
    }
}
