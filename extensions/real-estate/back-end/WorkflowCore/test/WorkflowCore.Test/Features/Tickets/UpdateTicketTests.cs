using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Willow.Infrastructure;
using Willow.Tests.Infrastructure;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using Xunit;
using Xunit.Abstractions;

namespace WorkflowCore.Test.Features.Tickets
{
    public class UpdateTicketTests : BaseInMemoryTest
    {
        public UpdateTicketTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_UpdateTicket_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}", new UpdateTicketRequest { AssigneeType = AssigneeType.NoAssignee });
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_UpdateTicket_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PutAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}", new UpdateTicketRequest { AssigneeType = AssigneeType.NoAssignee });
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public async Task GivenInvalidPriority_UpdateTicket_ReturnsBadRequest(int invalidPriority)
        {
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{existingTicket.SiteId}/tickets/{existingTicket.Id}", new UpdateTicketRequest { Priority = invalidPriority, AssigneeType = AssigneeType.NoAssignee });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Priority", out _));
            }
        }

        [Fact]
        public async Task GivenInvalidStatus_UpdateTicket_ReturnsBadRequest()
        {
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{existingTicket.SiteId}/tickets/{existingTicket.Id}", new UpdateTicketRequest { Status = 999, AssigneeType = AssigneeType.NoAssignee });

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("Status", out _));
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdateTicket_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(request.Priority);
                updatedTicket.Status.Should().Be(request.Status);
                updatedTicket.IssueType.Should().Be(request.IssueType.Value);
                updatedTicket.IssueId.Should().Be(request.IssueId);
                updatedTicket.IssueName.Should().Be(request.IssueName);
                updatedTicket.Summary.Should().Be(request.Summary);
                updatedTicket.Description.Should().Be(request.Description);
                updatedTicket.Cause.Should().Be(request.Cause);
                updatedTicket.Solution.Should().Be(request.Solution);
                updatedTicket.AssigneeType.Should().Be(request.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(request.AssigneeId);
                updatedTicket.AssigneeName.Should().BeNull();
                updatedTicket.SourceName.Should().NotBeNull();
                updatedTicket.DueDate.Should().Be(request.DueDate);
                updatedTicket.UpdatedDate.Should().Be(utcNow);
                updatedTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                updatedTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(updatedTicket);
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto, config => config
					           .Excluding(x => x.CanResolveInsight));

            }
        }

        [Fact]
        public async Task GivenValidInput_LongDescription_UpdateTicket_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x=>x.Description, new string('*', 50000))
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(request.Priority);
                updatedTicket.Status.Should().Be(request.Status);
                updatedTicket.IssueType.Should().Be(request.IssueType.Value);
                updatedTicket.IssueId.Should().Be(request.IssueId);
                updatedTicket.IssueName.Should().Be(request.IssueName);
                updatedTicket.Summary.Should().Be(request.Summary);
                updatedTicket.Description.Should().Be(request.Description);
                updatedTicket.Cause.Should().Be(request.Cause);
                updatedTicket.Solution.Should().Be(request.Solution);
                updatedTicket.AssigneeType.Should().Be(request.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(request.AssigneeId);
                updatedTicket.AssigneeName.Should().BeNull();
                updatedTicket.SourceName.Should().NotBeNull();
                updatedTicket.DueDate.Should().Be(request.DueDate);
                updatedTicket.UpdatedDate.Should().Be(utcNow);
                updatedTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                updatedTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(updatedTicket);
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto, config => config
                               .Excluding(x => x.CanResolveInsight));

            }
        }
        [Fact]
        public async Task GivenValidInput_UpdateTicketTasks_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.Category)
                                        .Without(x => x.CategoryId)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();
            var existingTicketTask = Fixture.Build<TicketTaskEntity>()
                                        .With(x => x.TicketId, ticketId)
                                        .Without(x => x.Ticket)
                                        .Without(x => x.NumberValue)
                                        .Create();

            var request = new UpdateTicketRequest { AssigneeType = AssigneeType.NoAssignee };
            var ticketTaskRequest = Fixture.Build<TicketTask>()
                                 .Without(x => x.Id)
                                 .Create();

            var existingTicketTaskRequest = TicketTaskEntity.MapToModel(existingTicketTask);
            existingTicketTaskRequest.NumberValue = existingTicketTaskRequest.MinValue + 10;
            request.Tasks = new List<TicketTask> { ticketTaskRequest, existingTicketTaskRequest };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.TicketTasks.Add(existingTicketTask);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Tasks).First();
                updatedTicket.UpdatedDate.Should().Be(utcNow);

                db.TicketTasks.Should().HaveCount(2);
                var addedTask = db.TicketTasks.First(x => x.Id != existingTicketTask.Id);
                addedTask.TaskName.Should().BeEquivalentTo(ticketTaskRequest.TaskName);
                addedTask.TicketId.Should().Be(existingTicket.Id);
                addedTask.IsCompleted.Should().Be(ticketTaskRequest.IsCompleted);
                addedTask.Order.Should().Be(2);
                var updatedTask = db.TicketTasks.First(x => x.Id == existingTicketTask.Id);
                updatedTask.Order.Should().Be(1);
                updatedTask.NumberValue = updatedTask.MinValue + 10;

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicketDto = TicketDetailDto.MapFromModel(TicketEntity.MapToModel(updatedTicket), server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto, config => config.Excluding(x => x.Attachments).Excluding(x => x.Comments).Excluding(x => x.Tasks).Excluding(x => x.CanResolveInsight));
                result.Tasks[0].Should().BeEquivalentTo(TicketTaskDto.MapFromModel(TicketTaskEntity.MapToModel(updatedTask)));
                result.Tasks[1].Should().BeEquivalentTo(TicketTaskDto.MapFromModel(TicketTaskEntity.MapToModel(addedTask)));
            }
        }

        [Fact]
        public async Task TicketIsNotInStatusInProgress_UpdateTicketWithStatusInProgress_TicketStartedDateIsUpdated()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = new UpdateTicketRequest { Status = (int)TicketStatusEnum.InProgress, AssigneeType = AssigneeType.NoAssignee };
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.Status, (int)TicketStatusEnum.Open)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.StartedDate.Should().BeCloseTo(utcNow, 1.Seconds());
            }
        }

        [Fact]
        public async Task TicketIsNotInStatusResolved_UpdateTicketWithStatusResolved_TicketResolvedDateIsUpdated()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = new UpdateTicketRequest { Status = (int)TicketStatusEnum.Resolved, AssigneeType = AssigneeType.NoAssignee };
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.ResolvedDate.Should().BeCloseTo(utcNow, 1.Seconds());
            }
        }

        [Fact]
        public async Task TicketIsNotInStatusClosed_UpdateTicketWithStatusClosed_TicketClosedDateIsUpdated()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = new UpdateTicketRequest { Status = (int)TicketStatusEnum.Closed, AssigneeType = AssigneeType.NoAssignee };
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

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
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.TicketStatuses.AddRange(ticketStatusEntities);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                result.ClosedDate.Should().BeCloseTo(utcNow, 1.Seconds());
            }
        }

        [Fact]
        public async Task ReporterDoesNotExist_UpdateTicket_ReporterIsCreated()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var request = new UpdateTicketRequest { AssigneeType = AssigneeType.NoAssignee };
            request.ReporterCompany = "newCompany";
            request.ReporterName = "newReporter";
            request.ReporterPhone = "1122334455";
            request.ReporterEmail = "new@reporter.com";
            request.ShouldUpdateReporterId = true;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.CategoryId)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.UpdatedDate, utcNow)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = arrangement.CreateDbContext<WorkflowContext>();
                db.Reporters.Should().HaveCount(1);
                var reporter = db.Reporters.First();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.First();
                updatedTicket.ReporterId.Should().Be(reporter.Id);
            }
        }

        [Fact]
        public async Task GivenNoUpdatedProperty_UpdateTicket_ReturnsNothingChanges()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.CategoryId)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.AssigneeId)
                                        .Without(x => x.AssigneeName)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .Without(x => x.ServiceNeededId)
                                        .Without(x => x.ServiceNeeded)
                                        .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.UpdatedDate, utcNow)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.LastUpdatedByExternalSource, false)
                                        .With(x => x.Latitude, 123.4532M)
                                        .With(x => x.Latitude, -34.7219M)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            var request = new UpdateTicketRequest 
            { 
                AssigneeType = existingTicket.AssigneeType, 
                AssigneeId = existingTicket.AssigneeId, 
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" } );

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).First();
                updatedTicket.Should().BeEquivalentTo(existingTicket, config => config.Excluding(x => x.Attachments).Excluding(x => x.Comments));

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicketDto = TicketDetailDto.MapFromModel(TicketEntity.MapToModel(updatedTicket), server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto, config => config.Excluding(x => x.Attachments).Excluding(x => x.Comments).Excluding(x => x.Tasks).Excluding(x => x.CanResolveInsight));
            }
        }

        [Fact]
        public async Task NewLatLong_UpdateTicket_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.CategoryId)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.ExternalCreatedDate)
                                        .Without(x => x.ExternalUpdatedDate)
                                        .Without(x => x.AssigneeId)
                                        .Without(x => x.AssigneeName)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .Without(x => x.ServiceNeededId)
                                        .Without(x => x.ServiceNeeded)
                                        .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.UpdatedDate, utcNow)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.LastUpdatedByExternalSource, false)
                                        .With(x => x.Latitude, 123.4532M)
                                        .With(x => x.Latitude, -34.7219M)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            var request = new UpdateTicketRequest
            {
                Latitude = -174.5543M,
                Longitude = 0M
            };

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, Name = "Site54" });

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).First();
                updatedTicket.Should().BeEquivalentTo(existingTicket, config => config
                    .Excluding(x => x.Attachments)
                    .Excluding(x => x.Comments)
                    .Excluding(x => x.Latitude)
                    .Excluding(x => x.Longitude));

                updatedTicket.Latitude = request.Latitude;
                updatedTicket.Longitude = request.Longitude;

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicketDto = TicketDetailDto.MapFromModel(TicketEntity.MapToModel(updatedTicket), server.Assert().GetImagePathHelper());
                
                result.Should().BeEquivalentTo(expectedTicketDto, config => config
                    .Excluding(x => x.Attachments)
                    .Excluding(x => x.Comments)
                    .Excluding(x => x.Tasks)
                    .Excluding(x => x.Latitude)
                    .Excluding(x => x.Longitude)
					.Excluding(x => x.CanResolveInsight));

                result.Latitude = request.Latitude;
                result.Longitude = request.Longitude;
            }
        }

        [Fact]
        public async Task GivenValidInput_UpdateDyanmicsTicket_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var externalId = "345678";
            var ticketId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.CustomerId, customerId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Description, "updated one")
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.CustomerId, customerId)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.SourceType, SourceType.Dynamics)
                                        .With(x => x.ExternalId, externalId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            var customerTicketStatuses = new List<TicketStatusEntity> { 
                                                Fixture.Build<TicketStatusEntity>().With(x => x.CustomerId, customerId).With(x => x.StatusCode, request.Status).Create(),
                                                Fixture.Build<TicketStatusEntity>().With(x => x.CustomerId, customerId).With(x => x.StatusCode, existingTicket.Status).Create()};
                        
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.TicketStatuses.AddRange(customerTicketStatuses);
                db.SaveChanges();
               
                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.Priority.Should().Be(request.Priority);
                updatedTicket.Status.Should().Be(request.Status);
                updatedTicket.IssueType.Should().Be(request.IssueType.Value);
                updatedTicket.IssueId.Should().Be(request.IssueId);
                updatedTicket.IssueName.Should().Be(request.IssueName);
                updatedTicket.Summary.Should().Be(request.Summary);
                updatedTicket.Description.Should().Be(request.Description);
                updatedTicket.Cause.Should().Be(request.Cause);
                updatedTicket.Solution.Should().Be(request.Solution);
                updatedTicket.AssigneeType.Should().Be(request.AssigneeType);
                updatedTicket.AssigneeId.Should().Be(request.AssigneeId);
                updatedTicket.AssigneeName.Should().BeNull();
                updatedTicket.DueDate.Should().Be(request.DueDate);
                updatedTicket.UpdatedDate.Should().Be(utcNow);
                updatedTicket.ExternalMetadata.Should().Be(request.ExternalMetadata);
                updatedTicket.CustomProperties.Should().Be(JsonConvert.SerializeObject(request.CustomProperties));
                updatedTicket.ExtendableSearchablePropertyKeys.Should().Be(JsonConvert.SerializeObject(request.ExtendableSearchablePropertyKeys));

                var result = await response.Content.ReadAsAsync<TicketDetailDto>();
                var expectedTicket = TicketEntity.MapToModel(updatedTicket);
                expectedTicket.AssigneeName = "Unassigned";
                var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
                result.Should().BeEquivalentTo(expectedTicketDto , config => config
							   .Excluding(x => x.CanResolveInsight));

            }
        }


		[Fact]
		public async Task GivenValidInputWithInsight_UpdateTicket_CreateAuditTrail()
		{
			var siteId = Guid.NewGuid();
			var ticketId = Guid.NewGuid();
			var request = Fixture.Build<UpdateTicketRequest>()
								 .Without(x => x.AssigneeId)
								 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
								 .With(x => x.Priority, new Random().Next() % 4 + 1)
								 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
								 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
								 .Create();
            var utcNow = DateTime.UtcNow;
			var existingTicket = Fixture.Build<TicketEntity>()
										.Without(x => x.Attachments)
										.Without(x => x.Comments)
										.Without(x => x.Category)
										.Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
										.With(x => x.SiteId, siteId)
										.With(x => x.TemplateId, (Guid?)null)
										.With(x => x.Status, (int)TicketStatusEnum.Reassign)
										.With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
					.ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

				server.Arrange().GetSiteApi()
					.SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
					.ReturnsJson(Fixture.CreateMany<User>(3).ToList());

				var arrangement = server.Arrange();
				arrangement.SetCurrentDateTime(utcNow);
				var db = arrangement.CreateDbContext<WorkflowContext>();
				db.Tickets.Add(existingTicket);
				db.SaveChanges();
				var createdTicket = db.Tickets.First();
				var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db = server.Assert().GetDbContext<WorkflowContext>();
				db.Tickets.Should().HaveCount(1);
				var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
				updatedTicket.Should().NotBeNull();
				
				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(updatedTicket);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto, config => config
							   .Excluding(x => x.CanResolveInsight));


				var timeStamp = DateTime.UtcNow;
				var expectedAuditTrails = new List<AuditTrailEntity> {
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.Status))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.Status.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.Status.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.AssigneeId))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.AssigneeId?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.AssigneeId.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.AssigneeName))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.AssigneeName?.ToString() ?? "")
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.AssigneeName.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.AssigneeType))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.AssigneeType.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.AssigneeType.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.Summary))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.Summary.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.Summary.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.Description))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.Description.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.Description.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.DueDate))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.DueDate.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.DueDate.ToString() ?? "")
						.Create(),
					Fixture.Build<AuditTrailEntity>()
						.With(x => x.RecordID, updatedTicket.Id)
						.With(x => x.ColumnName, nameof(updatedTicket.Priority))
						.With(x => x.TableName, nameof(TicketEntity))
						.With(x => x.OperationType, EntityState.Modified.ToString())
						.With(x => x.SourceId, request.SourceId)
						.With(x => x.SourceType, request.SourceType)
						.With(x => x.NewValue, updatedTicket.Priority.ToString())
						.With(x=> x.Timestamp, timeStamp)
						.With(x=> x.OldValue, createdTicket.Priority.ToString() ?? "")
						.Create()
				};
				var auditTrails = db.AuditTrails.Where(x => x.OperationType == EntityState.Modified.ToString()).ToList();
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
		public async Task GivenValidInputWithoutInsight_UpdateTicket_AuditTrailNotCreated()
		{
			var siteId = Guid.NewGuid();
			var ticketId = Guid.NewGuid();
			var request = Fixture.Build<UpdateTicketRequest>()
								 .Without(x => x.AssigneeId)
								 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
								 .With(x => x.Priority, new Random().Next() % 4 + 1)
								 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
								 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
								 .Create();
            var utcNow = DateTime.UtcNow;
			var existingTicket = Fixture.Build<TicketEntity>()
										.Without(x => x.Attachments)
										.Without(x => x.Comments)
										.Without(x => x.Category)
										.Without(x => x.Tasks)
										.Without(x => x.InsightId)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
										.With(x => x.SiteId, siteId)
										.With(x => x.TemplateId, (Guid?)null)
										.With(x => x.Status, (int)TicketStatusEnum.Reassign)
										.With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
			using (var client = server.CreateClient(null))
			{
				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
					.ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

				server.Arrange().GetSiteApi()
					.SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
					.ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

				server.Arrange().GetDirectoryApi()
					.SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
					.ReturnsJson(Fixture.CreateMany<User>(3).ToList());

				var arrangement = server.Arrange();
				arrangement.SetCurrentDateTime(utcNow);
				var db = arrangement.CreateDbContext<WorkflowContext>();
				db.Tickets.Add(existingTicket);
				db.SaveChanges();
				var createdTicket = db.Tickets.First();
				var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

				response.StatusCode.Should().Be(HttpStatusCode.OK);
				db = server.Assert().GetDbContext<WorkflowContext>();
				db.Tickets.Should().HaveCount(1);
				var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
				updatedTicket.Should().NotBeNull();

				var result = await response.Content.ReadAsAsync<TicketDetailDto>();
				var expectedTicket = TicketEntity.MapToModel(updatedTicket);
				expectedTicket.AssigneeName = "Unassigned";
				var expectedTicketDto = TicketDetailDto.MapFromModel(expectedTicket, server.Assert().GetImagePathHelper());
				result.Should().BeEquivalentTo(expectedTicketDto, config => config
							   .Excluding(x => x.CanResolveInsight));


				
				var auditTrails = db.AuditTrails.Where(x => x.OperationType == EntityState.Modified.ToString()).ToList();
				auditTrails.Should().HaveCount(0);
				
			}
		}


        [Fact]
        public async Task GivenValidInputWithoutTwinId_UpdateTicket_ReturnsUpdatedTicket()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .Without(x => x.TwinId)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.TwinId.Should().BeEquivalentTo(existingTicket.TwinId);              

            }
        }

        [Fact]
        public async Task RemoveTwinId_UpdateTicket_ReturnsUpdatedTicketWithEmptyTwinId()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.TwinId,string.Empty)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Tickets.Should().HaveCount(1);
                var updatedTicket = db.Tickets.Include(x => x.Attachments).Include(x => x.Comments).Include(x => x.Tasks).First();
                updatedTicket.Should().NotBeNull();
                updatedTicket.TwinId.Should().BeEmpty();

            }
        }

        [Fact]
        public async Task MappedEnabledAndStatusTransitionIsInvalid_UpdateTicket_ReturnBadRequest()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.TwinId, string.Empty)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var result  = await response.Content.ReadAsStringAsync();
                result.Should().Contain("Invalid status transition from Reassign to InProgress");


            }
        }
        /// <summary>
        /// ticket update status from InProgress to Reassign
        /// existing ticket status = InProgress
        /// update request status = Reassign
        /// allowed ticket transition FromStatus = InProgress to Reassign
        /// </summary>
        /// <returns>Valid request</returns>
        [Fact]
        public async Task MappedEnabledAndStatusTransitionIsValid_UpdateTicket_ReturnOk()
        {
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var request = Fixture.Build<UpdateTicketRequest>()
                                 .Without(x => x.AssigneeId)
                                 .With(x => x.TwinId, string.Empty)
                                 .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                 .With(x => x.Priority, new Random().Next() % 4 + 1)
                                 .With(x => x.Status, (int)TicketStatusEnum.Reassign)
                                 .With(x => x.Tasks, new List<TicketTask>())
                                 .With(x => x.CustomProperties, new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } })
                                 .With(x => x.ExtendableSearchablePropertyKeys, new List<string> { "prop1" })
                                 .Create();
            var utcNow = DateTime.UtcNow;
            var existingTicket = Fixture.Build<TicketEntity>()
                                        .Without(x => x.Attachments)
                                        .Without(x => x.Comments)
                                        .Without(x => x.Category)
                                        .Without(x => x.Tasks)
                                        .Without(x => x.JobType)
                                        .Without(x => x.Diagnostics)
                                        .With(x => x.Id, ticketId)
                                        .With(x => x.SiteId, siteId)
                                        .With(x => x.TemplateId, (Guid?)null)
                                        .With(x => x.Status, (int)TicketStatusEnum.InProgress)
                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                        .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
            using (var client = server.CreateClient(null))
            {
                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}")
                    .ReturnsJson(Fixture.Build<Customer>().With(x => x.Id, existingTicket.CustomerId).Create());

                server.Arrange().GetSiteApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(Fixture.Build<Site>().With(x => x.Id, siteId).Create());

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"customers/{existingTicket.CustomerId}/users")
                    .ReturnsJson(Fixture.CreateMany<User>(3).ToList());

                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.TicketStatusTransitions.Add(new TicketStatusTransitionsEntity
                {
                    Id = Guid.NewGuid(),
                    FromStatus = (int)TicketStatusEnum.InProgress,
                    ToStatus = (int)TicketStatusEnum.Reassign
                });
                db.Tickets.Add(existingTicket);
                db.SaveChanges();

                var response = await client.PutAsJsonAsync($"sites/{siteId}/tickets/{ticketId}", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);

                db = server.Assert().GetDbContext<WorkflowContext>();
                var ticket = db.Tickets.FirstOrDefault(x => x.Id == existingTicket.Id);
                ticket.Status.Should().Be((int)TicketStatusEnum.Reassign);




            }
        }

    }
}
