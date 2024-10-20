using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Willow.Api.Common.Runtime;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.TwinLifecycleManagement.Web.Services;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Services
{
    public class FileImporterServiceTests
    {
        private FileImporterService _sut;
        private Mock<ICurrentHttpContext> _httpContextMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<ILogger<FileImporterService>> _loggerMock;
        private string _resourceName;
        private string _siteId;
        private string _userData;
        private string _badResourceName;
        private IReadOnlyDictionary<Dtmi, DTEntityInfo> _modelsData;
        private Mock<ITwinsClient> _twinsClient;
        private Mock<ITwinsService> _twinsService;
        private Mock<IModelsService> _modelsService;
        private Mock<IDocumentsClient> _documentClient;
        private Mock<IImportClient> _importClient;
        private Mock<ITimeSeriesClient> _timeSeriesClient;

        public FileImporterServiceTests()
        {
            _httpContextMock = new Mock<ICurrentHttpContext>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _resourceName = "csvTestData.csv";
            _siteId = "testSiteId";
            _userData = "testUseData";
            _badResourceName = "csvTestDataEmptyUniqueId.csv";
            _modelsData = TestDataFactory.GetModelsData();
            _loggerMock = new Mock<ILogger<FileImporterService>>();
            _twinsClient = new Mock<ITwinsClient>();
            _modelsService = new Mock<IModelsService>();
            _documentClient = new Mock<IDocumentsClient>();
            _importClient = new Mock<IImportClient>();
            _twinsService = new Mock<ITwinsService>();
            _timeSeriesClient = new Mock<ITimeSeriesClient>();
            _sut = new FileImporterService(_httpContextMock.Object, _httpClientFactoryMock.Object, _loggerMock.Object, _twinsClient.Object, _twinsService.Object, _modelsService.Object, _documentClient.Object, _importClient.Object, _timeSeriesClient.Object);

            _httpContextMock.Setup(m => m.UserEmail).Returns("testuser@willowinc.com");
        }

        [Fact]
        public async void ImportAsync_ShouldReturnAValidImportJobStatus()
        {
            _modelsService.Setup(m => m.GetParsedModelsAsync()).ReturnsAsync(_modelsData);
            _twinsService.Setup(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()))
                .ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            IList<IFormFile> _formFiles = new List<IFormFile>()
            {
                TestDataFactory.GetFormFile(_resourceName),
                TestDataFactory.GetFormFile(_resourceName)
            };
            var result = await _sut.ImportAsync(_formFiles, _siteId, false, _userData, true);

            _modelsService.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
            _twinsService.Verify(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.True(result.Target.First() == EntityType.Twins);
        }

        [Fact]
        public async void ImportAsync_GetModelsShouldThrowAnException()
        {
            _modelsService.Setup(m => m.GetParsedModelsAsync()).ThrowsAsync(new Exception("Unit test exception"));
            _twinsService.Setup(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()))
                .ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

            IList<IFormFile> _formFiles = new List<IFormFile>()
            {
                TestDataFactory.GetFormFile(_resourceName),
                TestDataFactory.GetFormFile(_resourceName)
            };
            var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ImportAsync(_formFiles, _siteId, false, _userData, false));

            _modelsService.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
            _twinsService.Verify(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()), Times.Never);
            Assert.True(result.Message == "Unit test exception");
        }

        [Fact]
        public async void ImportAsync_PostTwinsAsyncShouldThrowAnException()
        {
            _modelsService.Setup(m => m.GetParsedModelsAsync()).ReturnsAsync(_modelsData);
            _twinsService.Setup(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unit test exception"));

            IList<IFormFile> _formFiles = new List<IFormFile>()
            {
                TestDataFactory.GetFormFile(_resourceName),
                TestDataFactory.GetFormFile(_resourceName)
            };
            var result = await Assert.ThrowsAsync<Exception>(async () => await _sut.ImportAsync(_formFiles, _siteId, true, _userData, true));

            _modelsService.Verify(m => m.GetParsedModelsAsync(), Times.Exactly(1));
            _twinsService.Verify(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()), Times.Exactly(1));
            Assert.True(result.Message == "Unit test exception");
        }

        [Fact]
        public async void ImportAsync_PostRelationshipsAsyncShouldThrowAnException111()
        {
            _modelsService.Setup(m => m.GetParsedModelsAsync()).ReturnsAsync(_modelsData);
            _twinsService.Setup(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()))
                .ReturnsAsync(TestDataFactory.GetAdtBulkImportJob(EntityType.Twins));

            IList<IFormFile> _formFiles = new List<IFormFile>()
            {
                TestDataFactory.GetFormFile(_resourceName),
                TestDataFactory.GetFormFile(_resourceName)
            };
            var result = await Assert.ThrowsAsync<NullReferenceException>(async () => await _sut.ImportAsync(It.Is<IEnumerable<IFormFile>>(x => x == null), _siteId, true, _userData, true));

            _modelsService.Verify(m => m.GetParsedModelsAsync(), Times.Once);
            _twinsService.Verify(m => m.PostTwinsAndRelationshipsAsync(It.IsAny<BulkImportTwinsRequest>(), It.IsAny<string>()), Times.Never);
            Assert.True(result.Message == "Object reference not set to an instance of an object.");
        }

        [Fact]
        public async void GetTwins_ShouldReturnAValidListOfTwins()
        {
            _twinsService.Setup(m => m.GetAllTwinsAsync(null, new string[] { "dtmi:com:willowinc:Document;1" }, false, false, false, SourceType.Adx)).ReturnsAsync(TestDataFactory.GetTestTwinsWithRelationships());

            var result = await _sut.GetDocumentsAsync();

            _twinsService.Verify(m => m.GetAllTwinsAsync(null, new string[] { "dtmi:com:willowinc:Document;1" }, false, false, false, SourceType.Adx), Times.Exactly(1));
            Assert.Equal(3, result.Count());
        }

        // [Fact(Skip = "Waiting for twinId creation logic")]
        // public async void CreateFileTwinsAsync_ShouldSetTwinId()
        // {
        // 	var response = new Azure.DigitalTwins.Core.BasicDigitalTwin();
        // 	// response.Content = new StringContent(JsonSerializer.Serialize(TestDataFactory.GetTestDocTwin()));
        // 	_documentClient.Setup(m => m.CreateDocumentAsync(
        // 		It.IsAny<string>(),
        // 		null,
        // 		null,
        // 		"dtmi:com:willowinc:Document;1",
        // 		null,
        // 		null,
        // 		true,
        // 		It.IsAny<FileParameter>(),
        // 		It.IsAny<string>(),
        // 		It.IsAny<string>(),
        // 		It.IsAny<string>() )).ReturnsAsync(response);
        // 	List<IFormFile> files = new List<IFormFile>();
        // 	files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));
        // 	files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));
        // 	files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));

        // 	var result = await _sut.CreateFileTwinsAsync(new Models.CreateDocumentRequest() { Files = files });

        // 	_proxyMock.Verify(m => m.PostDocumentTwinAsync(It.Is<CreateDocumentRequest>(x => !string.IsNullOrWhiteSpace(x.Twin.Id))), Times.Exactly(3));
        // 	Assert.True(result.Where(x => x.IsSuccessful).Count() == 3);
        // }

        // [Fact]
        // public async void CreateFileTwinsAsync_ShouldThrowAnExceptionOnUserUnknown()
        // {
        // 	_httpContextMock.Setup(m => m.UserEmail).Returns("");
        // 	_proxyMock.Setup(m => m.PostDocumentTwinAsync(It.IsAny<CreateDocumentRequest>())).ThrowsAsync(new Exception("Unit test exception"));
        // 	List<IFormFile> files = new List<IFormFile>();
        // 	files.Add(TestDataFactory.GetFormFile("imhiding.jpg"));

        // 	var result = await Assert.ThrowsAsync<BadHttpRequestException>(async () => await _sut.CreateFileTwinsAsync(new Models.CreateDocumentRequest() { Files = files }));

        // 	_proxyMock.Verify(m => m.PostDocumentTwinAsync(It.IsAny<CreateDocumentRequest>()), Times.Never());
        // 	Assert.True(result.Message == "User unknown");
        // }
    }
}
