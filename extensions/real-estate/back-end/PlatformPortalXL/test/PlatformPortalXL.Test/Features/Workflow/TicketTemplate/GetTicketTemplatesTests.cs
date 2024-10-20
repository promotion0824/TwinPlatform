using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Willow.Calendar;
using Willow.Tests.Infrastructure;
using Willow.Workflow;

using Xunit;
using Xunit.Abstractions;

namespace PlatformPortalXL.Test.Features.Workflow.TicketTemplates
{
    public class GetTicketTemplatesTests : BaseInMemoryTest
    {
        public GetTicketTemplatesTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetTicketTemplates_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.GetAsync($"sites/{siteId}/tickettemplate");

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task TicketTemplatesExist_GetTicketTemplates_ReturnThoseTicketTemplates()
        {
            var siteId = Guid.NewGuid();
            var expectedTicketTemplates = Fixture.Build<TicketTemplate>()
                .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                .Without(x => x.Assignee)
                .Without(x => x.AssigneeId)
                .With(x => x.Id,                Guid.NewGuid())
                .With(x => x.CustomerId,        Guid.NewGuid())
                .With(x => x.SiteId,            Guid.NewGuid())
                .With(x => x.FloorCode,         "123")
                .With(x => x.SequenceNumber,    "123")
                .With(x => x.Priority,          5)
                .With(x => x.Status,            TicketStatus.Open)
                .With(x => x.Summary,           "Very cool")
                .With(x => x.Description,       "This is a cool template")

                .With(x => x.ReporterId,        Guid.NewGuid())
                .With(x => x.ReporterName,      "Bob Jones")
                .With(x => x.ReporterPhone,     "555-5555")
                .With(x => x.ReporterEmail,     "bob@nowhere.com")
                .With(x => x.ReporterCompany,   "Acme Widgets")
                                              
                //.With(x => x.AssigneeType,      TicketAssigneeType.CustomerUser)
                //.With(x => x.AssigneeId,        Guid.NewGuid())
        
                .With(x => x.CreatedDate,       new DateTime(2021, 4, 20, 0, 0, 0, DateTimeKind.Utc))
                .With(x => x.UpdatedDate,       new DateTime(2021, 4, 20, 0, 0, 0, DateTimeKind.Utc))    
                .With(x => x.ClosedDate,        (DateTime?)null)

                .With(x => x.SourceType,        TicketSourceType.Platform)
                .With(x => x.Recurrence,        _event1)
                .With(x => x.NextTicketDate,    new DateTime(2021, 4, 20, 0, 0, 0, DateTimeKind.Unspecified).ToString("s"))
                .With(x => x.OverdueThreshold,  new Duration("3;3"))
                .With(x => x.CategoryId,        Guid.NewGuid())
                .With(x => x.Category,          "Chevy")

                .With(x => x.DataValue, new DataValue { Type = TicketTemplateDataType.Numeric, DecimalPlaces = 0, MinValue = 1, MaxValue = 2, EnumerationNameList = string.Empty, Value = "1.25"})
                .With(x => x.Tasks,             new List<TicketTaskTemplate> { new TicketTaskTemplate { Description = "Numeric Task", Type = TicketTaskType.Numeric, DecimalPlaces = 2, MaxValue = 1000, MinValue = 1, Unit = "psi" },
                                                                               new TicketTaskTemplate { Description = "Check Task" }})
                .With(x => x.Assets,            new List<TicketAsset> { new TicketAsset { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), AssetName = "Frank"},
                                                                        new TicketAsset { Id = Guid.NewGuid(), AssetId = Guid.NewGuid(), AssetName = "James"}} )
                .CreateMany(10)
                .ToList();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickettemplate")
                    .ReturnsJson(expectedTicketTemplates);

                var response = await client.GetAsync($"sites/{siteId}/tickettemplate");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketTemplateDto>>();
                result.Should().BeEquivalentTo(TicketTemplateDto.MapFromModels(expectedTicketTemplates, server.Assert().GetImageUrlHelper()));
            }
        }

        #region Sample Events

        private static EventDto _event1 = new EventDto
        {
            StartDate = new DateTime(2021, 1, 14, 0, 0, 0, DateTimeKind.Unspecified).ToString("O"),
            Occurs         = EventDto.Recurrence.Monthly,
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
