using AutoFixture;
using FluentAssertions;
using MobileXL.Dto;
using MobileXL.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace MobileXL.Test.Features.Workflow.Attachments
{
    public class CreateAttachmentTests : BaseInMemoryTest
    {
        public CreateAttachmentTests(ITestOutputHelper output) : base(output)
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
        public async Task ValidInput_CreateAttachment_ReturnsCreatedAttachment()
        {
            var customerUser = Fixture.Create<CustomerUser>();
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var attachmentFileName = $"{Guid.NewGuid()}.jpg";
            var attachmentContent = GetTestImageBytes();
            var expectedAttachment = Fixture.Create<Attachment>();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(customerUser.Id, Permissions.ViewSites, siteId))
            {
                server.Arrange().GetWorkflowApi()
                    .SetupRequestWithExpectedFileContent(HttpMethod.Post, $"sites/{siteId}/tickets/{ticketId}/attachments", "attachmentFile", attachmentContent)
                    .ReturnsJson(expectedAttachment);

                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(attachmentContent)
                {
                    Headers = { ContentLength = attachmentContent.Length }
                };
                dataContent.Add(fileContent, "attachmentFile", attachmentFileName);
                var response = await client.PostAsync($"sites/{siteId}/tickets/{ticketId}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<AttachmentDto>();
                result.Should().BeEquivalentTo(AttachmentDto.MapFromModel(expectedAttachment, server.Assert().GetImageUrlHelper()));
            }
        }

        [Fact]
        public async Task InvalidImage_CreateAttachment_ReturnsValidationError()
        {
            var customerUser = Fixture.Create<CustomerUser>();
            var siteId = Guid.NewGuid();
            var ticketId = Guid.NewGuid();
            var attachmentFileName = $"{Guid.NewGuid()}.jpg";
            var attachmentContent = Fixture.CreateMany<byte>(10).ToArray();

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserPermissionOnSite(customerUser.Id, Permissions.ViewSites, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(attachmentContent)
                {
                    Headers = { ContentLength = attachmentContent.Length }
                };
                dataContent.Add(fileContent, "attachmentFile", attachmentFileName);
                var response = await client.PostAsync($"sites/{siteId}/tickets/{ticketId}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
                var error = await response.Content.ReadAsAsync<ValidationError>();
                error.Items.Should().HaveCount(1);
                error.Items[0].Name.Should().Be("attachmentFile");
            }
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_CreateAttachment_ReturnsForbidden()
        {
            var siteId = Guid.NewGuid();
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClientWithCustomerUserDeniedPermissionOnSite(null, Permissions.ViewSites, siteId))
            {
                var dataContent = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(new byte[10])
                {
                    Headers = { ContentLength = 10 }
                };
                dataContent.Add(fileContent, "attachmentFile", "abc.jpg");
                var response = await client.PostAsync($"sites/{siteId}/tickets/{Guid.NewGuid()}/attachments", dataContent);

                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

    }
}