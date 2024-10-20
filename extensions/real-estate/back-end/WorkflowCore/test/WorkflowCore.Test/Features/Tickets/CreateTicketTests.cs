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
using Microsoft.EntityFrameworkCore;
using FluentAssertions.Extensions;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class CreateTicketTests : BaseInMemoryTest
    {
        public CreateTicketTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets", new CreateTicketRequest());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GivenValidInput_CreateTicket_ReturnsCreatedTicket()
        {
            var siteId = Guid.NewGuid();
            var category = Fixture.Build<TicketCategoryEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Tickets)
                .Create();
            var request = Fixture.Build<CreateTicketRequest>()
                            .Without(x => x.AssigneeId)
                            .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x => x.CategoryId, category.Id)
                            .With(x => x.Latitude, 123.4532M)
                            .With(x => x.Latitude, -34.7219M)
                            .With(x => x.Tasks, new List<TicketTask>())
                            .Create();
            var utcNow = DateTime.UtcNow;
            var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

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

                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketCategories.Add(category);
                db.SaveChanges();

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.Should().NotBeNull();
                createdTicket.Id.Should().NotBe(Guid.Empty);
                createdTicket.CustomerId.Should().Be(request.CustomerId);
                createdTicket.SiteId.Should().Be(siteId);
                createdTicket.FloorCode.Should().Be(request.FloorCode);
                createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
                createdTicket.Priority.Should().Be(request.Priority);
                createdTicket.Status.Should().Be((int)TicketStatusEnum.Open);
                createdTicket.IssueType.Should().Be(request.IssueType);
                createdTicket.IssueId.Should().Be(request.IssueId);
                createdTicket.IssueName.Should().Be(request.IssueName);
                createdTicket.InsightId.Should().Be(request.InsightId);
                createdTicket.InsightName.Should().Be(request.InsightName);
                createdTicket.Description.Should().Be(request.Description);
                createdTicket.Cause.Should().Be(request.Cause);
                createdTicket.Solution.Should().BeEmpty();
                createdTicket.ReporterId.Should().Be(request.ReporterId);
                createdTicket.ReporterName.Should().Be(request.ReporterName);
                createdTicket.ReporterPhone.Should().Be(request.ReporterPhone);
                createdTicket.ReporterEmail.Should().Be(request.ReporterEmail);
                createdTicket.ReporterCompany.Should().Be(request.ReporterCompany);
                createdTicket.AssigneeType.Should().Be(request.AssigneeType);
                createdTicket.AssigneeId.Should().Be(request.AssigneeId);
                createdTicket.AssigneeName.Should().BeNull();
                createdTicket.CreatorId.Should().Be(request.CreatorId);
                createdTicket.DueDate.Should().Be(request.DueDate);
                createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.UpdatedDate.Should().Be(utcNow);
                createdTicket.ResolvedDate.Should().BeNull();
                createdTicket.ClosedDate.Should().BeNull();
                createdTicket.SourceType.Should().Be(request.SourceType);
                createdTicket.SourceId.Should().Be(request.SourceId);
                createdTicket.SourceName.Should().NotBeNull();
                createdTicket.ExternalId.Should().Be(request.ExternalId);
                createdTicket.ExternalStatus.Should().Be(request.ExternalStatus);
                createdTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));
                createdTicket.CategoryId.Should().Be(category.Id);
                createdTicket.Latitude.Should().Be(request.Latitude);
                createdTicket.Longitude.Should().Be(request.Longitude);
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
                expectedTicket.AssigneeName = "Unassigned";
                expectedTicket.Diagnostics.Count.Should().Be(3);
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto);
                
            }
        }
        [Fact]
        public async Task GivenValidInputWithoutInsightId_CreateTicket_ReturnsCreatedTicket()
		{
			var siteId = Guid.NewGuid();
			var category = Fixture.Build<TicketCategoryEntity>()
				.With(x => x.SiteId, siteId)
				.Without(x => x.Tickets)
				.Create();
			var request = Fixture.Build<CreateTicketRequest>()
							.Without(x => x.AssigneeId)
							.Without(x => x.InsightId)
                            .Without(x => x.Diagnostics)
							.With(x => x.AssigneeType, AssigneeType.NoAssignee)
							.With(x => x.CategoryId, category.Id)
							.With(x => x.Latitude, 123.4532M)
							.With(x => x.Latitude, -34.7219M)
							.With(x => x.Tasks, new List<TicketTask>())
							.Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

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
				var db = server.Assert().GetDbContext<WorkflowContext>();
				db.TicketCategories.Add(category);
				db.SaveChanges();

				var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
				createdTicket.Id.Should().NotBe(Guid.Empty);
				createdTicket.CustomerId.Should().Be(request.CustomerId);
				createdTicket.SiteId.Should().Be(siteId);
				createdTicket.FloorCode.Should().Be(request.FloorCode);
				createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
				createdTicket.Priority.Should().Be(request.Priority);
				createdTicket.Status.Should().Be((int)TicketStatusEnum.Open);
				createdTicket.IssueType.Should().Be(request.IssueType);
				createdTicket.IssueId.Should().Be(request.IssueId);
				createdTicket.IssueName.Should().Be(request.IssueName);
				createdTicket.InsightId.Should().Be(request.InsightId);
				createdTicket.InsightName.Should().Be(request.InsightName);
				createdTicket.Description.Should().Be(request.Description);
				createdTicket.Cause.Should().Be(request.Cause);
				createdTicket.Solution.Should().BeEmpty();
				createdTicket.ReporterId.Should().Be(request.ReporterId);
				createdTicket.ReporterName.Should().Be(request.ReporterName);
				createdTicket.ReporterPhone.Should().Be(request.ReporterPhone);
				createdTicket.ReporterEmail.Should().Be(request.ReporterEmail);
				createdTicket.ReporterCompany.Should().Be(request.ReporterCompany);
				createdTicket.AssigneeType.Should().Be(request.AssigneeType);
				createdTicket.AssigneeId.Should().Be(request.AssigneeId);
				createdTicket.AssigneeName.Should().BeNull();
				createdTicket.CreatorId.Should().Be(request.CreatorId);
				createdTicket.DueDate.Should().Be(request.DueDate);
				createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
				createdTicket.UpdatedDate.Should().Be(utcNow);
				createdTicket.ResolvedDate.Should().BeNull();
				createdTicket.ClosedDate.Should().BeNull();
				createdTicket.SourceType.Should().Be(request.SourceType);
				createdTicket.SourceId.Should().Be(request.SourceId);
				createdTicket.SourceName.Should().NotBeNull();
				createdTicket.ExternalId.Should().Be(request.ExternalId);
				createdTicket.ExternalStatus.Should().Be(request.ExternalStatus);
				createdTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
				createdTicket.CategoryId.Should().Be(category.Id);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));
                createdTicket.Latitude.Should().Be(request.Latitude);
				createdTicket.Longitude.Should().Be(request.Longitude);

				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
				expectedTicket.Attachments = new List<TicketAttachment>();
				expectedTicket.Comments = new List<Comment>();
				expectedTicket.Tasks = new List<TicketTask>();
				expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto);
			}
		}

        [Fact]
        public async Task ReporterDoesNotExist_CreateTicket_ReporterIsCreated()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketRequest>()
                .Without(x => x.AssigneeId)
                .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                .With(x => x.ReporterId, (Guid?)null)
                .Create();

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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var reporter = db.Reporters.First();
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.ReporterId.Should().Be(reporter.Id);
                
			}
        }

        [Fact]
        public async Task ThereIsNoTicketForGivenSite_CreateTicket_ReturnsTicketWithFirstSequenceNumber()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketRequest>()
                .Without(x => x.AssigneeId)
                .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                .Create();
            var expectedSequenceNumber = $"{request.SequenceNumberPrefix}-T-1";

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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.SequenceNumber.Should().Be(expectedSequenceNumber);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.SequenceNumber.Should().Be(expectedSequenceNumber);
                
			}
        }

        [Fact]
        public async Task ThereAreTicketsForGivenSite_CreateTicket_ReturnsTicketWithNextSequenceNumber()
        {
            var siteId = Guid.NewGuid();
            var request = Fixture.Build<CreateTicketRequest>()
                .Without(x => x.AssigneeId)
                .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                .Create();

            var expectedSequenceNumber = $"{request.SequenceNumberPrefix}-T-1";

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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.SequenceNumber.Should().Be(expectedSequenceNumber);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.SequenceNumber.Should().Be(expectedSequenceNumber);
             
			}
        }

        [Fact]
        public async Task SequenceNumberPrefixIsNotProvided_CreateTicket_ReturnsBadRequest()
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
        public async Task GivenValidInput_CreateDynamicsTicket_ReturnsCreatedTicket()
        {
            var siteId = Guid.NewGuid();
            var category = Fixture.Build<TicketCategoryEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Tickets)
                .Create();

            var request = Fixture.Build<CreateTicketRequest>()
                            .Without(x => x.AssigneeId)
                            .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x => x.CategoryId, category.Id)
                            .With(x => x.SourceType, SourceType.Dynamics)
                            .With(x => x.Tasks, new List<TicketTask>())
                            .Create();

            var utcNow = DateTime.UtcNow;

            var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);
            var expectedUpdatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalUpdatedDate : utcNow);

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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketCategories.Add(category);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.Should().NotBeNull();
                createdTicket.Id.Should().NotBe(Guid.Empty);
                createdTicket.CustomerId.Should().Be(request.CustomerId);
                createdTicket.SiteId.Should().Be(siteId);
                createdTicket.FloorCode.Should().Be(request.FloorCode);
                createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
                createdTicket.Priority.Should().Be(request.Priority);
                createdTicket.Status.Should().Be((int)TicketStatusEnum.Open);
                createdTicket.IssueType.Should().Be(request.IssueType);
                createdTicket.IssueId.Should().Be(request.IssueId);
                createdTicket.IssueName.Should().Be(request.IssueName);
                createdTicket.InsightId.Should().Be(request.InsightId);
                createdTicket.InsightName.Should().Be(request.InsightName);
                createdTicket.Description.Should().Be(request.Description);
                createdTicket.Cause.Should().Be(request.Cause);
                createdTicket.Solution.Should().BeEmpty();
                createdTicket.ReporterId.Should().Be(request.ReporterId);
                createdTicket.ReporterName.Should().Be(request.ReporterName);
                createdTicket.ReporterPhone.Should().Be(request.ReporterPhone);
                createdTicket.ReporterEmail.Should().Be(request.ReporterEmail);
                createdTicket.ReporterCompany.Should().Be(request.ReporterCompany);
                createdTicket.AssigneeType.Should().Be(request.AssigneeType);
                createdTicket.AssigneeId.Should().Be(request.AssigneeId);
                createdTicket.AssigneeName.Should().BeNull();
                createdTicket.CreatorId.Should().Be(request.CreatorId);
                createdTicket.DueDate.Should().Be(request.DueDate);
                createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.UpdatedDate.Should().Be(utcNow);
                createdTicket.ComputedCreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.ComputedUpdatedDate.Should().Be(expectedUpdatedDate);
                createdTicket.ResolvedDate.Should().BeNull();
                createdTicket.ClosedDate.Should().BeNull();
                createdTicket.SourceType.Should().Be(request.SourceType);
                createdTicket.SourceId.Should().Be(request.SourceId);

                createdTicket.ExternalId.Should().Be(request.ExternalId);
                createdTicket.ExternalStatus.Should().Be(request.ExternalStatus);
                createdTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));
                createdTicket.CategoryId.Should().Be(category.Id);
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto);

			}
        }

        [Fact]
        public async Task GivenNoAssigneeTypeAndAssigneeId_CreateTicket_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketRequest>()
                .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenAssigneeTypeAndNoAssigneeId_CreateTicket_ReturnsBadRequest()
        {
            var request = Fixture.Build<CreateTicketRequest>()
                .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                .Without(x => x.AssigneeId)
                .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GivenValidInputWithInsight_CreateTicket_TicketAuditTrailCreated()
		{
			var siteId = Guid.NewGuid();
			var category = Fixture.Build<TicketCategoryEntity>()
				.With(x => x.SiteId, siteId)
				.Without(x => x.Tickets)
				.Create();
			var request = Fixture.Build<CreateTicketRequest>()
							.Without(x => x.AssigneeId)
							.With(x => x.AssigneeType, AssigneeType.NoAssignee)
							.With(x => x.CategoryId, category.Id)
							.With(x => x.Latitude, 123.4532M)
							.With(x => x.Latitude, -34.7219M)
							.With(x => x.Tasks, new List<TicketTask>())
							.Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

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
                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var db = server.Assert().GetDbContext<WorkflowContext>();

				db.TicketCategories.Add(category);
				db.SaveChanges();

				var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();


                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
				expectedTicket.Attachments = new List<TicketAttachment>();
				expectedTicket.Comments = new List<Comment>();
				expectedTicket.Tasks = new List<TicketTask>();
				expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto);

				var timeStamp = DateTime.UtcNow;
				var expectedAuditTrails = new List<AuditTrailEntity> {
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Status))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Status.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeId))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeId?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeName))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeName?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeType))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeType.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Summary))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Summary.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Description))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Description.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.DueDate))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.DueDate.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Priority))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.SourceId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Priority.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create()
				};
				var auditTrails = db.AuditTrails.ToList();
				auditTrails.Should().NotBeNull();
				auditTrails.Should().HaveCount(8);
				auditTrails.Should().BeEquivalentTo(expectedAuditTrails, config =>
				{
					config.Excluding(x => x.Id);
					config.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 30000.Seconds())).WhenTypeIs<DateTime>();
					return config;

				});

			}
		}

        [Fact]
        public async Task GivenValidInputWithInsightAndWithoutSourceId_CreateTicket_TicketAuditTrailCreated()
		{
			var siteId = Guid.NewGuid();
			var category = Fixture.Build<TicketCategoryEntity>()
				.With(x => x.SiteId, siteId)
				.Without(x => x.Tickets)
				.Create();
			var request = Fixture.Build<CreateTicketRequest>()
							.Without(x => x.AssigneeId)
							.With(x => x.AssigneeType, AssigneeType.NoAssignee)
							.With(x => x.CategoryId, category.Id)
							.With(x => x.Latitude, 123.4532M)
							.With(x => x.Latitude, -34.7219M)
							.Without(x => x.SourceId)
							.With(x => x.Tasks, new List<TicketTask>())
							.Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                server.Arrange().SetCurrentDateTime(utcNow);
				var db = server.Assert().GetDbContext<WorkflowContext>();

				db.TicketCategories.Add(category);
				db.SaveChanges();

				var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();


                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
				expectedTicket.Attachments = new List<TicketAttachment>();
				expectedTicket.Comments = new List<Comment>();
				expectedTicket.Tasks = new List<TicketTask>();
				expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto);

				var timeStamp = DateTime.UtcNow;
				var expectedAuditTrails = new List<AuditTrailEntity> {
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Status))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Status.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeId))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeId?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeName))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeName?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.AssigneeType))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.AssigneeType.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Summary))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Summary.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Description))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Description.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.DueDate))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.DueDate.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, createdTicket.Id)
						.With(x => x.ColumnName, nameof(createdTicket.Priority))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Added.ToString())
						.With(x => x.SourceId, createdTicket.CreatorId)
						.With(x => x.SourceType, createdTicket.SourceType)
						.With(x => x.NewValue, createdTicket.Priority.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.Without(x => x.OldValue)
						.Create()
				};
				var auditTrails = db.AuditTrails.ToList();
				auditTrails.Should().NotBeNull();
				auditTrails.Should().HaveCount(8);
				auditTrails.Should().BeEquivalentTo(expectedAuditTrails, config =>
				{
					config.Excluding(x => x.Id);
					config.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 30000.Seconds())).WhenTypeIs<DateTime>();
					return config;

				});

			}
		}
        /// <summary>
        /// When ticket source is Willow (Platform) or Mapped the status will tracked in Audit trail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GivenValidInputWithoutInsight_CreateTicket_TicketAuditTrailCreated()
		{
			var siteId = Guid.NewGuid();
			var category = Fixture.Build<TicketCategoryEntity>()
				.With(x => x.SiteId, siteId)
				.Without(x => x.Tickets)
				.Create();

			var request = Fixture.Build<CreateTicketRequest>()
							.Without(x => x.AssigneeId)
							.With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x=> x.SourceType, SourceType.Platform)
							.With(x => x.CategoryId, category.Id)
							.With(x => x.Latitude, 123.4532M)
							.With(x => x.Latitude, -34.7219M)
							.With(x => x.Tasks, new List<TicketTask>())
							.Without(x => x.InsightId)
                            .Without(x => x.Diagnostics)
                            .Without(x => x.SourceId)
                            .Create();
			var utcNow = DateTime.UtcNow;
			var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

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
				var db = server.Assert().GetDbContext<WorkflowContext>();

				db.TicketCategories.Add(category);
				db.SaveChanges();

				var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db.Tickets.Should().HaveCount(1);
				var createdTicket = db.Tickets.First();
				createdTicket.Should().NotBeNull();
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();


				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(createdTicket);
				expectedTicket.Attachments = new List<TicketAttachment>();
				expectedTicket.Comments = new List<Comment>();
				expectedTicket.Tasks = new List<TicketTask>();
				expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto);

				var auditTrails = db.AuditTrails.ToList();
				auditTrails.Should().HaveCount(1);
                var auditTrail = auditTrails.First();
                auditTrail.RecordID.Should().Be(createdTicket.Id);
                auditTrail.TableName.Should().Be(nameof(TicketEntity));
                auditTrail.ColumnName.Should().Be(nameof(TicketEntity.Status));
                auditTrail.OldValue.Should().BeNull();
                auditTrail.NewValue.Should().Be(((int)createdTicket.Status).ToString());
                auditTrail.SourceType.Should().Be(SourceType.Platform);
                auditTrail.SourceId.Should().Be(createdTicket.CreatorId);

			}
		}

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("TwinId_MN")]
        public async Task GivenValidInput_WithRequestedTwinId_CreateTicket_ReturnsCreatedTicketWithTwinId(string requestedTwinId)
        {
            var siteId = Guid.NewGuid();
            var category = Fixture.Build<TicketCategoryEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Tickets)
                .Create();
            var request = Fixture.Build<CreateTicketRequest>()
                            .Without(x => x.AssigneeId)
                            .With(i => i.TwinId, requestedTwinId)
                            .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x => x.CategoryId, category.Id)
                            .With(x => x.Latitude, 123.4532M)
                            .With(x => x.Latitude, -34.7219M)
                            .With(x => x.Tasks, new List<TicketTask>())
                            .Create();
            var utcNow = DateTime.UtcNow;
          
            var assetTwinIds = new List<TwinIdDto>()
            {
                new()
                {
                    Id ="TwinId-Ms-2",
                    UniqueId = request.IssueId.Value.ToString()
                }
            };
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDigitalTwinApi()
                    .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={request.IssueId}")
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

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                server.Arrange().SetCurrentDateTime(utcNow);
                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketCategories.Add(category);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.Include(x => x.Diagnostics).First();
                createdTicket.Should().NotBeNull();
                createdTicket.TwinId.Should().Be(string.IsNullOrWhiteSpace(requestedTwinId) ? assetTwinIds.First().Id : requestedTwinId);

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto);

            }
        }

        [Fact]
        public async Task MappedEnabled_CreateTicket_ReturnsCreatedTicketWithNewStatus()
        {
            var siteId = Guid.NewGuid();
            var category = Fixture.Build<TicketCategoryEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Tickets)
                .Create();
            var request = Fixture.Build<CreateTicketRequest>()
                            .Without(x => x.AssigneeId)
                            .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                            .With(x => x.CategoryId, category.Id)
                            .With(x => x.Latitude, 123.4532M)
                            .With(x => x.Latitude, -34.7219M)
                            .With(x => x.Tasks, new List<TicketTask>())
                            .Create();
            var utcNow = DateTime.UtcNow;
            var expectedCreatedDate = (DateTime)(request.LastUpdatedByExternalSource ? request.ExternalCreatedDate : utcNow);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
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

                var db = server.Assert().GetDbContext<WorkflowContext>();
                db.TicketCategories.Add(category);
                db.SaveChanges();

                server.Arrange().GetInsightCore()
                    .SetupRequestSequence(HttpMethod.Put, $"sites/{siteId}/insights/status")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.PostAsJsonAsync($"sites/{siteId}/tickets", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db.Tickets.Should().HaveCount(1);
                var createdTicket = db.Tickets.First();
                createdTicket.Should().NotBeNull();
                createdTicket.Id.Should().NotBe(Guid.Empty);
                createdTicket.CustomerId.Should().Be(request.CustomerId);
                createdTicket.SiteId.Should().Be(siteId);
                createdTicket.FloorCode.Should().Be(request.FloorCode);
                createdTicket.SequenceNumber.Should().StartWith(request.SequenceNumberPrefix);
                createdTicket.Priority.Should().Be(request.Priority);
                createdTicket.Status.Should().Be((int)TicketStatusEnum.New);
                createdTicket.IssueType.Should().Be(request.IssueType);
                createdTicket.IssueId.Should().Be(request.IssueId);
                createdTicket.IssueName.Should().Be(request.IssueName);
                createdTicket.InsightId.Should().Be(request.InsightId);
                createdTicket.InsightName.Should().Be(request.InsightName);
                createdTicket.Description.Should().Be(request.Description);
                createdTicket.Cause.Should().Be(request.Cause);
                createdTicket.Solution.Should().BeEmpty();
                createdTicket.ReporterId.Should().Be(request.ReporterId);
                createdTicket.ReporterName.Should().Be(request.ReporterName);
                createdTicket.ReporterPhone.Should().Be(request.ReporterPhone);
                createdTicket.ReporterEmail.Should().Be(request.ReporterEmail);
                createdTicket.ReporterCompany.Should().Be(request.ReporterCompany);
                createdTicket.AssigneeType.Should().Be(request.AssigneeType);
                createdTicket.AssigneeId.Should().Be(request.AssigneeId);
                createdTicket.AssigneeName.Should().BeNull();
                createdTicket.CreatorId.Should().Be(request.CreatorId);
                createdTicket.DueDate.Should().Be(request.DueDate);
                createdTicket.CreatedDate.Should().Be(expectedCreatedDate);
                createdTicket.UpdatedDate.Should().Be(utcNow);
                createdTicket.ResolvedDate.Should().BeNull();
                createdTicket.ClosedDate.Should().BeNull();
                createdTicket.SourceType.Should().Be(request.SourceType);
                createdTicket.SourceId.Should().Be(request.SourceId);
                createdTicket.SourceName.Should().NotBeNull();
                createdTicket.ExternalId.Should().Be(request.ExternalId);
                createdTicket.ExternalStatus.Should().Be(request.ExternalStatus);
                createdTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                createdTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                createdTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));
                createdTicket.CategoryId.Should().Be(category.Id);
                createdTicket.Latitude.Should().Be(request.Latitude);
                createdTicket.Longitude.Should().Be(request.Longitude);
                createdTicket.Diagnostics = db.TicketInsights.Where(x => x.TicketId == createdTicket.Id).ToList();
                

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(createdTicket);
                expectedTicket.Attachments = new List<TicketAttachment>();
                expectedTicket.Comments = new List<Comment>();
                expectedTicket.Tasks = new List<TicketTask>();
                expectedTicket.Category = TicketCategoryEntity.MapToModel(category);
                expectedTicket.AssigneeName = "Unassigned";
                expectedTicket.Diagnostics.Count.Should().Be(3);
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                expectedTicketDto.NextValidStatus = [];
                result.Should().BeEquivalentTo(expectedTicketDto);

            }
        }
    }
}
