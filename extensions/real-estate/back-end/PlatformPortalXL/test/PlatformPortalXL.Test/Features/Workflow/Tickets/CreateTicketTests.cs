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
using SixLabors.ImageSharp.PixelFormats;
using PlatformPortalXL.ServicesApi.AssetApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Api.DataValidation;
using Willow.Platform.Models;
using Willow.Workflow;
using System.Security.Policy;
using Willow.Platform.Users;
using Site = Willow.Platform.Models.Site;
using Autodesk.Forge.Model;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Test.Features.Workflow.Tickets
{
    public class CreateTicketTests : BaseInMemoryTest
    {
        public CreateTicketTests(ITestOutputHelper output) : base(output)
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
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
            var createdTicket = Fixture.Build<Ticket>()
                                       .Without(x => x.Assignee)
                                       .With(x => x.SiteId, site.Id)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(1);
                result.Items[0].Message.Should().Be("Contact number is invalid");
            }
        }

        [Fact]
        public async Task ValidInput_CreateTicket_ReturnsCreatedTicket()
        {
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
									   .With(x => x.SiteId, site.Id)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .With(x => x.SourceType, TicketSourceType.Platform)
                                       .With(x => x.SourceName, TicketSourceType.Platform.ToString())
                                       .With(x => x.Latitude, request.Latitude)
                                       .With(x => x.Longitude, request.Longitude)
                                       .Create();

            var user = Fixture.Build<User>().With(c=>c.Id,createdTicket.CreatorId).Create();
			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
	                .ReturnsJson(user);
                var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));

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
			var site = Fixture.Create<Site>();

			var request = new CreateTicketRequest
			{
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
									   .With(x => x.SiteId, site.Id)
									   .With(x => x.Comments, new List<Comment>())
									   .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
									   .With(x => x.SourceType, TicketSourceType.Platform)
									   .With(x => x.SourceName, TicketSourceType.Platform.ToString())
									   .With(x => x.Latitude, request.Latitude)
									   .With(x => x.Longitude, request.Longitude)
									   .Create();
			var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);
				server.Arrange().GetWorkflowApi()
					.SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
					.ReturnsJson(createdTicket);
				server.Arrange().GetMarketPlaceApi()
					.SetupRequest(HttpMethod.Post, $"messages")
					.ReturnsResponse(HttpStatusCode.NoContent);
				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}/insights/{request.InsightId}")
					.ReturnsJson(expectedInsight );
				
				server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
					.ReturnsJson(user);
				var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));


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
			var site = Fixture.Create<Site>();

			var request = new CreateTicketRequest
			{
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
			var expectedInsightTwinId = new InsightTwinIdResponse()
			{
				InsightId = request.InsightId.Value,
				TwinId = Fixture.Build<string>().Create()
			};

			var user = Fixture.Build<User>().With(c => c.Id, Guid.NewGuid).Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
			using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
			{
				server.Arrange().GetSiteApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
					.ReturnsJson(site);


				server.Arrange().GetInsightApi()
					.SetupRequest(HttpMethod.Get, $"sites/{site.Id}/insights/{request.InsightId}")
					.ReturnsJson(expectedInsight);
			
				var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));


				response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
				var result = await response.Content.ReadAsAsync<ValidationError>();
				result.Items.Should().HaveCount(1);
				result.Items[0].Name.Should().Be("insightStatus");
			}
		}
		[Fact]
        public async Task ValidInputWithImageAttachment_CreateTicket_ReturnsCreatedTicketWithAttachments()
        {
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
									   .With(x => x.SiteId, site.Id)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();
            var createdAttachmentCount = 0;
            var attachmentBytes = GetTestImageBytes();
            var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

			using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
                    .ReturnsJson(createdTicket);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets/{createdTicket.Id}/attachments", message =>
                    {
                        createdAttachmentCount++;
                        return Task.FromResult(true);
                    })
                    .ReturnsJson(Fixture.Create<Attachment>());
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/tickets/{createdTicket.Id}?includeAttachments=True&includeComments=True")
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
                var response = await client.PostAsync($"sites/{site.Id}/tickets", requestContent);
 
				response.StatusCode.Should().Be(HttpStatusCode.OK);
                createdAttachmentCount.Should().Be(1);
            }
        }

        [Fact]
        public async Task ValidInputWithValidImageAttachment_CreateTicket_ReturnsValidationError()
        {
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                var requestContent = GetMultipartContent(request);
                requestContent.Add(
                    new ByteArrayContent(new byte[10]) { Headers = { ContentLength = 10 } },
                    "attachmentFiles",
                    "abc1.jpg");
                var response = await client.PostAsync($"sites/{site.Id}/tickets", requestContent);

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
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
									   .With(x => x.SiteId, site.Id)
                                       .With(x => x.Comments, new List<Comment>())
                                       .With(x => x.AssigneeType, TicketAssigneeType.NoAssignee)
                                       .Create();
            var user = Fixture.Build<User>().With(c => c.Id, createdTicket.CreatorId).Create();

			var expectedIssueName = string.Empty;
            var actualIssueName = string.Empty;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                var updatedStatus = string.Empty;
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets", async message =>
                    {
                        var requestToWorkflow = await message.Content.ReadAsAsync<WorkflowCreateTicketRequest>();
                        actualIssueName = requestToWorkflow.IssueName;
                        return true;
                    })
                    .ReturnsJson(createdTicket);
                var asset = Fixture.Build<DigitalTwinAsset>().With(x => x.Id, request.IssueId.Value).Create();
                expectedIssueName = asset.Name;
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}/assets/{request.IssueId.Value}")
                    .ReturnsJson(asset);
                
                server.Arrange().GetMarketPlaceApi()
                    .SetupRequest(HttpMethod.Post, $"messages")
                    .ReturnsResponse(HttpStatusCode.NoContent);
                server.Arrange().GetDirectoryApi().SetupRequest(HttpMethod.Get, $"users/{createdTicket.CreatorId}")
	                .ReturnsJson(user);
				var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                actualIssueName.Should().Be(expectedIssueName);
            }
        }

        [Fact]
        public async Task InvalidInput_CreateTicket_ReturnsValidationError()
        {
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .ReturnsJson(site);
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
                    .ReturnsJson(createdTicket);

                var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var result = await response.Content.ReadAsAsync<ValidationError>();
                result.Items.Should().HaveCount(5);
            }
        }

        [Fact]
        public async Task ExceptionThrown_CreateTicket_ReturnsBadRequest()
        {
            var site = Fixture.Create<Site>();
            var request = new CreateTicketRequest
            {
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
            using (var client = server.CreateClientWithPermissionOnSite(null, Permissions.ViewSites, site.Id))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequest(HttpMethod.Get, $"sites/{site.Id}")
                    .Throws(new Exception("This is a test error"));
                server.Arrange().GetWorkflowApi()
                    .SetupRequest(HttpMethod.Post, $"sites/{site.Id}/tickets")
                    .ReturnsJson(createdTicket);

                var response = await client.PostAsync($"sites/{site.Id}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
                var result = await response.Content.ReadAsStringAsync();
                
                result.Should().NotContain("SiteApiService.cs:line");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateTicket_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            var request = new CreateTicketRequest
            {
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
            using (var client = server.CreateClientWithDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var response = await client.PostAsync($"sites/{siteId}/tickets", GetMultipartContent(request));

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private MultipartFormDataContent GetMultipartContent(CreateTicketRequest createTicketRequest)
        {
            var dataContent = new MultipartFormDataContent();
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
