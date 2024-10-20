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
using Willow.Infrastructure;
using System.Collections.Generic;
using System.Net.Http.Json;
using Moq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class CreateTicketsTests : BaseInMemoryTest
    {
        public CreateTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateTickets_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/batch", new CreateTicketsRequest());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenValidOneTicket_CreateTickets_ReturnsCreatedTicket()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketsRequest>()
                                 .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.Latitude, 123.453259M)
                                 .With(x => x.Latitude, -34.721902M)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(1).ToList())
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var expectedCreatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalCreatedDate : utcNow);
            var expectedUpdatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalUpdatedDate : utcNow);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().SetCurrentDateTime(utcNow);

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, request.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/batch", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.Should().NotBeNull();
                createdTicket.Id.Should().NotBe(Guid.Empty);
                createdTicket.CustomerId.Should().Be(request.CustomerId);
                createdTicket.SiteId.Should().Be(siteId);
                createdTicket.FloorCode.Should().Be(request.Tickets[0].FloorCode);
                createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
                createdTicket.Priority.Should().Be(request.Tickets[0].Priority);
                createdTicket.Status.Should().Be(request.Tickets[0].Status);
                createdTicket.IssueType.Should().Be(request.Tickets[0].IssueType);
                createdTicket.IssueId.Should().Be(request.Tickets[0].IssueId);
                createdTicket.IssueName.Should().Be(request.Tickets[0].IssueName);
                createdTicket.Description.Should().Be(request.Tickets[0].Description);
                createdTicket.Cause.Should().Be(request.Tickets[0].Cause);
                createdTicket.Solution.Should().BeEmpty();
                createdTicket.ReporterId.Should().BeNull();
                createdTicket.ReporterName.Should().Be(request.Tickets[0].ReporterName);
                createdTicket.ReporterPhone.Should().Be(request.Tickets[0].ReporterPhone);
                createdTicket.ReporterEmail.Should().Be(request.Tickets[0].ReporterEmail);
                createdTicket.ReporterCompany.Should().Be(request.Tickets[0].ReporterCompany);
                createdTicket.AssigneeType.Should().Be(request.Tickets[0].AssigneeType);
                createdTicket.AssigneeId.Should().Be(request.Tickets[0].AssigneeId);
                createdTicket.AssigneeName.Should().BeNull();
                createdTicket.CreatorId.Should().Be(request.Tickets[0].CreatorId);
                createdTicket.DueDate.Should().Be(request.Tickets[0].DueDate);
                createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.UpdatedDate.Should().Be(utcNow);
                createdTicket.ComputedCreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.ComputedUpdatedDate.Should().Be(expectedUpdatedDate);
                createdTicket.ResolvedDate.Should().BeNull();
                createdTicket.ClosedDate.Should().BeNull();
                createdTicket.SourceType.Should().Be(request.SourceType);
                createdTicket.SourceId.Should().Be(request.SourceId);
                createdTicket.SourceName.Should().NotBeNull();
                createdTicket.ExternalId.Should().Be(request.Tickets[0].ExternalId);
                createdTicket.ExternalStatus.Should().Be(request.Tickets[0].ExternalStatus);
                createdTicket.ExternalMetadata.Should().Be(request.Tickets[0].ExternalMetadata);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.Tickets[0].CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.Tickets[0].ExtendableSearchablePropertyKeys));
                createdTicket.CategoryId.Should().Be(request.Tickets[0].CategoryId);
                createdTicket.Latitude.Should().Be(request.Tickets[0].Latitude);
                createdTicket.Longitude.Should().Be(request.Tickets[0].Longitude);

                var result = await response.Content.ReadAsAsync<List<TicketDetailDto>>();
                var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(new List<TicketDetailDto>() { expectedTicketDto });

			}
        }
        [Fact]
        public async Task GivenValidOneTicket_withoutInsightId_CreateTickets_ReturnsCreatedTicket()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateTicketsRequest>()
								 .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
								 .Without(x => x.AssigneeId)
								 .With(x => x.Latitude, 123.453259M)
								 .With(x => x.Latitude, -34.721902M)
								 .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(1).ToList())
								 .Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalCreatedDate : utcNow);
			var expectedUpdatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalUpdatedDate : utcNow);

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().SetCurrentDateTime(utcNow);

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}")
					.ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, request.CustomerId).Create());

				server.Arrange().GetSiteApi()
					.SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}/users")
					.ReturnsJson(Fixture.CreateMany<User>(3).ToList());

				var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/batch", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var db = server.Assert().GetDbContext<WorkflowContext>();
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
				createdTicket.Id.Should().NotBe(Guid.Empty);
				createdTicket.CustomerId.Should().Be(request.CustomerId);
				createdTicket.SiteId.Should().Be(siteId);
				createdTicket.FloorCode.Should().Be(request.Tickets[0].FloorCode);
				createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
				createdTicket.Priority.Should().Be(request.Tickets[0].Priority);
				createdTicket.Status.Should().Be(request.Tickets[0].Status);
				createdTicket.IssueType.Should().Be(request.Tickets[0].IssueType);
				createdTicket.IssueId.Should().Be(request.Tickets[0].IssueId);
				createdTicket.IssueName.Should().Be(request.Tickets[0].IssueName);
				createdTicket.Description.Should().Be(request.Tickets[0].Description);
				createdTicket.Cause.Should().Be(request.Tickets[0].Cause);
				createdTicket.Solution.Should().BeEmpty();
				createdTicket.ReporterId.Should().BeNull();
				createdTicket.ReporterName.Should().Be(request.Tickets[0].ReporterName);
				createdTicket.ReporterPhone.Should().Be(request.Tickets[0].ReporterPhone);
				createdTicket.ReporterEmail.Should().Be(request.Tickets[0].ReporterEmail);
				createdTicket.ReporterCompany.Should().Be(request.Tickets[0].ReporterCompany);
				createdTicket.AssigneeType.Should().Be(request.Tickets[0].AssigneeType);
				createdTicket.AssigneeId.Should().Be(request.Tickets[0].AssigneeId);
				createdTicket.AssigneeName.Should().BeNull();
				createdTicket.CreatorId.Should().Be(request.Tickets[0].CreatorId);
				createdTicket.DueDate.Should().Be(request.Tickets[0].DueDate);
				createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
				createdTicket.UpdatedDate.Should().Be(utcNow);
				createdTicket.ComputedCreatedDate.Should().Be(expectedCreatedDate);
				createdTicket.ComputedUpdatedDate.Should().Be(expectedUpdatedDate);
				createdTicket.ResolvedDate.Should().BeNull();
				createdTicket.ClosedDate.Should().BeNull();
				createdTicket.SourceType.Should().Be(request.SourceType);
				createdTicket.SourceId.Should().Be(request.SourceId);
				createdTicket.SourceName.Should().NotBeNull();
				createdTicket.ExternalId.Should().Be(request.Tickets[0].ExternalId);
				createdTicket.ExternalStatus.Should().Be(request.Tickets[0].ExternalStatus);
				createdTicket.ExternalMetadata.Should().Be(request.Tickets[0].ExternalMetadata);
				createdTicket.CategoryId.Should().Be(request.Tickets[0].CategoryId);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.Tickets[0].CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.Tickets[0].ExtendableSearchablePropertyKeys));
				createdTicket.Latitude.Should().Be(request.Tickets[0].Latitude);
				createdTicket.Longitude.Should().Be(request.Tickets[0].Longitude);

				var result = await response.Content.ReadAsAsync<List<TicketDetailDto>>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(new List<TicketDetailDto>() { expectedTicketDto });
			}
		}

		[Fact]
        public async Task GivenSomeTickets_CreateTickets_ReturnsCreatedTickets()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketsRequest>()
                .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
                    .Without(x => x.AssigneeId)
                    .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(1).ToList())
                .Create();
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, request.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                server.Arrange().SetCurrentDateTime(utcNow);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/batch", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(request.Tickets.Count);

                var result = await response.Content.ReadAsAsync<List<TicketDetailDto>>();
                result.Should().HaveCount(request.Tickets.Count);

			}
        }

        [Fact]
        public async Task SequenceNumberPrefixIsNotProvided_CreateTickets_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketRequest>()
                                 .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                 .With(x => x.SequenceNumberPrefix, string.Empty)
                                 .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain(nameof(CreateTicketRequest.SequenceNumberPrefix));
            }
        }

        [Fact]
        public async Task GivenNoAssigneeTypeAndAssigneeId_CreateTickets_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketsRequest>()
                .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
                    .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(1).ToList())
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/batch", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenAssigneeTypeAndNoAssigneeId_CreateTickets_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketsRequest>()
                .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
                    .Without(x => x.AssigneeId)
                    .With(x => x.AssigneeType, AssigneeType.CustomerUser).CreateMany(1).ToList())
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/batch", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenValidOneTicket_CreateTickets_AuditTrailNotCreated()
		{
			var siteId = Guid.NewGuid();
			var request = Fixture.Build<CreateTicketsRequest>()
								 .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
								 .Without(x => x.AssigneeId)
								 .With(x => x.Latitude, 123.453259M)
								 .With(x => x.Latitude, -34.721902M)
								 .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(1).ToList())
                                 .With(x => x.SourceType, SourceType.Dynamics)
                                 .Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalCreatedDate : utcNow);
			var expectedUpdatedDate = (DateTime)(request.Tickets[0].LastUpdatedByExternalSource ? request.Tickets[0].ExternalUpdatedDate : utcNow);

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().SetCurrentDateTime(utcNow);

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}")
					.ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, request.CustomerId).Create());

				server.Arrange().GetSiteApi()
					.SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}/users")
					.ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/batch", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var db = server.Assert().GetDbContext<WorkflowContext>();
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
				

				var result = await response.Content.ReadAsAsync<List<TicketDetailDto>>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(new List<TicketDetailDto>() { expectedTicketDto });
				var auditTrails = db.AuditTrails.ToList();
				auditTrails.Should().HaveCount(0);

			}
		}

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("TwinId_MN")]
        public async Task GivenSomeTickets_WithRequestedTwinId_CreateTickets_ReturnsCreatedTickets(string requestTwinId)
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketsRequest>()
                   .With(x => x.Tickets, Fixture.Build<CreateTicketsRequest.CreateTicketsRequestItem>()
                       .Without(x => x.AssigneeId)
                       .With(x => x.TwinId, requestTwinId)
                       .With(x => x.AssigneeType, AssigneeType.NoAssignee).CreateMany(2).ToList())
                .Create();
            var utcNow = DateTime.UtcNow;
            var assetTwinIds = request.Tickets.Select(c => new TwinIdDto
            {
                Id = $"TwinId_{c.IssueId}",
                UniqueId = c.IssueId.Value.ToString()
            });
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={request.Tickets[0].IssueId}&uniqueIds={request.Tickets[1].IssueId}")
                    .ReturnsJson(assetTwinIds);
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, request.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{request.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                server.Arrange().SetCurrentDateTime(utcNow);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets/batch", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(request.Tickets.Count);

                var result = await response.Content.ReadAsAsync<List<TicketDetailDto>>();
                result.Should().HaveCount(request.Tickets.Count);
                result.First(c => c.IssueId == request.Tickets[0].IssueId).TwinId.Should().Be(
                    string.IsNullOrWhiteSpace(requestTwinId)
                        ? assetTwinIds.First(c => c.UniqueId == request.Tickets[0].IssueId.ToString()).Id
                        : requestTwinId);
                result.First(c => c.IssueId == request.Tickets[1].IssueId).TwinId.Should().Be(
                    string.IsNullOrWhiteSpace(requestTwinId)
                        ? assetTwinIds.First(c => c.UniqueId == request.Tickets[1].IssueId.ToString()).Id
                        : requestTwinId);

            }
        }
    }
}
