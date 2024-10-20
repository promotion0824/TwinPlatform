using WorkflowCore.Dto;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using WorkflowCore.Models;
using System.Linq;
using Willow.Infrastructure;
using FluentAssertions.Extensions;
using Newtonsoft.Json;

namespace WorkflowCore.Test.Features.Tickets
{
    public class GetSiteTicketsTests : BaseInMemoryTest
    {
        public GetSiteTicketsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_GetSiteTickets_RequiresAuthorization()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task IssueTypeIsProvidedButIssueIdIsNotProvided_GetSiteTickets_ReturnsBadRequest()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.GetAsync($"sites/{Guid.NewGuid()}/tickets?issueType={IssueType.Equipment}");
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                error.Message.Should().Contain("issueId");
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsBelongingToTheGivenSite()
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .Without(x => x.Tasks)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);
            var scheduledTickets = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Occurrence, 22)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server, siteId);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.Tickets.AddRange(scheduledTickets);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/tickets");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsScheduledTicketsBelongingToTheGivenSite()
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(9);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 22)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(11);
            var scheduledTickets = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Tasks, new List<TicketTaskEntity>())
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Occurrence, 22)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(12);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.Tickets.AddRange(scheduledTickets);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?scheduled=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(scheduledTickets)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTicketsWithStatuses_ReturnsTicketsInTheGivenStatuses()
        {
            var siteId = Guid.NewGuid();
            var status = (int)Fixture.Create<TicketStatusEnum>();
            var ticketEntitiesWithGivenStatus = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Status, status)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);
            var ticketEntitiesForOtherStatus = Fixture.Build<TicketEntity>()
                                                      .Without(x => x.Attachments)
                                                      .Without(x => x.Comments)
                                                      .Without(x => x.Category)
                                                      .Without(x => x.Tasks)
                                                      .Without(x => x.JobType)
                                                      .Without(x => x.Diagnostics)
                                                      .With(x => x.SiteId, siteId)
                                                      .With(x => x.Occurrence, 0)
                                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                      .CreateMany(10)
                                                      .Where(x => x.Status != status);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesWithGivenStatus);
                db.Tickets.AddRange(ticketEntitiesForOtherStatus);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?statuses={status}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesWithGivenStatus)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTicketsWithIssueId_ReturnsTicketsAssociatedToThatIssueId()
        {
            var siteId = Guid.NewGuid();
            var issueType = Fixture.Create<IssueType>();
            var issueId = Guid.NewGuid();
            var ticketEntitiesWithGivenIssue = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.IssueType, issueType)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.IssueId, issueId)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);
            var ticketEntitiesForOtherIssue = Fixture.Build<TicketEntity>()
                                                      .Without(x => x.Attachments)
                                                      .Without(x => x.Comments)
                                                      .Without(x => x.Category)
                                                      .Without(x => x.Tasks)
                                                      .Without(x => x.JobType)
                                                      .Without(x => x.Diagnostics)
                                                      .With(x => x.Occurrence, 0)
                                                      .With(x => x.SiteId, siteId)
                                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                      .CreateMany(20)
                                                      .Where(x => x.IssueType != issueType && x.IssueId != issueId);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesWithGivenIssue);
                db.Tickets.AddRange(ticketEntitiesForOtherIssue);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?issueType={issueType}&issueId={issueId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesWithGivenIssue)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTicketsWithAssigneeId_ReturnsTicketsAssignedToThatAssignee()
        {
            var siteId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var ticketEntitiesForGivenAssignee = Fixture.Build<TicketEntity>()
                                                        .Without(x => x.Attachments)
                                                        .Without(x => x.Comments)
                                                        .Without(x => x.Category)
                                                        .Without(x => x.Tasks)
                                                        .Without(x => x.JobType)
                                                        .Without(x => x.Diagnostics)
                                                        .With(x => x.AssigneeId, assigneeId)
                                                        .With(x => x.SiteId, siteId)
                                                        .With(x => x.Occurrence, 0)
                                                        .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                        .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                        .CreateMany(10);
            var ticketEntitiesForOtherAssignees = Fixture.Build<TicketEntity>()
                                                         .Without(x => x.Attachments)
                                                         .Without(x => x.Comments)
                                                         .Without(x => x.Category)
                                                         .Without(x => x.Tasks)
                                                         .Without(x => x.JobType)
                                                         .Without(x => x.Diagnostics)
                                                         .With(x => x.Occurrence, 0)
                                                         .With(x => x.SiteId, siteId)
                                                         .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                         .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                         .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForGivenAssignee);
                db.Tickets.AddRange(ticketEntitiesForOtherAssignees);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?assigneeId={assigneeId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForGivenAssignee)));
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TicketsExist_GetSiteTicketsWithUnassigned_ReturnsUnassignedTickets(bool queryUnassignedTickets)
        {
            var siteId = Guid.NewGuid();
            var assigneeId = Guid.NewGuid();
            var ticketEntitiesUnassigned = Fixture.Build<TicketEntity>()
                                                  .Without(x => x.Attachments)
                                                  .Without(x => x.Comments)
                                                  .Without(x => x.Category)
                                                  .Without(x => x.Tasks)
                                                  .Without(x => x.JobType)
                                                  .Without(x => x.Diagnostics)
                                                  .With(x => x.AssigneeType, AssigneeType.NoAssignee)
                                                  .With(x => x.SiteId, siteId)
                                                  .With(x => x.Occurrence, 0)
                                                  .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                  .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                 .CreateMany(10);
            var ticketEntitiesAssigned = Fixture.Build<TicketEntity>()
                                                .Without(x => x.Attachments)
                                                .Without(x => x.Comments)
                                                .Without(x => x.Category)
                                                .Without(x => x.Tasks)
                                                .Without(x => x.JobType)
                                                .Without(x => x.Diagnostics)
                                                .With(x => x.AssigneeType, AssigneeType.CustomerUser)
                                                .With(x => x.SiteId, siteId)
                                                .With(x => x.Occurrence, 0)
                                                .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                .CreateMany(10);
            var expectedTicketEntities = queryUnassignedTickets ? ticketEntitiesUnassigned : ticketEntitiesAssigned;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesUnassigned);
                db.Tickets.AddRange(ticketEntitiesAssigned);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?unassigned={queryUnassignedTickets}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(expectedTicketEntities)));
            }
        }
    
        [Fact]
        public async Task TicketsExist_GetPagedSiteTicketsWithStatuses_ReturnsPagedTickets()
        {
            var siteId = Guid.NewGuid();
            var page = 1;
            var pageSize = 3;
            var status = (int)Fixture.Create<TicketStatusEnum>();
            var ticketEntitiesWithGivenStatus = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Status, status)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);
            var ticketEntitiesForOtherStatus = Fixture.Build<TicketEntity>()
                                                      .Without(x => x.Attachments)
                                                      .Without(x => x.Comments)
                                                      .Without(x => x.Category)
                                                      .Without(x => x.Tasks)
                                                      .Without(x => x.JobType)
                                                      .Without(x => x.Diagnostics)
                                                      .With(x => x.SiteId, siteId)
                                                      .With(x => x.Occurrence, 0)
                                                      .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                      .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                      .CreateMany(10)
                                                      .Where(x => x.Status != status);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesWithGivenStatus);
                db.Tickets.AddRange(ticketEntitiesForOtherStatus);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?statuses={status}&page={page}&pageSize={pageSize}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                var ticketsWithGivenStatuses = TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesWithGivenStatus));
                result.Should().BeEquivalentTo(ticketsWithGivenStatuses.Take(pageSize));
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByStatus_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "Status";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.Select(x => x.Status).OrderBy(x => x).ToList();
                var orderedResults = result.Select(x => x.Status).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).Status.Should().Be(orderedTicketEntities.ElementAt(i));
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByDate_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "CreatedDate";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.Select(x => x.ComputedCreatedDate).OrderBy(x => x).ToList();
                var orderedResults = result.Select(x => x.ComputedCreatedDate).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).ComputedCreatedDate.Should().BeCloseTo(orderedTicketEntities.ElementAt(i), 1000.Seconds()); // https://improveandrepeat.com/2021/08/how-to-check-if-two-datetimes-are-close-in-fluent-assertions/
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByDateDesc_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "CreatedDate desc";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.Select(x => x.ComputedCreatedDate).OrderByDescending(x => x).ToList();
                var orderedResults = result.Select(x => x.ComputedCreatedDate).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).ComputedCreatedDate.Should().BeCloseTo(orderedTicketEntities.ElementAt(i), 1000.Seconds()); // https://improveandrepeat.com/2021/08/how-to-check-if-two-datetimes-are-close-in-fluent-assertions/
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByAssignedTo_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "AssignedTo";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.Select(x => x.AssigneeName).OrderBy(x => x).ToList();
                var orderedResults = result.Select(x => x.AssigneeName).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).AssigneeName.Should().Be(orderedTicketEntities.ElementAt(i));
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByDateAndAssignedTo_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "CreatedDate, AssignedTo desc";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.OrderBy(x => x.ComputedCreatedDate).ThenByDescending(x => x.AssigneeName).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).Id.Should().Be(orderedTicketEntities.ElementAt(i).Id);
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsByExternalId()
        {
            var siteId = Guid.NewGuid();
            var externalId = "test";
            var otherExternalId = "other";
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.ExternalId, externalId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var otherTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.ExternalId, otherExternalId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.ExternalId, externalId)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(otherTicketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?externalId={externalId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsByFloorId()
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.FloorCode, "L5")
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var otherTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.ExternalId, "L4")
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.ExternalId, "L5")
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(otherTicketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?floorId=L5");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }


        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsBySourceId()
        {
            var siteId = Guid.NewGuid();
            var sourceId = Guid.NewGuid();
            var otherSourceId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.SourceId, sourceId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var otherTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.SourceId, otherSourceId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SourceId, sourceId)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(otherTicketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?sourceId={sourceId}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsByCreatedAfter()
        {
            var siteId = Guid.NewGuid();
            var createdAfter = DateTime.UtcNow;
            var ticketsCreatedDate = createdAfter.AddDays(10);
            var otherTicketsCreatedDate = createdAfter.AddDays(-10);
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.CreatedDate, ticketsCreatedDate)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var otherTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.CreatedDate, otherTicketsCreatedDate)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.CreatedDate, ticketsCreatedDate)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(otherTicketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?createdAfter={createdAfter:yyyy-MM-ddTHH:mm:ss}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsBySourceType()
        {
            var siteId = Guid.NewGuid();
            var sourceType = SourceType.App;
            var otherSourceType = SourceType.Dynamics;
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.SourceType, sourceType)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var otherTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.SourceType, otherSourceType)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.SourceType, sourceType)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(otherTicketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?sourceType={sourceType}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByCategory_ReturnsOrderedTickets()
        {
            var siteId = Guid.NewGuid();
            var category = Fixture.Build<TicketCategoryEntity>()
                .With(x => x.SiteId, siteId)
                .Without(x => x.Tickets)
                .CreateMany(3).ToList();
            var orderBy = "Category";
            var ticketEntitiesC1 = Fixture.Build<TicketEntity>()
                                            .Without(x => x.Attachments)
                                            .Without(x => x.Comments)
                                            .Without(x => x.Tasks)
                                            .Without(x => x.JobType)
                                            .Without(x => x.Diagnostics)
                                            .With(x => x.Category, category[0])
                                            .With(x => x.SiteId, siteId)
                                            .With(x => x.Occurrence, 0)
                                            .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                            .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(4);
            var ticketEntitiesC2 = Fixture.Build<TicketEntity>()
                                           .Without(x => x.Attachments)
                                           .Without(x => x.Comments)
                                           .Without(x => x.Tasks)
                                           .Without(x => x.JobType)
                                           .Without(x => x.Diagnostics)
                                           .With(x => x.Category, category[1])
                                           .With(x => x.SiteId, siteId)
                                           .With(x => x.Occurrence, 0)
                                           .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                           .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                           .CreateMany(4);
            var ticketEntitiesC3 = Fixture.Build<TicketEntity>()
                                           .Without(x => x.Attachments)
                                           .Without(x => x.Comments)
                                           .Without(x => x.Tasks)
                                           .Without(x => x.JobType)
                                           .Without(x => x.Diagnostics)
                                           .With(x => x.Category, category[1])
                                           .With(x => x.SiteId, siteId)
                                           .With(x => x.Occurrence, 0)
                                           .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                           .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                           .CreateMany(4);

            var ticketEntities = ticketEntitiesC1.Union(ticketEntitiesC2.Union(ticketEntitiesC3));

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntities)));

                var orderedTicketEntities = ticketEntities.OrderBy(x => x.Category.Name).ToList();

                for (var i = 0; i < orderedTicketEntities.Count(); i++)
                {
                    result.ElementAt(i).Id.Should().Be(orderedTicketEntities.ElementAt(i).Id);
                }
            }
        }

        [Fact]
        public async Task TicketsExist_GetOrderedSiteTicketsByInvalidField_ReturnsError()
        {
            var siteId = Guid.NewGuid();
            var orderBy = "Date";
            var ticketEntities = Fixture.Build<TicketEntity>()
                                                       .Without(x => x.Attachments)
                                                       .Without(x => x.Comments)
                                                       .Without(x => x.Category)
                                                       .Without(x => x.Tasks)
                                                       .Without(x => x.JobType)
                                                       .Without(x => x.Diagnostics)
                                                       .With(x => x.SiteId, siteId)
                                                       .With(x => x.Occurrence, 0)
                                                       .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                       .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                       .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntities);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?&orderBy={orderBy}");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("OrderByField", out _));
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteScheduledTickets_ReturnsScheduledTickets()
        {
            var siteId = Guid.NewGuid();
            var scheduledTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 1)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(3);
            var unscheduledTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(5);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(scheduledTicketEntitiesForSite);
                db.Tickets.AddRange(unscheduledTicketEntitiesForSite);
                db.SaveChanges();

                SetupSite(server, siteId);

                var response = await client.GetAsync($"sites/{siteId}/tickets?scheduled=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().HaveCount(scheduledTicketEntitiesForSite.Count());
            }
        }

        [Fact]
        public async Task TicketsExist_GetSiteScheduledTicketsScheduledFeatureNoEnabled_ReturnsNoTickets()
        {
            var siteId = Guid.NewGuid();
            var scheduledTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 1)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(3);
            var unscheduledTicketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(5);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(scheduledTicketEntitiesForSite);
                db.Tickets.AddRange(unscheduledTicketEntitiesForSite);
                db.SaveChanges();

                server.Arrange().GetDirectoryApi()
                    .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                    .ReturnsJson(new Site { Id = siteId, CustomerId = Guid.NewGuid(), TimezoneId = "Pacific Standard Time", Features = new SiteFeatures { IsScheduledTicketsEnabled = false } });

                var response = await client.GetAsync($"sites/{siteId}/tickets?scheduled=true");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().HaveCount(0);
            }
        }

        /// <summary>
        ///  Test tickets that return extra fields SpaceTwinId, SubStatus and JobType 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TicketsExist_GetSiteTickets_ReturnsTicketsWithExtraFields()
        {
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x=> x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .Without(x => x.Tasks)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);
            var scheduledTickets = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Occurrence, 22)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server, siteId);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.Tickets.AddRange(scheduledTickets);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/tickets");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                result.Should().BeEquivalentTo(TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite)));
            }
        }


         [Fact]
        public async Task TicketsWithMappedSourceExist_GetSiteTickets_ReturnsTicketsAndRenameSourceName()
        {
            // this name match the name configured in ServerFixtureConfigurations.InMemoryWithMappedIntegration
            var cmmsExternalName = "CMMS Name";
            var siteId = Guid.NewGuid();
            var ticketEntitiesForSite = Fixture.Build<TicketEntity>()
                                               .Without(x => x.Attachments)
                                               .Without(x => x.Comments)
                                               .Without(x => x.Category)
                                               .Without(x => x.Tasks)
                                               .Without(x => x.JobType)
                                               .Without(x => x.Diagnostics)
                                               .With(x => x.SiteId, siteId)
                                               .With(x => x.Occurrence, 0)
                                               .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                               .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                               .CreateMany(10)
                                               .ToList();

            var ticketEntitiesWithMappedSource = Fixture.Build<TicketEntity>()
                                              .Without(x => x.Attachments)
                                              .Without(x => x.Comments)
                                              .Without(x => x.Category)
                                              .Without(x => x.Tasks)
                                              .Without(x => x.JobType)
                                              .Without(x => x.Diagnostics)
                                              .With(x => x.SiteId, siteId)
                                              .With(x => x.Occurrence, 0)
                                              .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                              .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                              .With(x => x.SourceType, SourceType.Mapped)
                                              .CreateMany(3)
                                              .ToList();

            ticketEntitiesForSite.AddRange(ticketEntitiesWithMappedSource);
            var ticketEntitiesForOtherSites = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .Without(x => x.Tasks)
                                                     .With(x => x.Occurrence, 0)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);
            var scheduledTickets = Fixture.Build<TicketEntity>()
                                                     .Without(x => x.Attachments)
                                                     .Without(x => x.Comments)
                                                     .Without(x => x.Category)
                                                     .Without(x => x.Tasks)
                                                     .Without(x => x.JobType)
                                                     .Without(x => x.Diagnostics)
                                                     .With(x => x.SiteId, siteId)
                                                     .With(x => x.Occurrence, 22)
                                                     .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                                     .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                                     .CreateMany(10);
            var expectedResult = TicketSimpleDto.MapFromModels(TicketEntity.MapToModels(ticketEntitiesForSite));
            foreach (var ticket in expectedResult)
            {
                if(ticket.SourceType == SourceType.Mapped)
                {
                    ticket.SourceName = cmmsExternalName;
                }
            }
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryWithMappedIntegration))
            using (var client = server.CreateClient(null))
            {
                SetupSite(server, siteId);

                var db = server.Arrange().CreateDbContext<WorkflowContext>();
                db.Tickets.AddRange(ticketEntitiesForSite);
                db.Tickets.AddRange(ticketEntitiesForOtherSites);
                db.Tickets.AddRange(scheduledTickets);
                db.SaveChanges();

                var response = await client.GetAsync($"sites/{siteId}/tickets");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<List<TicketSimpleDto>>();
                
                result.Should().BeEquivalentTo(expectedResult);
                result.Count.Should().Be(13);
                var mappedTicketSourceNames = result.Where(t => ticketEntitiesWithMappedSource.Select(x => x.Id).Contains(t.Id)).Select(t => t.SourceName).ToList();
                mappedTicketSourceNames.Should().AllBeEquivalentTo(cmmsExternalName);




            }
        }
        #region Private Methods

        private void SetupSite(ServerFixture server, Guid siteId)
        {
            server.Arrange().GetDirectoryApi()
                .SetupRequestSequence(HttpMethod.Get, $"sites/{siteId}")
                .ReturnsJson(new Site { Id = siteId, CustomerId = Guid.NewGuid(), TimezoneId = "Pacific Standard Time", Features = new SiteFeatures { IsInspectionEnabled = true, IsScheduledTicketsEnabled = true } });
        }
        #endregion
    }
}
