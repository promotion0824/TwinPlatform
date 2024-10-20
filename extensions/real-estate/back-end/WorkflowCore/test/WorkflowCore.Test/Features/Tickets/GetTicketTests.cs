using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;

using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using WorkflowCore.Services.Apis;
using System.Linq;
using System.Collections.Generic;
using WorkflowCore.Models;
using Newtonsoft.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetTicketTests : BaseInMemoryTest
    {
        public GetTicketTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketNotExists_GetTicket_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task TicketExists_GetTicket_ReturnsTicket()
        {
            var ticketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
			expectedTicket.CanResolveInsight = true;

            var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        [Fact]
        public async Task TicketExists_NoSiteId_GetTicket_ReturnsTicket()
        {
            var ticketEntity = Fixture.Build<TicketEntity>()
                                      .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
            expectedTicket.CanResolveInsight = true;

            var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }


        [Fact]
        public async Task ScheduledTicketExists_GetTicket_ReturnsScheduledTicket()
        {
            var ticketEntity = Fixture.Build<TicketEntity>()
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.Tasks)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .With(x => x.Solution, string.Empty)
                                      .With(x => x.Cause, string.Empty)
									  .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Create();
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
			expectedTicket.CanResolveInsight = true;

			var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities.OrderBy(t => t.Order)));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
                result.Notes.Should().BeEquivalentTo(expectedTicketDto.Notes);
                for (var i = 0; i < 10; i++)
                {
                    result.Tasks[i].Should().BeEquivalentTo(expectedTicketDto.Tasks[i]);
                }
            }
        }


		[Fact]
		public async Task CanResolveInsightTicketExists_GetTicket_ReturnsTicketWithCanResolveInsightTrue()
		{
			var insightId = Guid.NewGuid();
			var ticketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, 0)
									  .With(x => x.InsightId, insightId)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
									  .Without(x => x.Comments)
									  .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
									  .Create();
			var otherTicketEntities = Fixture.Build<TicketEntity>()
									  .With(x => x.Status,(int) TicketStatusEnum.Closed)
									  .With(x => x.InsightId, insightId)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
									  .Without(x => x.Comments)
									  .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.Tasks)
									  .CreateMany();

			var attachmentEntities = Fixture.Build<AttachmentEntity>()
											.Without(x => x.Ticket)
											.With(x => x.TicketId, ticketEntity.Id)
											.CreateMany(10);
			var commentEntities = Fixture.Build<CommentEntity>()
										 .Without(x => x.Ticket)
										 .With(x => x.TicketId, ticketEntity.Id)
										 .CreateMany(10);
			var taskEntities = Fixture.Build<TicketTaskEntity>()
										.Without(x => x.Ticket)
										.With(x => x.TicketId, ticketEntity.Id)
										.CreateMany(10);
			var imagePathHelper = new ImagePathHelper();
			var expectedTicket = TicketEntity.MapToModel(ticketEntity);
			expectedTicket.CanResolveInsight = true;

			var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
			expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
			expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
			expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));

            var ticketStatusEntities = new List<TicketStatusEntity>
            {
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.CLOSED)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Closed)
                        .With(x=>x.Status, "Closed")
                       .Create(),
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.OPEN)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Open)
                        .With(x=>x.Status, "Open")
                       .Create(),
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Tickets.Add(ticketEntity);
				db.Tickets.AddRange(otherTicketEntities);
				db.Attachments.AddRange(attachmentEntities);
				db.Comments.AddRange(commentEntities);
				db.TicketTasks.AddRange(taskEntities);
                db.TicketStatuses.AddRange(ticketStatusEntities);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				result.Should().BeEquivalentTo(expectedTicketDto);
			}
		}

		[Fact]
		public async Task TicketCanNotResolveInsightExists_GetTicket_ReturnsTicketWithCanResolveInsightFalse()
		{
			var insightId = Guid.NewGuid();
			var ticketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, (int)TicketStatusEnum.Open)
									  .With(x => x.InsightId, insightId)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
									  .Without(x => x.Comments)
									  .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();
			var otherTicketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, (int)TicketStatusEnum.InProgress)
									  .With(x => x.InsightId, insightId)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
									  .Without(x => x.Comments)
									  .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.Tasks)
                                      .Create();

			var attachmentEntities = Fixture.Build<AttachmentEntity>()
											.Without(x => x.Ticket)
											.With(x => x.TicketId, ticketEntity.Id)
											.CreateMany(10);
			var commentEntities = Fixture.Build<CommentEntity>()
										 .Without(x => x.Ticket)
										 .With(x => x.TicketId, ticketEntity.Id)
										 .CreateMany(10);
			var taskEntities = Fixture.Build<TicketTaskEntity>()
										.Without(x => x.Ticket)
										.With(x => x.TicketId, ticketEntity.Id)
										.CreateMany(10);
			var imagePathHelper = new ImagePathHelper();
			var expectedTicket = TicketEntity.MapToModel(ticketEntity);
			expectedTicket.CanResolveInsight = false;

			var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
			expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
			expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
			expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));


            var ticketStatusEntities = new List<TicketStatusEntity>
            {
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.CLOSED)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Closed)
                        .With(x=>x.Status, "Closed")
                       .Create(),
                Fixture.Build<TicketStatusEntity>()
                        .With(x=>x.Tab, TicketTabs.OPEN)
                        .With(x=>x.StatusCode,(int)TicketStatusEnum.Open)
                        .With(x=>x.Status, "Open")
                       .Create(),
            };
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Tickets.Add(ticketEntity);
				db.Tickets.Add(otherTicketEntity);
				db.Attachments.AddRange(attachmentEntities);
				db.Comments.AddRange(commentEntities);
				db.TicketTasks.AddRange(taskEntities);
                db.TicketStatuses.AddRange(ticketStatusEntities);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				result.Should().BeEquivalentTo(expectedTicketDto);
			}
		}

		[Fact]
		public async Task TicketWithoutInsightExists_GetTicket_ReturnsTicketWithCanResolveInsightFalse()
		{
			var ticketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.InsightId)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.Attachments)
									  .Without(x => x.Comments)
									  .Without(x => x.Category)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();


			var attachmentEntities = Fixture.Build<AttachmentEntity>()
											.Without(x => x.Ticket)
											.With(x => x.TicketId, ticketEntity.Id)
											.CreateMany(10);
			var commentEntities = Fixture.Build<CommentEntity>()
										 .Without(x => x.Ticket)
										 .With(x => x.TicketId, ticketEntity.Id)
										 .CreateMany(10);
			var taskEntities = Fixture.Build<TicketTaskEntity>()
										.Without(x => x.Ticket)
										.With(x => x.TicketId, ticketEntity.Id)
										.CreateMany(10);
			var imagePathHelper = new ImagePathHelper();
			var expectedTicket = TicketEntity.MapToModel(ticketEntity);

			var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
			expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
			expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
			expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Tickets.Add(ticketEntity);
				db.Attachments.AddRange(attachmentEntities);
				db.Comments.AddRange(commentEntities);
				db.TicketTasks.AddRange(taskEntities);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				result.Should().BeEquivalentTo(expectedTicketDto);
			}
		}

		[Fact]
		public async Task TicketWithCustomStatusExists_GetTicket_ReturnsCanResolveInsightTickets()
		{
			var customerId = Guid.NewGuid();
			var insightId = Guid.NewGuid();
			var ticketEntity = Fixture.Build<TicketEntity>()
									  .With(x => x.Status, 15)
									  .With(x => x.CustomerId, customerId)
									  .With(x => x.InsightId, insightId)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.InsightId)
									  .Without(x => x.Attachments)
									  .Without(x => x.Comments)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Category)
									  .Without(x => x.Tasks)
									  .Create();


			var otherTicketEntity = Fixture.Build<TicketEntity>()
								  .With(x => x.Status, 10)
								  .With(x => x.CustomerId, customerId)
								  .With(x => x.InsightId, insightId)
                                  .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                  .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                  .Without(x => x.Attachments)
								  .Without(x => x.Comments)
								  .Without(x => x.Category)
                                  .Without(x => x.JobType)
                                  .Without(x => x.Diagnostics)
                                  .Without(x => x.Tasks)
								  .Create();


			var customerTicketStatusClosed = Fixture.Build<TicketStatusEntity>()
											.With(x => x.CustomerId, customerId)
											.With(x => x.StatusCode, 0)
											.With(x => x.Tab, "Closed")
											.Create();

			var customerTicketStatusClosed2 = Fixture.Build<TicketStatusEntity>()
											.With(x => x.CustomerId, customerId)
											.With(x => x.StatusCode, 1)
											.With(x => x.Tab, "Closed")
											.Create();

			var customerTicketStatusOpen = Fixture.Build<TicketStatusEntity>()
											.With(x => x.CustomerId, customerId)
											.With(x => x.StatusCode, 10)
											.With(x => x.Tab, "Open")
											.Create();

			var customerTicketStatusResolved = Fixture.Build<TicketStatusEntity>()
											.With(x => x.CustomerId, customerId)
											.With(x => x.StatusCode, 15)
											.With(x => x.Tab, "Resolved")
											.Create();

			var customerTicketStatus = new List<TicketStatusEntity>
			{
				customerTicketStatusClosed,
				customerTicketStatusClosed2,
				customerTicketStatusOpen,
				customerTicketStatusResolved
			};

			var attachmentEntities = Fixture.Build<AttachmentEntity>()
											.Without(x => x.Ticket)
											.With(x => x.TicketId, ticketEntity.Id)
											.CreateMany(10);
			var commentEntities = Fixture.Build<CommentEntity>()
										 .Without(x => x.Ticket)
										 .With(x => x.TicketId, ticketEntity.Id)
										 .CreateMany(10);
			var taskEntities = Fixture.Build<TicketTaskEntity>()
										.Without(x => x.Ticket)
										.With(x => x.TicketId, ticketEntity.Id)
										.CreateMany(10);
			var imagePathHelper = new ImagePathHelper();
			var expectedTicket = TicketEntity.MapToModel(ticketEntity);
			expectedTicket.CanResolveInsight = false;

			var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
			expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
			expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
			expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));
			expectedTicketDto.CanResolveInsight = false;

			await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				var db = server.Arrange().CreateDbContext<WorkflowContext>();
				db.Tickets.Add(ticketEntity);
				db.Tickets.Add(otherTicketEntity);
				db.Attachments.AddRange(attachmentEntities);
				db.Comments.AddRange(commentEntities);
				db.TicketTasks.AddRange(taskEntities);
				db.TicketStatuses.AddRange(customerTicketStatus);
				db.SaveChanges();

				var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				result.Should().BeEquivalentTo(expectedTicketDto);
			}
		}

        /// <summary>
        ///  Test tickets that return extra fields SpaceTwinId, SubStatus and JobType 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TicketExists_GetTicket_ReturnsTicketWithExtraField()
        {
            var ticketEntity = Fixture.Build<TicketEntity>()
                                      .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
            expectedTicket.CanResolveInsight = true;

            var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

        /// <summary>
        ///  Test tickets that return a ticket with next valid status for transition
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TicketExists_GetTicket_ReturnsTicketWithValidNextStatus()
        {
            var ticketEntity = Fixture.Build<TicketEntity>()
                                      .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();

            var ticketStatusTransition = new List<TicketStatusTransitionsEntity> {
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 0,
                    ToStatus = 110
                },
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 0,
                    ToStatus = 120
                },
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 100,
                    ToStatus = 160
                },

            };
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
            expectedTicket.CanResolveInsight = true;

            var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));
            expectedTicketDto.NextValidStatus = new List<int> { 110, 120 };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.TicketStatusTransitions.AddRange(ticketStatusTransition);
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }
        /// <summary>
        /// when ticket source type is Mapped
        /// the source name should be overridden with the configured source name of the third-party CMMS
        /// </summary>
        /// <returns></returns>

        [Fact]
        public async Task TicketWithMappedSourceTypeExists_GetTicket_ReturnsTicketWithConfiguredSourceName()
        {
            // this name match the name configured in ServerFixtureConfigurations.InMemoryWithMappedIntegration
            var cmmsExternalName = "CMMS Name";
            var ticketEntity = Fixture.Build<TicketEntity>()
                                      .With(x => x.Status, 0)
                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                      .With(x => x.SourceType, SourceType.Mapped)
                                      .Without(x => x.Attachments)
                                      .Without(x => x.Comments)
                                      .Without(x => x.Category)
                                      .Without(x => x.JobType)
                                      .Without(x => x.Diagnostics)
                                      .Without(x => x.ServiceNeeded)
                                      .Without(x => x.Tasks)
                                      .Create();
            var attachmentEntities = Fixture.Build<AttachmentEntity>()
                                            .Without(x => x.Ticket)
                                            .With(x => x.TicketId, ticketEntity.Id)
                                            .CreateMany(10);
            var commentEntities = Fixture.Build<CommentEntity>()
                                         .Without(x => x.Ticket)
                                         .With(x => x.TicketId, ticketEntity.Id)
                                         .CreateMany(10);
            var taskEntities = Fixture.Build<TicketTaskEntity>()
                                        .Without(x => x.Ticket)
                                        .With(x => x.TicketId, ticketEntity.Id)
                                        .CreateMany(10);

            var ticketStatusTransition = new List<TicketStatusTransitionsEntity> {
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 0,
                    ToStatus = 110
                },
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 0,
                    ToStatus = 120
                },
                new TicketStatusTransitionsEntity{
                    Id = Guid.NewGuid(),
                    FromStatus = 100,
                    ToStatus = 160
                },

            };
            var imagePathHelper = new ImagePathHelper();
            var expectedTicket = TicketEntity.MapToModel(ticketEntity);
            expectedTicket.CanResolveInsight = true;

            var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, imagePathHelper);
            expectedTicketDto.Attachments = AttachmentDto.MapFromTicketModels(AttachmentEntity.MapToModels(attachmentEntities), imagePathHelper, expectedTicket);
            expectedTicketDto.Comments = CommentDto.MapFromModels(CommentEntity.MapToModels(commentEntities));
            expectedTicketDto.Tasks = TicketTaskDto.MapFromModels(TicketTaskEntity.MapToModels(taskEntities));
            expectedTicketDto.NextValidStatus = new List<int> { 110, 120 };
            expectedTicketDto.SourceName = cmmsExternalName;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticketEntity);
                db.Attachments.AddRange(attachmentEntities);
                db.Comments.AddRange(commentEntities);
                db.TicketTasks.AddRange(taskEntities);
                db.TicketStatusTransitions.AddRange(ticketStatusTransition);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{ticketEntity.SiteId}/tickets/{ticketEntity.Id}?includeAttachments=true&includeComments=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.Should().BeEquivalentTo(expectedTicketDto);
            }
        }

    }
}
