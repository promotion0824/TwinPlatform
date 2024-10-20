using FluentAssertions;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using WorkflowCore.Entities;
using AutoFixture;
using Willow.Infrastructure;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorkflowCore.Test.Features.Reporters
{
    public class DeleteCommentTests : BaseInMemoryTest
    {
        public DeleteCommentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_DeleteComment_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.DeleteAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/comments/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task CommentExists_DeleteComment_ReturnsNoContent()
        {
            var ticket = Fixture.Build<TicketEntity>()
                                .Without(x => x.Comments)
                                .Without(x => x.Attachments)
                                .Without(x => x.Category)
                                .Without(x => x.JobType)
                                .Without(x => x.Diagnostics)
                                .Without(x => x.Tasks)
                                .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                .Create();
            var comment = Fixture.Build<CommentEntity>()
                                 .Without(x => x.Ticket)
                                 .With(x => x.TicketId, ticket.Id)
                                 .Create();
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.Comments.Add(comment);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}/comments/{comment.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Comments.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_DeleteComment_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/comments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CommentDoesNotExist_DeleteComment_ReturnsNotFound()
        {
            var ticket = Fixture.Build<TicketEntity>()
                                .Without(x => x.Comments)
                                .Without(x => x.Attachments)
                                .Without(x => x.JobType)
                                .Without(x => x.Diagnostics)
                                .Without(x => x.Category)
                                .Without(x => x.Tasks)
                                .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                .Create();
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}/comments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenSiteIdDoesNotMatchTicketSiteId_DeleteComment_ReturnsBadRequest()
        {
            var ticket = Fixture.Build<TicketEntity>()
                                .Without(x => x.Comments)
                                .Without(x => x.Attachments)
                                .Without(x => x.JobType)
                                .Without(x => x.Diagnostics)
                                .Without(x => x.Category)
                                .Without(x => x.Tasks)
                                .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                .Create();
            var givenSiteId = Guid.NewGuid();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.SaveChanges();

                var response = await client.DeleteAsync($"sites/{givenSiteId}/tickets/{ticket.Id}/comments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("TicketId", out _));
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("SiteId", out _));
            }
        }
    }
}
