using PlatformPortalXL.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using System.IO;
using System.Text;
using System.Collections.Generic;
using PlatformPortalXL.Dto;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.Features.Assets;
using Moq;
using PlatformPortalXL.Services;
using PlatformPortalXL.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading;
using PlatformPortalXL.Features.Pilot;
using DigitalTwinCore.DTO;
using AutoFixture;
using Willow.Common;
using Willow.Directory.Models;

namespace PlatformPortalXL.Test.Features.Documents
{
    public class DocumentsControllerTests : BaseInMemoryTest
    {
        private DocumentsController documentsController;
        private Mock<IAccessControlService> accessControlServiceMock;
        private Mock<IDigitalTwinApiService> digitalTwinApiServiceMock;
        private Mock<IControllerHelper> controllerHelperMock;


        public DocumentsControllerTests(ITestOutputHelper output) : base(output)
        {
            accessControlServiceMock = new Mock<IAccessControlService>();
            digitalTwinApiServiceMock = new Mock<IDigitalTwinApiService>();
            controllerHelperMock = new Mock<IControllerHelper>();
            documentsController = new DocumentsController(accessControlServiceMock.Object, digitalTwinApiServiceMock.Object, controllerHelperMock.Object);
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_LinkDocumentToTwin_ThorwsUnauthorizedAccessException()
        {
            await ValidatePermissionsTest(() => documentsController.LinkDocumentToTwin(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_DeleteLinkDocumentToTwin_ThorwsUnauthorizedAccessException()
        {
            await ValidatePermissionsTest(() => documentsController.DeleteLinkDocumentToTwin(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_UploadDocuments_ThorwsUnauthorizedAccessException()
        {
            await ValidatePermissionsTest(() => documentsController.PostAsync(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task UserDoesNotHaveCorrectPermission_GetDocumentStream_ThorwsUnauthorizedAccessException()
        {
            await ValidatePermissionsTest(() => documentsController.GetDocumentStream(Guid.NewGuid(), string.Empty));
        }

        private async Task ValidatePermissionsTest(Func<Task> action)
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .Throws(new UnauthorizedAccessException().WithData(new { UserId = Guid.NewGuid(), string.Empty, RoleResourceType.Site, ResourceId = Guid.NewGuid() }));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => action());
        }

        [Fact]
        public async Task UserWithCorrectPermission_EmptyFiles_UploadDocuments_ThrowsBadRequestException()
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));

            await Assert.ThrowsAsync<ArgumentNullException>(() => documentsController.PostAsync(Guid.NewGuid(), new CreateDocumentRequest()));
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "00600000-0000-0000-0000-000001552528")]
        [InlineData("00600000-0000-0000-0000-000001552528", "00000000-0000-0000-0000-000000000000")]
        public async Task UserWithCorrectPermission_EmptyUniqueIds_LinkDocumentToTwin_ThrowsBadRequestException(string twinUniqueId, string documentUnqueId)
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));

            await Assert.ThrowsAsync<ArgumentNullException>(() => documentsController.LinkDocumentToTwin(Guid.NewGuid(), Guid.Parse(twinUniqueId), Guid.Parse(documentUnqueId)));
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "00600000-0000-0000-0000-000001552528")]
        [InlineData("00600000-0000-0000-0000-000001552528", "00000000-0000-0000-0000-000000000000")]
        public async Task UserWithCorrectPermission_EmptyUniqueIds_DeleteLinkDocumentToTwin_ThrowsBadRequestException(string twinUniqueId, string documentUnqueId)
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));

            await Assert.ThrowsAsync<ArgumentNullException>(() => documentsController.DeleteLinkDocumentToTwin(Guid.NewGuid(), Guid.Parse(twinUniqueId), Guid.Parse(documentUnqueId)));
        }

        [Fact]
        public async Task UserWithCorrectPermission_DeleteLinkDocumentToTwin_ShouldReturnNoContent()
        {
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));
            digitalTwinApiServiceMock.Setup(x => x.DeleteDocumentLinkAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));


            var response = await documentsController.DeleteLinkDocumentToTwin(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var result = response as NoContentResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UserWithCorrectPermission_LinkDocumentToTwin_ShouldReturnRelationship()
        {
            var twinUniqueId = Guid.NewGuid();
            var documentUniqueId = Guid.NewGuid();
            var relationShipDto = new RelationshipDto { Id = "TheId", Name = "TheName", SourceId = twinUniqueId.ToString(), TargetId = documentUniqueId.ToString() };
            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));
            digitalTwinApiServiceMock.Setup(x => x.LinkDocumentToTwinAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(relationShipDto);

            var response = await documentsController.LinkDocumentToTwin(Guid.NewGuid(), twinUniqueId, documentUniqueId);
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(relationShipDto);
        }

        [Fact]
        public async Task UploadDocuments_WithValidFormFile_ShouldGenerateValidResponse()
        {
            var fileId = Guid.NewGuid();
            var url = "http://MyUrl.com";
            var model = "themodel";
            var uniqueId = Guid.NewGuid();
            var digitalTwinFile = new DigitalTwinFile { 
                Id = fileId, 
                CustomProperties = new DigitalTwinFileCustomProperties { Url = new Uri(url) }, 
                DisplayName = fileId.ToString(), 
                Metadata = new DigitalTwinFileMetadata { ModelId = model },
                UniqueId = uniqueId
            };

            var expectedDto = new DocumentTwinDto { DisplayName = fileId.ToString(), Id = fileId, ModelId = model, Url = new Uri(url), UniqueId = uniqueId };
            var files = new List<DigitalTwinFile> { digitalTwinFile };
            var body = GetDummyFile();
            var formCollection = new FormFileCollection { body };
            var request = Fixture.Build<CreateDocumentRequest>().With(x => x.formFiles, formCollection).Create();

            var siteId = Guid.NewGuid();

            controllerHelperMock.Setup(x => x.GetCurrentUserId(It.IsAny<ControllerBase>())).Returns(Guid.NewGuid());
            accessControlServiceMock.Setup(x => x.EnsureAccessSite(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()));
            digitalTwinApiServiceMock.Setup(x => x.UploadDocumentAsync(It.IsAny<Guid>(), It.IsAny<CreateDocumentRequest>()))
                .ReturnsAsync(files);

            var response = await documentsController.PostAsync(siteId, request);
            var result = response.Result as OkObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(new List<DocumentTwinDto> { expectedDto });
        }

        private IFormFile GetDummyFile()
        {
            var formFile = new Mock<IFormFile>();
            formFile.Setup(ff => ff.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns<Stream, CancellationToken>((s, ct) =>
              {
                  byte[] buffer = Encoding.UTF8.GetBytes("Dummy file");
                  s.Write(buffer, 0, buffer.Length);
                  return Task.CompletedTask;
              });

            return formFile.Object;
        }
    }
}
