using Azure;
using Azure.Storage.Blobs.Models;
using DigitalTwinCore.Controllers;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Services.DocumentsService
{
    public class DocumentsServiceTests : BaseInMemoryTest
    {
        private DigitalTwinCore.Services.DocumentsService documentsService;
        private Mock<IContentTypeProvider> contentTypeProviderMock;
        private Mock<IHashCreator> hashCreatorMock;
        private Mock<IBlobStore> blobStore;
        private Mock<IDigitalTwinServiceProvider> digitalTwinServiceProviderMock;
        private Mock<IGuidWrapper> guidWrapperMock;
        private Mock<IDigitalTwinService> digitalTwinServiceMock;

        public DocumentsServiceTests(ITestOutputHelper output) : base(output)
        {
            contentTypeProviderMock = new Mock<IContentTypeProvider>();
            hashCreatorMock = new Mock<IHashCreator>();
            blobStore = new Mock<IBlobStore>();
            digitalTwinServiceProviderMock = new Mock<IDigitalTwinServiceProvider>();
            digitalTwinServiceMock = new Mock<IDigitalTwinService>();
            guidWrapperMock = new Mock<IGuidWrapper>();

            digitalTwinServiceProviderMock.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>())).ReturnsAsync(digitalTwinServiceMock.Object);

            documentsService = new DigitalTwinCore.Services.DocumentsService(contentTypeProviderMock.Object, hashCreatorMock.Object, blobStore.Object,
                digitalTwinServiceProviderMock.Object, guidWrapperMock.Object, "http://TheUrl");
        }

        [Fact]
        public async Task GetUploadedFileName_WithExistingFile_ReUseAllowed_ShouldReturnExistingFileName()
        {
            var duplicateName = "TheDuplicateFileName";
            blobStore.Setup(x => x.Enumerate(It.IsAny<Func<string, Task>>(), false, It.IsAny<object>())).Callback<Func<string, Task>, bool, object>((fnEach, asynch, tags) => fnEach(duplicateName));
            hashCreatorMock.Setup(x => x.Create(It.IsAny<Stream>())).Returns(Encoding.UTF8.GetBytes("Dummy bytes"));

            var name = await documentsService.UploadFile("mimeType", GetDummyFile("TheFile"), true);

            Assert.Equal(duplicateName, name);
            blobStore.Verify(s => s.Put(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetUploadedFileName_WithExistingFile_ReUseDisabled_ShouldReturnNewFileName()
        {
            var uploadedFileName = "TheFile";

            hashCreatorMock.Setup(x => x.Create(It.IsAny<Stream>())).Returns(Encoding.UTF8.GetBytes("Dummy bytes"));

            var name = await documentsService.UploadFile("mimeType", GetDummyFile(uploadedFileName), false);

            Assert.Equal(uploadedFileName, name);

            blobStore.Verify(s => s.Put(uploadedFileName, It.IsAny<Stream>(), It.IsAny<object>()), Times.Once);
            blobStore.Verify(x => x.Enumerate(It.IsAny<Func<string, Task>>(), false, It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task GetUploadedFileName_WithExistingFile_ReUseDisabled_BlobNotCreated_ShouldThrowException()
        {
            var uploadedFileName = "TheFile";

            hashCreatorMock.Setup(x => x.Create(It.IsAny<Stream>())).Returns(Encoding.UTF8.GetBytes("Dummy bytes"));
            blobStore.Setup(s => s.Put(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<object>())).ThrowsAsync(new Exception("blah"));

            await Assert.ThrowsAsync<Exception>(() => documentsService.UploadFile("mimeType", GetDummyFile(uploadedFileName), false));
        }

        [Fact]
        public async Task CreateFileTwin_WithExistingDocumentId_ShouldReturnSameDocumentId()
        {
            var documentId = "TheID";
            digitalTwinServiceMock.Setup(x => x.AddOrUpdateTwinAsync(It.IsAny<Twin>(), true, null)).ReturnsAsync(new TwinWithRelationships { Id = documentId });
            var request = new CreateDocumentRequest
            {
                Id = documentId
            };

            var result = await documentsService.CreateFileTwin(Guid.NewGuid(), request, GetDummyFile("TheFile"), "Blob");

            Assert.Equal(documentId, result.Id);
        }

        [Fact]
        public async Task CreateFileTwin_WithExistingDocumentId_ShouldReturnNewDocumentId()
        {
            var documentId = Guid.NewGuid();
            digitalTwinServiceMock.Setup(x => x.AddOrUpdateTwinAsync(It.IsAny<Twin>(), true, null)).ReturnsAsync(new TwinWithRelationships { Id = documentId.ToString() });
            guidWrapperMock.Setup(x => x.NewGuid()).Returns(documentId);

            var result = await documentsService.CreateFileTwin(Guid.NewGuid(), new CreateDocumentRequest(), GetDummyFile("TheFile"), "Blob");

            Assert.Equal(documentId.ToString(), result.Id);
        }

        [Fact]
        public async Task GetTwinByUniqueIdAsync_ShouldReturnValidTwin()
        {
            var id = Guid.NewGuid();
            digitalTwinServiceMock.Setup(x => x.GetTwinByUniqueIdAsync(It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(new TwinWithRelationships { Id = id.ToString() });

            var result = await documentsService.GetTwinByUniqueIdAsync(Guid.NewGuid(), id);

            Assert.Equal(id.ToString(), result.Id);
        }

        [Fact]
        public async Task AddRelationshipAsync_ShouldReturnValidTwinRelationship()
        {
            var twinId = Guid.NewGuid().ToString();
            var documentId = Guid.NewGuid().ToString();
            digitalTwinServiceMock.Setup(x => x.AddRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Relationship>())).ReturnsAsync(new TwinRelationship
            {
                Source = new TwinWithRelationships { Id = twinId },
                Target = new TwinWithRelationships { Id = documentId, Metadata = new TwinMetadata() },
                CustomProperties = new Dictionary<string, object>()
            });

            var result = await documentsService.AddRelationshipAsync(Guid.NewGuid(), twinId, documentId);

            Assert.Equal(twinId, result.SourceId);
            Assert.Equal(documentId, result.TargetId);
        }

        private IFormFile GetDummyFile(string fileName)
        {
            var formFile = new Mock<IFormFile>();
            formFile.Setup(ff => ff.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
              .Returns<Stream, CancellationToken>((s, ct) =>
              {
                  byte[] buffer = Encoding.UTF8.GetBytes("Dummy file");
                  s.Write(buffer, 0, buffer.Length);
                  return Task.CompletedTask;
              });

            formFile.Setup(x => x.FileName).Returns(fileName);

            return formFile.Object;
        }
    }
}
