using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Controllers;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Controllers
{
    public class RelationshipsControllerTests
    {
        private readonly RelationshipsController _relationshipsController;
        private readonly Mock<IAzureDigitalTwinReader> _azureDigitalTwinsReaderMock;
        private readonly Mock<IAzureDigitalTwinWriter> _azureDigitalTwinsWriterMock;

        public RelationshipsControllerTests()
        {
            _azureDigitalTwinsReaderMock = new Mock<IAzureDigitalTwinReader>();
            _azureDigitalTwinsWriterMock = new Mock<IAzureDigitalTwinWriter>();
            _relationshipsController = new RelationshipsController(_azureDigitalTwinsReaderMock.Object, _azureDigitalTwinsWriterMock.Object);
        }

        [Fact]
        public async Task UpdateRelationship_WithValidContent_ShouldReturnCreated()
        {
            var id = "TheId";
            var relationship = new BasicRelationship
            {
                Id = id,
                TargetId = "target",
                Name = "name",
                SourceId = "source"
            };

            _azureDigitalTwinsReaderMock.Setup(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new BasicRelationship());
            _azureDigitalTwinsWriterMock.Setup(x => x.CreateOrReplaceRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>())).ReturnsAsync(new BasicRelationship { Id = id });

            var response = await _relationshipsController.UpsertRelationship(relationship);

            Assert.NotNull(response);
            Assert.Equal(id, response.Value.Id);
        }

        [Fact]
        public async Task GetRelationshipsByTwin_WithContext_ShouldReturnOk()
        {
            var id = "TheId";

            _azureDigitalTwinsReaderMock.Setup(x => x.GetTwinRelationshipsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<BasicRelationship> { new BasicRelationship() });
            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(new BasicDigitalTwin());

            var response = await _relationshipsController.GetRelationships(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Value);
        }

        [Fact]
        public async Task GetIncomingRelationships_WithContext_ShouldReturnOk()
        {
            var id = "TheId";

            _azureDigitalTwinsReaderMock.Setup(x => x.GetIncomingRelationshipsAsync(It.IsAny<string>())).ReturnsAsync(new List<BasicRelationship> { new BasicRelationship() });
            _azureDigitalTwinsReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(new BasicDigitalTwin());

            var response = await _relationshipsController.GetIncomingRelationships(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Value);
        }

        [Fact]
        public async Task GetRelationship_WithContext_ShouldReturnOk()
        {
            _azureDigitalTwinsReaderMock.Setup(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new BasicRelationship());

            var response = await _relationshipsController.GetRelationship("Id", "TwinId");

            Assert.NotNull(response);
            Assert.NotNull(response.Value);
        }

        [Fact]
        public async Task DeleteRelationship_WithInvalidTwin_ShouldReturnNotFound()
        {
            BasicRelationship relationship = null;
            _azureDigitalTwinsReaderMock.Setup(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(relationship);

            var response = await _relationshipsController.DeleteRelationship("TwinId", "Id");

            var result = response as NotFoundResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteRelationship_WithValidId_ShouldReturnNoContent()
        {
            _azureDigitalTwinsReaderMock.Setup(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new BasicRelationship());
            _azureDigitalTwinsWriterMock.Setup(x => x.DeleteRelationshipAsync(It.IsAny<string>(), It.IsAny<string>()));

            var response = await _relationshipsController.DeleteRelationship("TwinId", "Id");

            var result = response as NoContentResult;

            Assert.NotNull(result);
            Assert.Equal(result.StatusCode, (int)HttpStatusCode.NoContent);
        }
    }
}
