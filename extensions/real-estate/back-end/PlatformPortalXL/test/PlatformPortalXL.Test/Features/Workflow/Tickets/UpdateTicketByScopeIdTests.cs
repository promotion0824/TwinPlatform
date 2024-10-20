using AutoFixture;
using FluentAssertions;
using PlatformPortalXL.Models;
using PlatformPortalXL.Dto;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq.Contrib.HttpClient;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;
using Willow.Platform.Users;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.IO;
using PlatformPortalXL.Features.Pilot;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class UpdateTicketByScopeIdTests : BaseInMemoryTest
    {
        public UpdateTicketByScopeIdTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task ValidInput_UpdateTicket_ReturnsUpdatedTicket()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new UpdateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                Priority = 1,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = Guid.NewGuid().ToString(),
                Solution = Guid.NewGuid().ToString(),
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "+1(23)456-7890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null,
                AttachmentIds = null,
                StatusCode = (int)TicketStatus.InProgress,
                Latitude = 176.8954M,
                Longitude = -23.0812M,
                IssueId = null,
                IssueType = TicketIssueType.Asset
            };
            var updatedTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .Without(x => x.AssigneeId)
                                       .Without(x=>x.TwinId)
                                       .Without(x => x.Creator)
									   .With(x => x.Attachments, new List<Attachment>())
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .With(x => x.SourceType, TicketSourceType.Platform)
                                       .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                       .With(x => x.Latitude, request.Latitude)
                                       .With(x => x.Longitude, request.Longitude)
                                       .Create();

			var user = Fixture.Build<User>().With(c => c.Id, updatedTicket.CreatorId).Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(userSites[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{updatedTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/tickets/{updatedTicket.Id}")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{updatedTicket.CreatorId}")
	                .ReturnsJson(user);
				var response = await client.PutAsync($"tickets/{updatedTicket.Id}", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                updatedTicket.Creator = user.ToCreator();
                var expectedResult = TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper());
                expectedResult.SourceName = $"{TicketSourceType.Platform}";
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateTicket_AttachmentIdsIsNull_NoCallToDeleteAttachment_ReturnsUpdatedTicket()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            var request = new UpdateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                Priority = 1,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = Guid.NewGuid().ToString(),
                Solution = Guid.NewGuid().ToString(),
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "+1(23)456-7890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null,
                AttachmentIds = null,
                StatusCode = (int)TicketStatus.InProgress,
                Latitude = 176.8954M,
                Longitude = -23.0812M
            };
            var expectedAttachments = Fixture.Build<Attachment>().CreateMany(2).ToList();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .Without(x => x.AssigneeId)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.Creator)
                                       .With(x => x.Attachments, expectedAttachments)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .With(x => x.SourceType, TicketSourceType.Platform)
                                       .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                       .With(x => x.Latitude, request.Latitude)
                                       .With(x => x.Longitude, request.Longitude)
                                       .Create();

            var user = Fixture.Build<User>().With(c => c.Id, updatedTicket.CreatorId).Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(userSites[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{updatedTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/tickets/{updatedTicket.Id}")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{updatedTicket.CreatorId}")
                    .ReturnsJson(user);
                var response = await client.PutAsync($"tickets/{updatedTicket.Id}", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                updatedTicket.Creator = user.ToCreator();
                var expectedResult = TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper());
                expectedResult.SourceName = $"{TicketSourceType.Platform}";
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task ValidInput_UpdateTicket_AttachmentIdsIsEmpty_DeleteAttachmentGetCalled_ReturnsUpdatedTicket()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            var request = new UpdateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                Priority = 1,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = Guid.NewGuid().ToString(),
                Solution = Guid.NewGuid().ToString(),
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "+1(23)456-7890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null,
                AttachmentIds = new List<Guid>(){Guid.Empty},
                StatusCode = (int)TicketStatus.InProgress,
                Latitude = 176.8954M,
                Longitude = -23.0812M
            };
            var expectedAttachments = Fixture.Build<Attachment>().CreateMany(2).ToList();
            var updatedTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .Without(x => x.AssigneeId)
                                       .Without(x => x.TwinId)
                                       .Without(x => x.Creator)
                                       .With(x => x.Attachments, expectedAttachments)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .With(x => x.SourceType, TicketSourceType.Platform)
                                       .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                       .With(x => x.Latitude, request.Latitude)
                                       .With(x => x.Longitude, request.Longitude)
                                       .Create();

            var user = Fixture.Build<User>().With(c => c.Id, updatedTicket.CreatorId).Create();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete,
                        $"sites/{siteId}/tickets/{updatedTicket.Id}/attachments/{expectedAttachments[0].Id}").ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Delete,
                        $"sites/{siteId}/tickets/{updatedTicket.Id}/attachments/{expectedAttachments[1].Id}").ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(userSites[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{updatedTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{updatedTicket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/tickets/{updatedTicket.Id}")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{updatedTicket.CreatorId}")
                    .ReturnsJson(user);

                var response = await client.PutAsync($"tickets/{updatedTicket.Id}", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                updatedTicket.Creator = user.ToCreator();
                var expectedResult = TicketDetailDto.MapFromModel(updatedTicket, server.Assert().GetImageUrlHelper());
                expectedResult.SourceName = $"{TicketSourceType.Platform}";
                result.Should().BeEquivalentTo(expectedResult);
            }
        }

        [Fact]
        public async Task InvalidPhoneNumber_UpdateTicket_ReturnsError()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            var request = new UpdateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                Priority = 1,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = Guid.NewGuid().ToString(),
                Solution = Guid.NewGuid().ToString(),
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterEmail = "test@site.com",
                ReporterPhone = "invalid phone",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null,
                AttachmentIds = null,
                StatusCode = (int)TicketStatus.Open
            };
            var updatedTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .With(x => x.Attachments, new List<Attachment>())
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(userSites[0]);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{updatedTicket.Id}?includeAttachments=True&includeComments=False")
                    .ReturnsJson(updatedTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Put, $"sites/{siteId}/tickets/{updatedTicket.Id}")
                    .ReturnsJson(updatedTicket);

                var response = await client.PutAsync($"tickets/{updatedTicket.Id}", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Contact number is invalid");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UpdateTicket_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var request = new UpdateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                Priority = 1,
                Summary = "bob",
                Description = "bob",
                StatusCode = (int)TicketStatus.Open,
                Notes = string.Empty,
                Cause = "cause",
                Solution = "solution",
                ReporterId = Guid.NewGuid(),
                ReporterName = "bob",
                ReporterEmail = "bob@bob.bob",
                ReporterPhone = "5555551212",
                ReporterCompany = "bob",
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithDeniedPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(new List<TwinDto>());

                var response = await client.PutAsync($"tickets/{Guid.NewGuid()}", GetMultipartContent(request));
                var sresult = response.Content.ReadAsStringAsync();
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private MultipartFormDataContent GetMultipartContent(UpdateTicketByScopeRequest updateTicketRequest)
        {

            var dataContent = new MultipartFormDataContent();
            dataContent.Add(new StringContent(updateTicketRequest.ScopeId), "ScopeId");
            dataContent.Add(new StringContent(updateTicketRequest.Priority.ToString()), "Priority");
            dataContent.Add(new StringContent(updateTicketRequest.Summary ?? string.Empty), "Summary");
            dataContent.Add(new StringContent(updateTicketRequest.Description ?? string.Empty), "Description");
            dataContent.Add(new StringContent(updateTicketRequest.Cause ?? string.Empty), "Cause");
            dataContent.Add(new StringContent(updateTicketRequest.Solution ?? string.Empty), "Solution");
            if (updateTicketRequest.AttachmentIds!=null)
            {
                updateTicketRequest.AttachmentIds.ForEach(c => dataContent.Add(new StringContent(c.ToString()), "AttachmentIds"));
            }
            if (updateTicketRequest.StatusCode.HasValue)
            {
                dataContent.Add(new StringContent(updateTicketRequest.StatusCode.Value.ToString()), "Status");
            }
            if (updateTicketRequest.ReporterId.HasValue)
            {
                dataContent.Add(new StringContent(updateTicketRequest.ReporterId.Value.ToString()), "ReporterId");
            }
            dataContent.Add(new StringContent(updateTicketRequest.ReporterName ?? string.Empty), "ReporterName");
            dataContent.Add(new StringContent(updateTicketRequest.ReporterPhone ?? string.Empty), "ReporterPhone");
            dataContent.Add(new StringContent(updateTicketRequest.ReporterEmail ?? string.Empty), "ReporterEmail");
            dataContent.Add(new StringContent(updateTicketRequest.ReporterCompany ?? string.Empty), "ReporterCompany");
            dataContent.Add(new StringContent(updateTicketRequest.AssigneeType.ToString()), "AssigneeType");
            if (updateTicketRequest.AssigneeId.HasValue)
            {
                dataContent.Add(new StringContent(updateTicketRequest.AssigneeId.Value.ToString()), "AssigneeId");
            }
            if (updateTicketRequest.DueDate.HasValue)
            {
                dataContent.Add(new StringContent(updateTicketRequest.DueDate.Value.ToString("o", CultureInfo.InvariantCulture)), "DueDate");
            }
            return dataContent;
        }
    }
}
