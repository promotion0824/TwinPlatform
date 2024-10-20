using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Features.Workflow;
using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Calendar;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using PlatformPortalXL.Dto;
using System.Linq;
using System.Collections.Generic;

using Willow.Platform.Models;
using Willow.Workflow;

namespace PlatformPortalXL.Test.Features.Workflow.TicketTemplates
{
    public class CreateTicketTemplateTests : BaseInMemoryTest
    {
        private readonly CreateTicketTemplateRequest _defaultRequest = new CreateTicketTemplateRequest
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

        public CreateTicketTemplateTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateTicketTemplate_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", _defaultRequest);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task InvalidStarDate_CreateTicketTemplate_ReturnsUnprocessableEntity()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", new CreateTicketTemplateRequest
                {
                    Recurrence = new EventDto
                    {
                        StartDate = new DateTime(2021, 04, 20, 0, 0, 0, DateTimeKind.Unspecified).ToString("s"),
                        EndDate = new DateTime(2021, 04, 20, 0, 0, 0, DateTimeKind.Unspecified).ToString("s"),
                        Timezone = "Dateline Standard Time"
                    }
                });

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain($"Summary is required");
                result.Should().Contain($"Description is required");
                result.Should().Contain($"Requestor name is required");
                result.Should().Contain($"Contact number is required");
                result.Should().Contain($"Contact email is required");
                result.Should().Contain($"OverdueThreshold is required");
            }
        }

        [Fact]
        public async Task CreateTicketTemplate_Success()
        {
            var siteId = Guid.NewGuid();
            var expectedTicketTemplate = new TicketTemplate 
            {
                CustomerId  = _defaultRequest.CustomerId,
                Summary     = _defaultRequest.Summary,
                Description = _defaultRequest.Description,
                SiteId      = siteId
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickettemplate")
                    .ReturnsJson(expectedTicketTemplate);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, CustomerId = _defaultRequest.CustomerId, Name = "Bedrock" } );

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", _defaultRequest);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketTemplateDto>();
                
                Assert.Equal(_defaultRequest.CustomerId,  result.CustomerId);
                Assert.Equal(_defaultRequest.Summary,     result.Summary);
                Assert.Equal(_defaultRequest.Description, result.Description);
                Assert.Equal(siteId,                      result.SiteId);
            }
        }

        [Fact]
        public async Task InvalidStarDate_CreateTicketTemplate_ReturnsUnprocessableEntity_taskname_too_long()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, siteId))
            {
                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickettemplate", new CreateTicketTemplateRequest
                {
                    Recurrence = new EventDto
                    {
                        StartDate = new DateTime(2021, 04, 20, 0, 0, 0, DateTimeKind.Unspecified).ToString("s"),
                        EndDate = new DateTime(2021, 04, 20, 0, 0, 0, DateTimeKind.Unspecified).ToString("s"),
                        Timezone = "Dateline Standard Time"
                    },
                    Tasks = new List<TicketTaskTemplate>
                    {
                        new TicketTaskTemplate
                        {
                            Description = "TaskName1"
                        },
                        new TicketTaskTemplate
                        {
                            Description ="TaskName2_".PadRight(320, 'A')
                        }
                    }
                });

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsStringAsync();
                result.Should().Contain($"Summary is required");
                result.Should().Contain($"Description is required");
                result.Should().Contain($"Requestor name is required");
                result.Should().Contain($"Contact number is required");
                result.Should().Contain($"Contact email is required");
                result.Should().Contain($"OverdueThreshold is required");
                result.Should().Contain($"Task name length cannot exceed 300");
            }
        }

        [Fact]
        public async Task InvalidRequest_CreateTicketTemplate_ReturnsUnprocessableEntity()
        {
            var site = Fixture.Create<Site>();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ManageSites, site.Id))
            {
                server.Arrange().GetSiteApi().SetupRequest(HttpMethod.Get, $"sites/{site.Id}").ReturnsJson(site);

                var response = await client.PostAsJsonAsync($"sites/{site.Id}/tickettemplate", new CreateTicketTemplateRequest { });

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
    }
}
