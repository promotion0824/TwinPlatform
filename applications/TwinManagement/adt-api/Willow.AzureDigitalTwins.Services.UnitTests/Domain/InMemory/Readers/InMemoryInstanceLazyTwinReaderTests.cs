namespace Willow.AzureDigitalTwins.Services.UnitTests.Domain.InMemory.Readers
{
    using AutoFixture;
    using Azure.DigitalTwins.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Willow.AzureDigitalTwins.Services.Cache.Models;
    using Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.Model.Adt;
    using Xunit;

    public class InMemoryInstanceLazyTwinReaderTests
    {
        private readonly InMemoryInstanceLazyTwinReader inMemoryInstanceLazyTwinReader;
        private readonly Mock<IAzureDigitalTwinModelParser> azureDigitalTwinModelParserMock;
        private readonly Mock<IAzureDigitalTwinCacheProvider> azureDigitalTwinCacheProviderMock;
        private readonly Mock<IAzureDigitalTwinReader> azureDigitalTwinReaderMock;
        private readonly Fixture fixture;

        public InMemoryInstanceLazyTwinReaderTests()
        {
            azureDigitalTwinModelParserMock = new Mock<IAzureDigitalTwinModelParser>();
            azureDigitalTwinCacheProviderMock = new Mock<IAzureDigitalTwinCacheProvider>();
            azureDigitalTwinReaderMock = new Mock<IAzureDigitalTwinReader>();
            fixture = new Fixture();
            Twins = new List<BasicDigitalTwin>();
            Relationships = new List<BasicRelationship>();
            var twinsByModel = new ConcurrentDictionary<string, HashSet<string>>();
            var twinRelationships = new ConcurrentDictionary<string, List<string>>();
            var twinIncomingRelationships = new ConcurrentDictionary<string, List<string>>();
            Mock<ILogger> loggerMock = new Mock<ILogger>();

            azureDigitalTwinCacheProviderMock.Setup(x => x.IsCacheReady(It.IsAny<bool>())).Returns(true);

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(directory, "Data", "Models.json");

            Models = new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(JsonSerializer.Deserialize<List<DigitalTwinsModelBasicData>>(File.ReadAllText(path)).ToDictionary(x => x.Id, x => x));

            for (int i = 0; i < 20; i++)
            {
                Twins.Add(fixture.Build<BasicDigitalTwin>()
                    .With(x => x.Metadata, new DigitalTwinMetadata { ModelId = Models.First().Key })
                    .Create());
            }

            twinsByModel.TryAdd(Models.First().Key, Twins.Select(x => x.Id).ToHashSet<string>());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[0].Id)
                .With(x => x.SourceId, Twins[9].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[2].Id)
                .With(x => x.SourceId, Twins[0].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[4].Id)
                .With(x => x.SourceId, Twins[8].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[12].Id)
                .With(x => x.SourceId, Twins[13].Id)
                .Create());

            Relationships.ForEach(x =>
            {
                if (!twinIncomingRelationships.ContainsKey(x.TargetId))
                {
                    twinIncomingRelationships.TryAdd(x.TargetId, new List<string>());
                }

                if (!twinRelationships.ContainsKey(x.SourceId))
                {
                    twinRelationships.TryAdd(x.SourceId, new List<string>());
                }

                twinRelationships[x.SourceId].Add(x.Id);
                twinIncomingRelationships[x.TargetId].Add(x.Id);
            });

            azureDigitalTwinCacheProviderMock.Setup(x => x.GetOrCreateCache(true)).Returns(
                new AzureDigitalTwinCache(
                    new ModelCache(Models, loggerMock.Object),
                    new TwinCache(
                new ConcurrentDictionary<string, BasicDigitalTwin>(Twins.ToDictionary(x => x.Id, x => (BasicDigitalTwin)null)),
                new ConcurrentDictionary<string, BasicRelationship>(Relationships.ToDictionary(x => x.Id, x => new BasicRelationship { SourceId = x.SourceId, TargetId = x.TargetId })),
                twinRelationships,
                twinsByModel,
                twinIncomingRelationships)));

            inMemoryInstanceLazyTwinReader = new InMemoryInstanceLazyTwinReader(azureDigitalTwinModelParserMock.Object, azureDigitalTwinCacheProviderMock.Object, azureDigitalTwinReaderMock.Object);
        }

        public List<BasicDigitalTwin> Twins { get; private set; }

        public List<BasicRelationship> Relationships { get; private set; }

        public ConcurrentDictionary<string, DigitalTwinsModelBasicData> Models { get; private set; }

        [Fact]
        public async Task GetRelationshipsAsync_NonExistingTwin_ShouldReturnNoRelationships()
        {
            var relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Guid.NewGuid().ToString());

            Assert.Empty(relationships);
        }

        [Fact]
        public async Task GetRelationshipsAsync_TwinWithNoRelationships_ShouldReturnNoRelationships()
        {
            var relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Twins[1].Id);

            Assert.Empty(relationships);
        }

        [Fact]
        public async Task GetRelationshipsAsync_TwinWithRelationshipsNotInCache_ShouldReturnRelationships_AndLoadRelationships()
        {
            var relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_TwinWithRelationshipsNotInCache_ShouldReturnRelationships_AndLoadRelationships_SecondTimeNotCallInstance()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<BasicRelationship> { Relationships[1] });

            var relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetIncomingRelationshipsAsync_TwinWithNoRelationships_ShouldReturnNoRelationships()
        {
            var relationships = await inMemoryInstanceLazyTwinReader.GetIncomingRelationshipsAsync(Twins[1].Id);

            Assert.Empty(relationships);
        }

        [Fact]
        public async Task GetIncomingRelationshipsAsync_TwinWithRelationshipsNotInCache_ShouldReturnRelationships_AndLoadRelationships()
        {
            var relationships = await inMemoryInstanceLazyTwinReader.GetIncomingRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetIncomingRelationshipsAsync_TwinWithRelationshipsNotInCache_ShouldReturnRelationships_AndLoadRelationships_SecondTimeNotCallInstance()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<BasicRelationship> { Relationships[1] });

            var relationships = await inMemoryInstanceLazyTwinReader.GetIncomingRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            relationships = await inMemoryInstanceLazyTwinReader.GetTwinRelationshipsAsync(Twins[0].Id);

            Assert.Single(relationships);

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_WithNull_ShouldReturnRelationships_AndLoadRelationships_SecondTimeNotCallInstance()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(Relationships);

            var relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(Enumerable.Empty<string>());

            Assert.Equal(Relationships.Count, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(Enumerable.Empty<string>());

            Assert.Equal(Relationships.Count, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_WithEmpty_ShouldReturnRelationships_AndLoadRelationships_SecondTimeNotCallInstance()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(Relationships);

            var relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(Enumerable.Empty<string>());

            Assert.Equal(Relationships.Count, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(Enumerable.Empty<string>());

            Assert.Equal(Relationships.Count, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipsAsync_WithRelationshipIds_ShouldReturnRelationships_AndLoadRelationships_SecondTimeNotCallInstance()
        {
            var relationshipsToRetrieve = new List<BasicRelationship> { Relationships[0], Relationships[1] };

            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(relationshipsToRetrieve);

            var relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(relationshipsToRetrieve.Select(x => x.Id));

            Assert.Equal(2, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            relationships = await inMemoryInstanceLazyTwinReader.GetRelationshipsAsync(relationshipsToRetrieve.Select(x => x.Id));

            Assert.Equal(2, relationships.Count());

            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetDigitalTwinAsync_NonExistingTwin_ReturnNull()
        {
            var twin = await inMemoryInstanceLazyTwinReader.GetDigitalTwinAsync(Guid.NewGuid().ToString());

            Assert.Null(twin);
        }

        [Fact]
        public async Task GetDigitalTwinAsync_WithValidTwin_ReturnTwin_SecondTimeReturnFromCache()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetDigitalTwinAsync(It.IsAny<string>())).ReturnsAsync(Twins[0]);

            var id = Twins[0].Id;
            var twin = await inMemoryInstanceLazyTwinReader.GetDigitalTwinAsync(id);

            Assert.NotNull(twin);
            Assert.Equal(id, twin.Id);

            twin = await inMemoryInstanceLazyTwinReader.GetDigitalTwinAsync(id);

            Assert.NotNull(twin);
            Assert.Equal(id, twin.Id);
            azureDigitalTwinReaderMock.Verify(x => x.GetDigitalTwinAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetRelationshipAsync_NonExistingRelationship_ReturnNull()
        {
            var relationship = await inMemoryInstanceLazyTwinReader.GetRelationshipAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.Null(relationship);
        }

        [Fact]
        public async Task GetRelationshipAsync_WithValidTwin_ReturnRelationship_SecondTimeReturnFromCache()
        {
            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(Relationships[0]);

            var id = Relationships[0].Id;
            var id2 = Relationships[0].SourceId;
            var entity = await inMemoryInstanceLazyTwinReader.GetRelationshipAsync(id, id2);

            Assert.NotNull(entity);
            Assert.Equal(id, entity.Id);

            entity = await inMemoryInstanceLazyTwinReader.GetRelationshipAsync(id, id2);

            Assert.NotNull(entity);
            Assert.Equal(id, entity.Id);
            azureDigitalTwinReaderMock.Verify(x => x.GetRelationshipAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
