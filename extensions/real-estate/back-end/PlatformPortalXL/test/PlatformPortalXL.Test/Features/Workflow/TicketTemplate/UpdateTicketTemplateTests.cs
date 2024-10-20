using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Workflow;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using Willow.Calendar;
using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.TicketTemplates
{
    public class UpdateTicketTemplateTests : BaseInMemoryTest
    {
        private readonly UpdateTicketTemplateRequest _defaultRequest = new UpdateTicketTemplateRequest
        {
            Priority      = 1,
            Summary       = "test",
            Description   = "test",
            ReporterId    = Guid.NewGuid(),
            ReporterName  = "bob",
            ReporterEmail = "bob@bob.com",
            ReporterPhone = "555-555-1212",
            Recurrence    = new EventDto
            {
                StartDate = "2021-01-01T00:00:00",
                Occurs = EventDto.Recurrence.Daily,
            },
            OverdueThreshold = new Duration
            {
                UnitOfMeasure = Duration.DurationUnit.Day,
                Units = 1
            }
        };

        public UpdateTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateTicketTemplate_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var ticketTemplateId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickettemplate/{ticketTemplateId}", _defaultRequest);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateTicketTemplate_ReturnsUpdatedTicketTemplate()
        {
            var site = Fixture.Create<Site>();
            var ticketTemplateId = Guid.NewGuid();
            var expectedTicketTemplate = Fixture.Create<TicketTemplate>();

            expectedTicketTemplate.Recurrence = _event1;

            var request = Fixture.Build<WorkflowUpdateTicketTemplateRequest>()
                .With(x => x.ReporterPhone, "+1234567890")
                .With(x => x.ReporterEmail, "test@site.com")
                .With(x => x.Recurrence, _event1)
                .Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi().SetupRequest(HttpMethod.Get, $"sites/{site.Id}").ReturnsJson(site);

                server.Arrange().GetWorkflowApi()
                                .SetupRequest(HttpMethod.Put, $"sites/{site.Id}/tickettemplate/{ticketTemplateId}")
                                .ReturnsJson(expectedTicketTemplate);

                var response = await client.PutAsJsonAsync($"sites/{site.Id}/tickettemplate/{ticketTemplateId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                result.Should().BeEquivalentTo(TicketTemplateDto.MapFromModel(expectedTicketTemplate, server.Assert().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task InvalidRequest_UpdateTicketTemplate_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            var ticketTemplateId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickettemplate/{ticketTemplateId}", new UpdateTicketTemplateRequest { });

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain($"Summary is required");
                result.Should().Contain($"Description is required");
                result.Should().Contain($"Requestor name is required");
                result.Should().Contain($"Contact number is required");
                result.Should().Contain($"Contact email is required");
                result.Should().Contain($"Recurrence is required");
                result.Should().Contain($"OverdueThreshold is required");
            }
        }
        
        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate      = (new DateTime(2021, 1, 14, 0, 0, 0, DateTimeKind.Local)).ToString("O"),
            Occurs         = EventDto.Recurrence.Monthly,
            Timezone       = "Pacific Standard Time",
            DayOccurrences = new List<EventDto.DayOccurrence>
            {
                new EventDto.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Wednesday
                }
            }
        };

        #endregion        

    }
}
