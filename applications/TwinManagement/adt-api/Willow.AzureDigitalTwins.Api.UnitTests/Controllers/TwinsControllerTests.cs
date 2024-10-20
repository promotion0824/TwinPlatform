using AutoFixture;
using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Controllers;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Controllers
{
    public class TwinsControllerTests
    {
        private readonly TwinsController _twinsController;
        private readonly Mock<IAzureDigitalTwinReader> _azureDigitalTwinsReaderMock;
        private readonly Mock<IAzureDigitalTwinWriter> _azureDigitalTwinsWriterMock;
        private readonly Mock<ITwinsService> _twinsServiceMock;
        private readonly Mock<IDQRuleService> _ruleServiceMock;
        private readonly Mock<IExportService> _exportServiceMock;
        private readonly Mock<IBulkImportService> _importServiceMock;
        private readonly Mock<ILogger<TwinsController>> _loggerMock;
        private readonly Fixture _fixture;

        public TwinsControllerTests()
        {
            _azureDigitalTwinsReaderMock = new Mock<IAzureDigitalTwinReader>();
            _azureDigitalTwinsWriterMock = new Mock<IAzureDigitalTwinWriter>();
            _twinsServiceMock = new Mock<ITwinsService>();
            _ruleServiceMock = new Mock<IDQRuleService>();
            _exportServiceMock = new Mock<IExportService>();
            _importServiceMock = new Mock<IBulkImportService>();
            _loggerMock = new Mock<ILogger<TwinsController>>();
            _twinsController = new TwinsController(_azureDigitalTwinsReaderMock.Object,
                _azureDigitalTwinsWriterMock.Object,
                _twinsServiceMock.Object,
                _ruleServiceMock.Object,
                _exportServiceMock.Object,
                _importServiceMock.Object,
                _loggerMock.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task GetTwinById_WithValidId_ShouldReturnValidTwin()
        {
            var id = "theId";
            var twin = new BasicDigitalTwin { Id = id };
            var twinWithRelationship = new Page<TwinWithRelationships>() { Content = new[] { new TwinWithRelationships { Twin = twin } }, ContinuationToken = null };

            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(twin);
            _twinsServiceMock.Setup(x => x.GetTwinsByIds(new string[] { id }, SourceType.AdtQuery, false)).ReturnsAsync(twinWithRelationship);
            var response = await _twinsController.GetTwinById(id, SourceType.AdtQuery, false);

            var result = response.Result as OkObjectResult;
            var twinResult = result.Value as TwinWithRelationships;

            Assert.NotNull(result);
            Assert.NotNull(twinResult);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
            Assert.Equal(id, twinResult.Twin.Id);
        }

        [Fact]
        public async Task GetTwinById_WithNonExistingTwin_ShouldReturnNotFound()
        {
            BasicDigitalTwin twin = null;
            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(twin);

            var response = await _twinsController.GetTwinById("id");

            var result = response.Result as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTwin_WithContext_ShouldReturnOk()
        {
            var id = "TheId";

            var twin = new BasicDigitalTwin { Id = id };
            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(twin);
            _azureDigitalTwinsWriterMock.Setup(x => x.CreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>(), It.IsAny<CancellationToken>())).ReturnsAsync(twin);

            var response = await _twinsController.UpdateTwin(new BasicDigitalTwin { Id = id });

            var result = response.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task PatchTwin_WithNonExistingTwin_ShouldReturnNotFound()
        {
            BasicDigitalTwin twin = null;
            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(twin);

            var response = await _twinsController.PatchTwin("id", new JsonPatchDocument<Twin>());

            var result = response as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);
        }


        [Fact]
        public async Task PatchTwin_WithValidTwin_ShouldReturnOk()
        {
            var twin = new BasicDigitalTwin { Id = "id" };
            var patchDocument = new JsonPatchDocument<Twin>();

            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(twin);

            var response = await _twinsController.PatchTwin("id", patchDocument);

            var result = response as OkResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetTwinsWithRelationships_ShouldReturnTwins()
        {
            var twins = _fixture.CreateMany<TwinWithRelationships>(10).ToList();
            var request = new GetTwinsInfoRequest();

            _twinsServiceMock.Setup(x => x.GetTwins(request,
                                /*pageSize:*/ It.IsAny<int>(),
                                /*continuationToken:*/ It.IsAny<string>(),
                                 /*includeTotalCount*/ It.IsAny<bool>()))
                    .ReturnsAsync(twins.ToPageModel(1, 500));

            var response = await _twinsController.GetTwins(request);

            var result = response.Result as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.OK);
        }
    }
}
