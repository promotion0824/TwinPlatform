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
using WorkflowCore.Services.Apis;
using WorkflowCore.Models;
using Willow.Infrastructure;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WorkflowCore.Test.Features.Reporters
{
    public class CreateAttachmentTests : BaseInMemoryTest
    {
        public CreateAttachmentTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task TokenIsNotGiven_CreateAttachment_ReturnsUnauthorized()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient())
            {
                var result = await client.PostAsJsonAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/attachments", new object());
                result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task ValidInput_CreateAttachment_ReturnsCreatedAttachment()
        {
            var ticket = Fixture.Build<TicketEntity>()
                                .Without(x => x.Comments)
                                .Without(x => x.JobType)
                                .Without(x => x.Diagnostics)
                                .Without(x => x.Attachments)
                                .Without(x => x.Category)
                                .Without(x => x.Tasks)
                                .With(x => x.CustomProperties, JsonConvert.SerializeObject(new Dictionary<string, string> { { "prop1", "val1" }, { "prop2", "val2" } }))
                                .With(x => x.ExtendableSearchablePropertyKeys, JsonConvert.SerializeObject(new List<string> { "prop1" }))
                                .Create();
            var attachmentFileName = $"{Guid.NewGuid()}.jpg";
            var attachmentContent = Fixture.CreateMany<byte>(10).ToArray();
            var imageDescriptor = Fixture.Create<OriginalImageDescriptor>();
            var utcNow = DateTime.UtcNow;

            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var arrangement = server.Arrange();
                arrangement.SetCurrentDateTime(utcNow);
                var db = arrangement.CreateDbContext<WorkflowContext>();
                db.Tickets.Add(ticket);
                db.SaveChanges();
                arrangement.GetImageHubHttpHandler()
                    .SetupRequestWithExpectedFileContent(HttpMethod.Post, $"{ticket.CustomerId}/sites/{ticket.SiteId}/tickets/{ticket.Id}", "imageFile", attachmentContent)
                    .ReturnsJson(imageDescriptor);

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(attachmentContent)
                {
                    Headers = { ContentLength = attachmentContent.Length }
                };
                dataContent.Add(fileContent, "attachmentFile", attachmentFileName);
                var response = await client.PostAsync($"sites/{ticket.SiteId}/tickets/{ticket.Id}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                db = server.Assert().GetDbContext<WorkflowContext>();
                db.Attachments.Should().HaveCount(1);
                var attachmentEntity = db.Attachments.First();
                attachmentEntity.Id.Should().Be(imageDescriptor.ImageId);
                attachmentEntity.TicketId.Should().Be(ticket.Id);
                attachmentEntity.Type.Should().Be(AttachmentType.Image);
                attachmentEntity.FileName.Should().Be(attachmentFileName);
                attachmentEntity.CreatedDate.Should().Be(utcNow);
                var result = await response.Content.ReadAsAsync<AttachmentDto>();
                var expectedAttachmentDto = AttachmentDto.MapFromTicketModel(
                    AttachmentEntity.MapToModel(attachmentEntity),
                    server.Assert().GetImagePathHelper(),
                    TicketEntity.MapToModel(ticket));
                result.Should().BeEquivalentTo(expectedAttachmentDto);
            }
        }

        [Fact]
        public async Task TicketDoesNotExist_CreateAttachment_ReturnsNotFound()
        {
            await using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
            using (var client = server.CreateClient(null))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(Fixture.CreateMany<byte>(10).ToArray())
                {
                    Headers = { ContentLength = 10 }
                };
                dataContent.Add(fileContent, "attachmentFile", "abc.jpg");
                var response = await client.PostAsync($"sites/{Guid.NewGuid()}/tickets/{Guid.NewGuid()}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GivenSiteIdDoesNotMatchTicketSiteId_CreateAttachment_ReturnsBadRequest()
        {
            var ticket = Fixture.Build<TicketEntity>()
                                .Without(x => x.Comments)
                                .Without(x => x.JobType)
                                .Without(x => x.Diagnostics)
                                .Without(x => x.Attachments)
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

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(Fixture.CreateMany<byte>(10).ToArray())
                {
                    Headers = { ContentLength = 10 }
                };
                dataContent.Add(fileContent, "attachmentFile", "abc.jpg");
                var response = await client.PostAsync($"sites/{givenSiteId}/tickets/{ticket.Id}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                var error = await response.Content.ReadAsErrorResponseAsync();
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("TicketId", out _));
                Assert.True(((System.Text.Json.JsonElement)error.Data).TryGetProperty("SiteId", out _));
            }
        }
    }
}
