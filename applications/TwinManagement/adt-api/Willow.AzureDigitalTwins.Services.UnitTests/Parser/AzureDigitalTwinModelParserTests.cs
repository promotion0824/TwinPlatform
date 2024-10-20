namespace Willow.AzureDigitalTwins.Services.UnitTests.Parser
{
    using Azure.DigitalTwins.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using Willow.AzureDigitalTwins.Services.Cache.Models;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.AzureDigitalTwins.Services.Parser;
    using Willow.Model.Adt;
    using Xunit;

    public class AzureDigitalTwinModelParserTests
    {
        private readonly AzureDigitalTwinModelParser azureDigitalTwinModelParser;
        private readonly List<DigitalTwinsModelBasicData> models = new List<DigitalTwinsModelBasicData>();
        private const string ModelIdWithDescendants = "dtmi:digitaltwins:rec_3_3:core:Capability;1";
        private const string DescendantModelId = "dtmi:digitaltwins:rec_3_3:core:Parameter;1";
        private const string ModelIdWithoutDescendants = "dtmi:digitaltwins:rec_3_3:business:Role;1";
        private readonly Mock<ILogger<AzureDigitalTwinModelParser>> loggerMock;

        public AzureDigitalTwinModelParserTests()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(directory, "Data", "Models.json");

            models.AddRange(JsonSerializer.Deserialize<List<DigitalTwinsModelBasicData>>(File.ReadAllText(path)));

            loggerMock = new Mock<ILogger<AzureDigitalTwinModelParser>>();
            var cacheProviderMock = new Mock<IAzureDigitalTwinCacheProvider>();
            var cache = new AzureDigitalTwinCache(
                new ModelCache(new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(models.ToDictionary(x => x.Id, x => x)), loggerMock.Object),
                new TwinCache(
                new ConcurrentDictionary<string, BasicDigitalTwin>(),
                new ConcurrentDictionary<string, BasicRelationship>(),
                new ConcurrentDictionary<string, List<string>>(),
                new ConcurrentDictionary<string, HashSet<string>>(),
                new ConcurrentDictionary<string, List<string>>()));

            cacheProviderMock.Setup(x => x.GetOrCreateCache(true)).Returns(cache);

            azureDigitalTwinModelParser = new AzureDigitalTwinModelParser(cacheProviderMock.Object, loggerMock.Object);
        }

        [Theory]
        [InlineData(ModelIdWithDescendants, 587)]
        [InlineData(ModelIdWithoutDescendants, 1)]
        public void GetInterfaceDescendants_ShouldReturnDescendantsIncludingItself(string modelId, int expectedCount)
        {
            var descendants = azureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { modelId });

            Assert.NotNull(descendants);
            Assert.NotEmpty(descendants);
            Assert.True(descendants.ContainsKey(modelId));
            Assert.Equal(expectedCount, descendants.Count);
        }

        [Theory]
        [InlineData(ModelIdWithDescendants, DescendantModelId, true)]
        [InlineData(ModelIdWithDescendants, ModelIdWithDescendants, true)]
        [InlineData(ModelIdWithoutDescendants, ModelIdWithoutDescendants, true)]
        [InlineData(ModelIdWithoutDescendants, ModelIdWithDescendants, false)]
        public void IsDescendantOf_ShouldIndicateIfDescendant(string rootModelId, string modelId, bool expectedResult)
        {
            var isDescendant = azureDigitalTwinModelParser.IsDescendantOf(rootModelId, modelId);

            Assert.Equal(expectedResult, isDescendant);
        }

        [Fact]
        public void IsDescendantOfAny_ShouldIndicateIfDescendant()
        {
            var parents = new List<string> { ModelIdWithDescendants, ModelIdWithoutDescendants };

            var isDescendant = azureDigitalTwinModelParser.IsDescendantOfAny(parents, DescendantModelId);

            Assert.True(isDescendant);
        }
    }
}
