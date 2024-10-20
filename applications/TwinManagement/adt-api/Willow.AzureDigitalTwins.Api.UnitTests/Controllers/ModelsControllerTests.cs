using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Controllers;
using Willow.AzureDigitalTwins.Api.Processors;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Storage.Repositories;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Controllers
{
    public class ModelsControllerTests
    {
        private readonly ModelsController _controller;
        private readonly Mock<IAzureDigitalTwinReader> _azureDigitalTwinsReaderMock;
        private readonly Mock<IAzureDigitalTwinWriter> _azureDigitalTwinsWriterMock;
        private readonly Mock<IExportService> _exportService;
        private readonly Mock<IRepositoryService> _repositoryServiceMock;
        private readonly Mock<IBulkImportService> _bulkImportServiceMock;
        private readonly Mock<IAdxService> _adxServiceMock;
        private readonly Mock<IAzureDigitalTwinModelParser> _azureDigitalTwinModelParserMock;
        private readonly Mock<ITwinsService> _twinService;
        private readonly Mock<ITelemetryCollector> _telemetryCollector;
        private readonly Mock<ILogger<ModelsController>> _logger;
        private readonly Fixture _fixture;
        private DigitalTwinsModelBasicData _model;

        public ModelsControllerTests()
        {
            _azureDigitalTwinsReaderMock = new Mock<IAzureDigitalTwinReader>();
            _azureDigitalTwinsWriterMock = new Mock<IAzureDigitalTwinWriter>();
            _repositoryServiceMock = new Mock<IRepositoryService>();
            _bulkImportServiceMock = new Mock<IBulkImportService>();
            _adxServiceMock = new Mock<IAdxService>();
            _azureDigitalTwinModelParserMock = new Mock<IAzureDigitalTwinModelParser>();
            _exportService = new Mock<IExportService>();
            _telemetryCollector = new Mock<ITelemetryCollector>();
            _twinService = new Mock<ITwinsService>();
            _logger = new Mock<ILogger<ModelsController>>();
            _controller = new ModelsController(_azureDigitalTwinsReaderMock.Object,
                _azureDigitalTwinsWriterMock.Object,
                _exportService.Object,
                _repositoryServiceMock.Object,
                _bulkImportServiceMock.Object,
                _adxServiceMock.Object,
                _azureDigitalTwinModelParserMock.Object,
                _twinService.Object,
                _telemetryCollector.Object,
                _logger.Object);
            _fixture = new Fixture();
            _model = new DigitalTwinsModelBasicData
            {
                Id = "dtmi:digitaltwins:rec_3_3:core:Capability;1",
                DtdlModel = "{\u0022@id\u0022:\u0022dtmi:digitaltwins:rec_3_3:core:Capability;1\u0022,\u0022@type\u0022:\u0022Interface\u0022,\u0022contents\u0022:[{\u0022@type\u0022:\u0022Relationship\u0022,\u0022description\u0022:{\u0022en\u0022:\u0022The coverage or impact area of a given Asset or Sensor/Actuator. For example: an air-treatment unit might serve several Rooms or a full Building. Note that Assets can also service one another, e.g., an air-treatment Asset might serve an air diffuser Asset. Inverse of: servedBy\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022serves\u0022},\u0022name\u0022:\u0022serves\u0022},{\u0022@type\u0022:\u0022Relationship\u0022,\u0022description\u0022:{\u0022en\u0022:\u0022The entity (Asset, Space, LogicalDevice, etc.) that has this Capability. Inverse of: hasCapability\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022is capabilty of\u0022},\u0022name\u0022:\u0022isCapabilityOf\u0022},{\u0022@type\u0022:\u0022Property\u0022,\u0022displayName\u0022:{\u0022en\u0022:\u0022name\u0022},\u0022name\u0022:\u0022name\u0022,\u0022schema\u0022:\u0022string\u0022,\u0022writable\u0022:true}],\u0022description\u0022:{\u0022en\u0022:\u0022A Capability indicates the capacity of an entity, be it a Space, an Asset, or a Device, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \\\u0022point\\\u0022. Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022Capability\u0022},\u0022@context\u0022:[\u0022dtmi:dtdl:context;2\u0022]}"
            };
        }

        [Fact]
        public async Task GetModel_WithNonExistingModel_ShouldReturnNotFound()
        {
            DigitalTwinsModelBasicData model = null;
            _azureDigitalTwinsReaderMock.Setup(x => x.GetModelAsync(It.IsAny<string>())).ReturnsAsync(model);

            var response = await _controller.GetModel("id");

            var result = response.Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteModel_WithNonExistingTwin_ShouldReturnNotFound()
        {
            DigitalTwinsModelBasicData model = null;
            _azureDigitalTwinsReaderMock.Setup(x => x.GetModelAsync(It.IsAny<string>())).ReturnsAsync(model);

            var response = await _controller.DeleteModel("id");

            var result = response as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteModel_WithExistingTwin_ShouldReturnNoContent()
        {
            _azureDigitalTwinsReaderMock.Setup(x => x.GetModelAsync(It.IsAny<string>())).ReturnsAsync(_fixture.Create<DigitalTwinsModelBasicData>());
            _azureDigitalTwinsWriterMock.Setup(x => x.DeleteModelAsync(It.IsAny<string>()));

            var response = await _controller.DeleteModel("id");

            var result = response as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task GetModels_ShouldReturnModels()
        {
            _azureDigitalTwinsReaderMock.Setup(x => x.GetModelsAsync(It.IsAny<string>())).ReturnsAsync(new List<DigitalTwinsModelBasicData> { _model });

            var response = await _controller.GetModels();

            Assert.Single(response.Value);
        }

        [Fact]
        public async Task GetModel_ShouldReturnModel()
        {
            _azureDigitalTwinsReaderMock.Setup(x => x.GetModelAsync(It.IsAny<string>())).ReturnsAsync(_model);

            var response = await _controller.GetModels();

            var result = response.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateModels_WithEmptyModels_ShouldReturnBadRequest()
        {
            var response = await _controller.CreateModels(Enumerable.Empty<JsonDocument>());

            var result = response as BadRequestResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateModels_WithInvalidModelDtdl_ShouldReturnBadRequest()
        {
            var invalidDtdlModel = "{\u0022@i23d\u0022:\u0022dtmi:digitaltwins:rec_3_3:core:Capability;1\u0022,\u0022@type\u0022:\u0022Interface\u0022,\u0022contents\u0022:[{\u0022@type\u0022:\u0022Relationship\u0022,\u0022description\u0022:{\u0022en\u0022:\u0022The coverage or impact area of a given Asset or Sensor/Actuator. For example: an air-treatment unit might serve several Rooms or a full Building. Note that Assets can also service one another, e.g., an air-treatment Asset might serve an air diffuser Asset. Inverse of: servedBy\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022serves\u0022},\u0022name\u0022:\u0022serves\u0022},{\u0022@type\u0022:\u0022Relationship\u0022,\u0022description\u0022:{\u0022en\u0022:\u0022The entity (Asset, Space, LogicalDevice, etc.) that has this Capability. Inverse of: hasCapability\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022is capabilty of\u0022},\u0022name\u0022:\u0022isCapabilityOf\u0022},{\u0022@type\u0022:\u0022Property\u0022,\u0022displayName\u0022:{\u0022en\u0022:\u0022name\u0022},\u0022name\u0022:\u0022name\u0022,\u0022schema\u0022:\u0022string\u0022,\u0022writable\u0022:true}],\u0022description\u0022:{\u0022en\u0022:\u0022A Capability indicates the capacity of an entity, be it a Space, an Asset, or a Device, to produce or ingest data. This is roughly equivalent to the established Brick Schema and generic BMS term \\\u0022point\\\u0022. Specific subclasses specialize this behaviour: Sensor entities harvest data from the real world, Actuator entities accept commands from a digital twin platform, and Parameter entities configure some capability or system.\u0022},\u0022displayName\u0022:{\u0022en\u0022:\u0022Capability\u0022},\u0022@context\u0022:[\u0022dtmi:dtdl:context;2\u0022]}";

            var response = await _controller.CreateModels(new List<JsonDocument> { JsonDocument.Parse(invalidDtdlModel) });

            var result = response as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateModels_WithValidModel_ShouldReturnCreated()
        {
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(x => x.Request).Returns(new Mock<HttpRequest>().Object);
            _controller.ControllerContext.HttpContext = httpContextMock.Object;

            _azureDigitalTwinsWriterMock.Setup(x => x.CreateModelsAsync(It.IsAny<IEnumerable<DigitalTwinsModelBasicData>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DigitalTwinsModelBasicData> { _model });

            var response = await _controller.CreateModels(new List<JsonDocument> { JsonDocument.Parse(_model.DtdlModel) });

            var result = response as OkResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpgradeFromRepos_WithEmptyRepos_ShouldReturnBadRequest()
        {
            var response = await _controller.UpgradeFromRepos(new List<UpgradeModelsRepoRequest>(), Guid.NewGuid().ToString());

            var result = response.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpgradeFromZipFiles_WithEmptyFiles_ShouldReturnBadRequest()
        {
            var response = await _controller.UpgradeFromZipFiles(new List<IFormFile>(), Guid.NewGuid().ToString());

            var result = response.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpgradeFromRepos_WithInvalidRepo_ShouldReturnBadRequest()
        {
            _repositoryServiceMock.Setup(x => x.GetRepositoryContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new HttpRequestException("Error", new Exception(), HttpStatusCode.NotFound));

            var response = await _controller.UpgradeFromRepos(new List<UpgradeModelsRepoRequest> { _fixture.Create<UpgradeModelsRepoRequest>() }, Guid.NewGuid().ToString());

            var result = response.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpgradeFromRepos_WithEmptyModels_ShouldReturnBadRequest()
        {
            _repositoryServiceMock.Setup(x => x.GetRepositoryContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Dictionary<string, string>());

            var response = await _controller.UpgradeFromRepos(new List<UpgradeModelsRepoRequest> { _fixture.Create<UpgradeModelsRepoRequest>() }, Guid.NewGuid().ToString());

            var result = response.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpgradeFromZipFiles_WithEmptyModels_ShouldReturnBadRequest()
        {
            _repositoryServiceMock.Setup(x => x.ReadContent(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<string, string>());
            var stream = new MemoryStream();

            var response = await _controller.UpgradeFromZipFiles(new List<IFormFile> { new FormFile(stream, 0, stream.Length, "test", "testfile") }, Guid.NewGuid().ToString());

            var result = response.Result as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpgradeFromRepos_WithValidData_ShouldReturnJob()
        {
            var job = _fixture.Create<AdtBulkImportJob>();
            _repositoryServiceMock.Setup(x => x.GetRepositoryContent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new Dictionary<string, string> { { "path", "{ \"@id\": \"dtmi:com:willowinc:Structure;1\" }" } });
            _bulkImportServiceMock.Setup(x => x.QueueBulkProcess(It.IsAny<BulkImportModels>(), It.IsAny<EntityType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool?>())).ReturnsAsync(job);

            var response = await _controller.UpgradeFromRepos(new List<UpgradeModelsRepoRequest> { _fixture.Create<UpgradeModelsRepoRequest>() }, Guid.NewGuid().ToString());

            var resultJob = response.Value;

            Assert.NotNull(resultJob);
            Assert.Equal(job.JobId, resultJob.JobId);
            Assert.Equal((int)AsyncJobStatus.Queued, (int)resultJob.Details.Status);
        }

        [Fact]
        public async Task UpgradeFromZipFiless_WithValidData_ShouldReturnJob()
        {
            var job = _fixture.Create<AdtBulkImportJob>();
            _repositoryServiceMock.Setup(x => x.ReadContent(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new Dictionary<string, string> { { "path", "{ \"@id\": \"dtmi:com:willowinc:Structure;1\" }" } });
            _bulkImportServiceMock.Setup(x => x.QueueModelsImport(It.IsAny<IEnumerable<JsonDocument>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(job);

            var stream = new MemoryStream();

            var response = await _controller.UpgradeFromZipFiles(new List<IFormFile> { new FormFile(stream, 0, stream.Length, "test", "testfile") }, Guid.NewGuid().ToString());

            var resultJob = response.Value;

            Assert.NotNull(resultJob);
            Assert.Equal(job.JobId, resultJob.JobId);
            Assert.Equal((int)AsyncJobStatus.Queued, (int)resultJob.Details.Status);
        }
    }
}
