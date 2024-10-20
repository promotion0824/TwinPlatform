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
using Willow.Infrastructure;
using Moq.Contrib.HttpClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorkflowCore.Test.Features.Reporters
{
    public class DeleteAttachmentTests : BaseInMemoryTest
    {
        public DeleteAttachmentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_DeleteAttachment_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.DeleteAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}");
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ValidInput_DeleteAttachment_ReturnsNoContent()
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
            var attachment = Fixture.Build<AttachmentEntity>()
                                    .Without(x => x.Ticket)
                                    .With(x => x.TicketId, ticket.Id)
                                    .Create();

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.Attachments.Add(attachment);
                db.SaveChanges();
                arrangement.GetImageHubHttpHandler()
                    .SetupRequest(HttpMethod.Delete, $"{ticket.CustomerId}/sites/{ticket.SiteId}/tickets/{ticket.Id}/{attachment.Id}")
                    .ReturnsResponse(HttpStatusCode.NoContent);

                var response = await client.DeleteAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}/attachments/{attachment.Id}");

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Attachments.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_DeleteAttachment_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var response = await client.DeleteAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenSiteIdDoesNotMatchTicketSiteId_DeleteAttachment_ReturnsBadRequest()
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

                var response = await client.DeleteAsync($"sites/{givenSiteId}/tickets/{ticket.Id}/attachments/{Guid.NewGuid()}");

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("TicketId", out _));
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("SiteId", out _));
            }
        }
    }
}
