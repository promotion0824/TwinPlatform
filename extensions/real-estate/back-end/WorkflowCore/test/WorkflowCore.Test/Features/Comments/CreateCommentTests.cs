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
using System.Linq;
using WorkflowCore.Models;
using Willow.Infrastructure;
using WorkflowCore.Controllers.Request;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorkflowCore.Test.Features.Reporters
{
    public class CreateCommentTests : BaseInMemoryTest
    {
        public CreateCommentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateComment_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/comments", new object());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ValidInput_CreateComment_ReturnsCreatedComment()
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
            var request = new CreateCommentRequest
            {
                Text = Fixture.Create<string>(),
                CreatorType = CommentCreatorType.CustomerUser,
                CreatorId = Guid.NewGuid()
            };
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}/comments", request);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Comments.Should().HaveCount(1);
                var commentEntity = db.Comments.First();
                commentEntity.TicketId.Should().Be(ticket.Id);
                commentEntity.Text.Should().Be(request.Text);
                commentEntity.CreatorType.Should().Be(request.CreatorType);
                commentEntity.CreatorId.Should().Be(request.CreatorId);
                commentEntity.CreatedDate.Should().Be(utcNow);
                var result = await response.Content.ReadAsAsync<CommentDto>();
                result.Should().BeEquivalentTo(CommentDto.MapFromModel(CommentEntity.MapToModel(commentEntity)));
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_CreateComment_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/comments", new CreateReporterRequest());

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenSiteIdDoesNotMatchTicketSiteId_CreateComment_ReturnsBadRequest()
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
            var givenSiteId = Guid.NewGuid();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.SaveChanges();

                var response = await client.PostAsJsonAsync($"sites/{givenSiteId}/tickets/{ticket.Id}/comments", new CreateReporterRequest());

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("TicketId", out _));
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("SiteId", out _));
            }
        }
    }
}
