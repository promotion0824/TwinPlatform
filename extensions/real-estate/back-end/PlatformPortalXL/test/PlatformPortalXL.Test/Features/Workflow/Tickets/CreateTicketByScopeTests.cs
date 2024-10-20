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
using Moq.Contrib.HttpClient;
using System.Globalization;
using SixLabors.ImageSharp;
using System.IO;
using System.Linq;
using PlatformPortalXL.Features.Pilot;
using SixLabors.ImageSharp.PixelFormats;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;
using Willow.Platform.Users;
using Site = Willow.Platform.Models.Site;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class CreateTicketByScopeTests : BaseInMemoryTest
    {
        public CreateTicketByScopeTests(ITestOutputHelper output) : base(output)
        {
        }

        private byte[] GetTestImageBytes()
        {
            var image = new Image<Rgba32>(10, 20);
            using (var stream = new MemoryStream())
            {
                image.SaveAsJpeg(stream);
                return stream.ToArray();
            }
        }

        [Fact]
        public async Task InvalidContactNumber_CreateTicket_ReturnError()
        {
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId=Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "invalid phone",
                ReporterEmail = "test+123@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };
            var userId = Guid.NewGuid();
            var createdTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .With(x => x.SiteId, siteId)
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
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsync($"tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Contact number is invalid");
            }
        }

        [Fact]
        public async Task ValidInput_CreateTicket_ReturnsCreatedTicket()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "1234567890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null,
                Latitude = 176.8954M,
                Longitude = -23.0812M
            };
            var createdTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .Without(x=>x.InsightId)
                                       .Without(x => x.Creator)
									   .With(x => x.SiteId, siteId)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .With(x => x.SourceType, TicketSourceType.Platform)
                                       .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                       .With(x => x.Latitude, request.Latitude)
                                       .With(x => x.Longitude, request.Longitude)
                                       .Create();

            var user = Fixture.Build<User>().With(c=>c.Id,createdTicket.CreatorId).Create();
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
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
	                .ReturnsJson(user);
                var response = await client.PostAsync($"tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                createdTicket.Creator = user.ToCreator();
                var expectedResult = TicketDetailDto.MapFromModel(createdTicket, server.Assert().GetImageUrlHelper());
                result.Should().BeEquivalentTo(expectedResult);
            }
        }
		[Fact]
		public async Task ValidInput_CreateTicket_WithInsightId_ReturnsCreatedTicket()
		{
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
				Priority = 1,
				InsightId = Guid.NewGuid(),
				IssueType = TicketIssueType.NoIssue,
				IssueId = null,
				Summary = Guid.NewGuid().ToString(),
				Description = Guid.NewGuid().ToString(),
				Cause = string.Empty,
				ReporterId = Guid.NewGuid(),
				ReporterName = Guid.NewGuid().ToString(),
				ReporterPhone = "1234567890",
				ReporterEmail = "email@site.com",
				ReporterCompany = string.Empty,
				AssigneeType = (int)TicketAssigneeType.NoAssignee,
				AssigneeId = null,
				DueDate = null,
				Latitude = 176.8954M,
				Longitude = -23.0812M
			};
			var expectedInsight = new Insight
			{
				Id = request.InsightId.Value,
				TwinId = Guid.NewGuid().ToString(),
				Name = Fixture.Build<string>().Create(),
				LastStatus = InsightStatus.InProgress
			};

			var createdTicket = Fixture.Build<Ticket>()
									   .Without(x => x.Assignee)
									   .Without(x => x.Creator)
									   .With(x => x.InsightId,request.InsightId.Value)
									   .With(x => x.SiteId, siteId)
									   .With(x => x.Comments, new List<Comment>())
									   .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
									   .With(x => x.SourceType, TicketSourceType.Platform)
									   .With(x => x.SourceName, TicketSourceType.Platform.ToString())
									   .With(x => x.Latitude, request.Latitude)
									   .With(x => x.Longitude, request.Longitude)
									   .Create();
			var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

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
					.SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
					.ReturnsJson(createdTicket);
				server.Arrange().GetMarketPlaceApi()
					.SetupRequest(HttpMethod.Post, $"messages")
					.ReturnsResponse(HttpStatusCode.NoContent);
				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{request.InsightId}")
					.ReturnsJson(expectedInsight );

				server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
					.ReturnsJson(user);
				var response = await client.PostAsync($"tickets", GetMultipartContent(request));


				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                createdTicket.Creator = user.ToCreator();
				var expectedResult = TicketDetailDto.MapFromModel(createdTicket, server.Assert().GetImageUrlHelper());
				result.Should().BeEquivalentTo(expectedResult);
			}
		}

		[Theory]
		[InlineData(InsightStatus.Resolved)]
		[InlineData(InsightStatus.Ignored)]
		public async Task ValidInput_CreateTicket_WithInsightId_InvalidInsightStatus_ReturnsValidationError(InsightStatus insightStatus)
		{
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
				Priority = 1,
				InsightId = Guid.NewGuid(),
				IssueType = TicketIssueType.NoIssue,
				IssueId = null,
				Summary = Guid.NewGuid().ToString(),
				Description = Guid.NewGuid().ToString(),
				Cause = string.Empty,
				ReporterId = Guid.NewGuid(),
				ReporterName = Guid.NewGuid().ToString(),
				ReporterPhone = "1234567890",
				ReporterEmail = "email@site.com",
				ReporterCompany = string.Empty,
				AssigneeType = (int)TicketAssigneeType.NoAssignee,
				AssigneeId = null,
				DueDate = null,
				Latitude = 176.8954M,
				Longitude = -23.0812M
			};
			var expectedInsight = new Insight
			{
				Id = request.InsightId.Value,
				TwinId = Fixture.Build<string>().Create(),
				Name = Fixture.Build<string>().Create(),
				LastStatus = insightStatus
			};

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

                server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"sites/{siteId}/insights/{request.InsightId}")
					.ReturnsJson(expectedInsight);

				var response = await client.PostAsync($"tickets", GetMultipartContent(request));


				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Name.Should().Be("insightStatus");
			}
		}
		[Fact]
        public async Task ValidInputWithImageAttachment_CreateTicket_ReturnsCreatedTicketWithAttachments()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "1234567890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };
            var createdTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Attachments)
                                       .Without(x => x.Assignee)
                                       .Without(x => x.Creator)
									   .Without(x => x.InsightId)
									   .With(x => x.SiteId, siteId)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();
            var createdAttachmentCount = 0;
            var attachmentBytes = GetTestImageBytes();
            var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

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
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets/{createdTicket.Id}/attachments", message =>
                    {
                        createdAttachmentCount++;
                        return Task.FromResult(true);
                    })
                    .ReturnsJson(Fixture.Create<Attachment>());
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/tickets/{createdTicket.Id}?includeAttachments=True&includeComments=True")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
	                .ReturnsJson(user);
				var requestContent = GetMultipartContent(request);
                requestContent.Add(
                    new ByteArrayContent(attachmentBytes) { Headers = { ContentLength = attachmentBytes.Length } },
                    "attachmentFiles",
                    "abc1.jpg");
                var response = await client.PostAsync($"tickets", requestContent);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
                createdAttachmentCount.Should().Be(1);
            }
        }

        [Fact]
        public async Task ValidInputWithValidImageAttachment_CreateTicket_ReturnsValidationError()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "1234567890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };

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
                var requestContent = GetMultipartContent(request);
                requestContent.Add(
                    new ByteArrayContent(new byte[10]) { Headers = { ContentLength = 10 } },
                    "attachmentFiles",
                    "abc1.jpg");
                var response = await client.PostAsync($"tickets", requestContent);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Name.Should().Be("attachmentFiles");
            }
        }

        [Theory]
        [InlineData(TicketIssueType.Equipment)]
        [InlineData(TicketIssueType.Asset)]
        public async Task TicketIssueIsSpecified_CreateTicket_IssueNameIsSetCorrectly(TicketIssueType issueType)
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = issueType,
                IssueId = Guid.NewGuid(),
                Summary = "summary-abc",
                Description = "desc",
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = "name",
                ReporterPhone = "1234567890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };
            var createdTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .Without(x => x.InsightId)
                                       .Without(x => x.Creator)
									   .With(x => x.SiteId, siteId)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();
            var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

			var expectedIssueName = string.Empty;
            var actualIssueName = string.Empty;

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
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets", async message =>
                    {
                        var requestToWorkflow = await message.Content.ReadAsAsync<WorkflowCreateTicketRequest>();
                        actualIssueName = requestToWorkflow.IssueName;
                        return true;
                    })
                    .ReturnsJson(createdTicket);

                var asset = Fixture.Build<DigitalTwinAsset>().With(x => x.Id, request.IssueId.Value).Create();
                expectedIssueName = asset.Name;
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}/assets/{request.IssueId.Value}")
                    .ReturnsJson(asset);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
	                .ReturnsJson(user);
				var response = await client.PostAsync($"sites/{siteId}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                actualIssueName.Should().Be(expectedIssueName);
            }
        }

        [Fact]
        public async Task InvalidInput_CreateTicket_ReturnsValidationError()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 0,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = string.Empty,
                Description = string.Empty,
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = string.Empty,
                ReporterPhone = string.Empty,
                ReporterEmail = string.Empty,
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };
            var createdTicket = Fixture.Create<Ticket>();

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
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
                    .ReturnsJson(createdTicket);

                var response = await client.PostAsync($"sites/{siteId}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(5);
            }
        }

        [Fact]
        public async Task ExceptionThrown_CreateTicket_ReturnsBadRequest()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;
            var expectedTwinDto = userSites.Select(x => Fixture.Build<TwinDto>().With(y => y.SiteId, x.Id).Create()).ToList();
            expectedTwinDto[0].SiteId = siteId;
            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = Guid.NewGuid().ToString(),
                ReporterPhone = "1234567890",
                ReporterEmail = "email@site.com",
                ReporterCompany = string.Empty,
                AssigneeType = (int)TicketAssigneeType.NoAssignee,
                AssigneeId = null,
                DueDate = null
            };

            var createdTicket = Fixture.Create<Ticket>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(userId, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={Permissions.ViewSites}")
                    .ReturnsJson(userSites);
                server.Arrange().GetDigitalTwinApi().SetupRequest(HttpMethod.Post, $"scopes/sites")
                    .ReturnsJson(expectedTwinDto);

                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{siteId}")
                    .Throws(new Exception("This is a test error"));
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{siteId}/tickets")
                    .ReturnsJson(createdTicket);

                var response = await client.PostAsync($"sites/{siteId}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
                var result = await response.Content.ReadAsStringAsync();

                result.Should().NotContain("SiteApiService.cs:line");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateTicket_ReturnsForbidden()
        {
            var userId = Guid.NewGuid();
            var userSites = Fixture.Build<Site>()
                .With(x => x.Features, new SiteFeatures { IsTicketingDisabled = false })
                .CreateMany(2).ToList();
            var siteId = userSites[0].Id;

            var request = new CreateTicketByScopeRequest()
            {
                ScopeId = Guid.NewGuid().ToString(),
                FloorCode = string.Empty,
                Priority = 1,
                IssueType = TicketIssueType.NoIssue,
                IssueId = null,
                Summary = "bob",
                Description = "bob",
                Cause = string.Empty,
                ReporterId = Guid.NewGuid(),
                ReporterName = "bob",
                ReporterPhone = "5555551212",
                ReporterEmail = "bob@bob.bob",
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
                var response = await client.PostAsync($"tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private MultipartFormDataContent GetMultipartContent(CreateTicketByScopeRequest createTicketRequest)
        {
            var dataContent = new MultipartFormDataContent();
            dataContent.Add(new StringContent(createTicketRequest.ScopeId), "ScopeId");
            dataContent.Add(new StringContent(createTicketRequest.FloorCode ?? string.Empty), "FloorCode");
            dataContent.Add(new StringContent(createTicketRequest.Priority.ToString()), "Priority");
            dataContent.Add(new StringContent(createTicketRequest.IssueType.ToString()), "IssueType");
            if (createTicketRequest.IssueId.HasValue)
            {
                dataContent.Add(new StringContent(createTicketRequest.IssueId.Value.ToString()), "IssueId");
            }
            if (createTicketRequest.InsightId.HasValue)
            {
                dataContent.Add(new StringContent(createTicketRequest.InsightId.Value.ToString()), "InsightId");
            }
            dataContent.Add(new StringContent(createTicketRequest.Summary ?? string.Empty), "Summary");
            dataContent.Add(new StringContent(createTicketRequest.Description ?? string.Empty), "Description");
            dataContent.Add(new StringContent(createTicketRequest.Cause ?? string.Empty), "Cause");
            if (createTicketRequest.ReporterId.HasValue)
            {
                dataContent.Add(new StringContent(createTicketRequest.ReporterId.Value.ToString()), "ReporterId");
            }
            dataContent.Add(new StringContent(createTicketRequest.ReporterName ?? string.Empty), "ReporterName");
            dataContent.Add(new StringContent(createTicketRequest.ReporterPhone ?? string.Empty), "ReporterPhone");
            dataContent.Add(new StringContent(createTicketRequest.ReporterEmail ?? string.Empty), "ReporterEmail");
            dataContent.Add(new StringContent(createTicketRequest.ReporterCompany ?? string.Empty), "ReporterCompany");
            dataContent.Add(new StringContent(createTicketRequest.AssigneeType.ToString()), "AssigneeType");
            if (createTicketRequest.AssigneeId.HasValue)
            {
                dataContent.Add(new StringContent(createTicketRequest.AssigneeId.Value.ToString()), "AssigneeId");
            }
            if (createTicketRequest.DueDate.HasValue)
            {
                dataContent.Add(new StringContent(createTicketRequest.DueDate.Value.ToString("o", CultureInfo.InvariantCulture)), "DueDate");
            }
            return dataContent;
        }
    }
}
