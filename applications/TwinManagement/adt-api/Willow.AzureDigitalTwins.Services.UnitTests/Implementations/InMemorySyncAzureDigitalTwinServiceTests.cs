namespace Willow.AzureDigitalTwins.Services.UnitTests.Implementations
{
    using System.Linq;
    using System.Threading;
    using Azure;
    using Azure.DigitalTwins.Core;
    using Moq;
    using Willow.AzureDigitalTwins.Services.Extensions;
    using Willow.AzureDigitalTwins.Services.UnitTests.Fixtures;
    using Xunit;

    public class InMemorySyncAzureDigitalTwinServiceTests : InMemoryAzureDigitalTwinServiceTests, IClassFixture<AzureDigitalTwinSyncServiceFixture>
    {
        private readonly AzureDigitalTwinSyncServiceFixture azureDigitalTwinServiceFixture;

        public InMemorySyncAzureDigitalTwinServiceTests(AzureDigitalTwinSyncServiceFixture azureDigitalTwinServiceFixture)
            : base(azureDigitalTwinServiceFixture)
        {
            this.azureDigitalTwinServiceFixture = azureDigitalTwinServiceFixture;
        }

        [Fact]
        public override void CreateOrReplaceDigitalTwinAsync_ShouldCreate()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinWriterMock.Setup(x => x.CreateOrReplaceDigitalTwinAsync(It.IsAny<BasicDigitalTwin>(), It.IsAny<CancellationToken>()));
            base.CreateOrReplaceDigitalTwinAsync_ShouldCreate();
        }

        [Fact]
        public override void UpdateDigitalTwinAsync_WithExisting_ShouldUpdate()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinWriterMock.Setup(x => x.UpdateDigitalTwinAsync(It.IsAny<BasicDigitalTwin>(), It.IsAny<JsonPatchDocument>()));
            base.UpdateDigitalTwinAsync_WithExisting_ShouldUpdate();
        }

        [Fact]
        public override void CreateOrReplaceRelationshipAsync_ShouldCreate()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinWriterMock.Setup(x => x.CreateOrReplaceRelationshipAsync(It.IsAny<BasicRelationship>(), It.IsAny<CancellationToken>()));
            base.CreateOrReplaceRelationshipAsync_ShouldCreate();
        }

        [Fact]
        public override void DeleteDigitalTwinAsync_ShouldRemove()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinWriterMock.Setup(x => x.DeleteDigitalTwinAsync(It.IsAny<string>()));
            base.DeleteDigitalTwinAsync_ShouldRemove();
        }

        [Fact]
        public override void DeleteRelationshipAsync_ShouldRemove()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinWriterMock.Setup(x => x.DeleteRelationshipAsync(It.IsAny<string>(), It.IsAny<string>()));
            base.DeleteRelationshipAsync_ShouldRemove();
        }

        [Fact]
        public async void QueryTwinsAsync_WithRandomQuery_ShouldReturnTwins()
        {
            azureDigitalTwinServiceFixture.AzureDigitalTwinReaderMock.Setup(x => x.QueryTwinsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                    .ReturnsAsync(azureDigitalTwinServiceFixture.Twins.ToPageModel(1, 500));

            var response = await azureDigitalTwinServiceFixture.InMemoryAzureDigitalTwinReader.QueryTwinsAsync(query: "DUMMY QUERY", pageSize: 100, continuationToken: null);

            Assert.Equal(azureDigitalTwinServiceFixture.Twins.Count, response.Content.Count());
            Assert.Null(response.ContinuationToken);
        }
    }
}
