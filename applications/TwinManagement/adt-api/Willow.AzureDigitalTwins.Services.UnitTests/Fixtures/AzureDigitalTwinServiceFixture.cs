namespace Willow.AzureDigitalTwins.Services.UnitTests.Fixtures
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
    using Willow.AzureDigitalTwins.Services.Cache.Models;
    using Willow.AzureDigitalTwins.Services.Configuration;
    using Willow.AzureDigitalTwins.Services.Domain.InMemory.Readers;
    using Willow.AzureDigitalTwins.Services.Domain.InMemory.Writers;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.Model.Adt;

    public class AzureDigitalTwinServiceFixture : IDisposable
    {
        private readonly Fixture fixture;
        private readonly Mock<ILogger> loggerMock = new Mock<ILogger>();

        public AzureDigitalTwinServiceFixture()
        {
            fixture = new Fixture();
            Settings = fixture.Build<InMemorySettings>().With(x => x.Source, InMemorySourceType.Instance).Create();
            AzureDigitalTwinModelParserMock = new Mock<IAzureDigitalTwinModelParser>();
            AzureDigitalTwinCacheProviderMock = new Mock<IAzureDigitalTwinCacheProvider>();
            Twins = new List<BasicDigitalTwin>();
            Relationships = new List<BasicRelationship>();
            AzureDigitalTwinCacheProviderMock.Setup(x => x.IsCacheReady(It.IsAny<bool>())).Returns(true);

            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(directory, "Data", "Models.json");

            Models = JsonSerializer.Deserialize<List<DigitalTwinsModelBasicData>>(File.ReadAllText(path)).ToList();

            for (int i = 0; i < 20; i++)
            {
                Twins.Add(fixture.Build<BasicDigitalTwin>()
                    .With(x => x.Metadata, new DigitalTwinMetadata { ModelId = Models[new Random().Next(Models.Count - 1)].Id })
                    .Create());
            }

            TwinWithRelationship = Twins[0];

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, TwinWithRelationship.Id)
                .With(x => x.SourceId, Twins[9].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[2].Id)
                .With(x => x.SourceId, TwinWithRelationship.Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[4].Id)
                .With(x => x.SourceId, Twins[8].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.TargetId, Twins[12].Id)
                .With(x => x.SourceId, Twins[13].Id)
                .Create());

            AzureDigitalTwinCacheProviderMock.Setup(x => x.GetOrCreateCache(true)).Returns(new AzureDigitalTwinCache(
                new ModelCache(
                    new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(
                           Models.Select(x => new KeyValuePair<string, DigitalTwinsModelBasicData>(x.Id, x))),
                    loggerMock.Object),
                new TwinCache(
                new ConcurrentDictionary<string, BasicDigitalTwin>(Twins.Select(x => new KeyValuePair<string, BasicDigitalTwin>(x.Id, x))),
                new ConcurrentDictionary<string, BasicRelationship>(Relationships.Select(x => new KeyValuePair<string, BasicRelationship>(x.Id, x))),
                new ConcurrentDictionary<string, List<string>>(Twins.Where(x => Relationships.Any(r => r.SourceId == x.Id)).Select(x => new KeyValuePair<string, List<string>>(x.Id, Relationships.Where(y => y.SourceId == x.Id).Select(x => x.Id).ToList()))),
                new ConcurrentDictionary<string, HashSet<string>>(Twins.GroupBy(x => x.Metadata.ModelId).Where(x => x.Any()).Select(x => new KeyValuePair<string, HashSet<string>>(x.Key, x.Select(x => x.Id).ToHashSet<string>()))),
                new ConcurrentDictionary<string, List<string>>(Twins.Where(x => Relationships.Any(r => r.TargetId == x.Id)).Select(x => new KeyValuePair<string, List<string>>(x.Id, Relationships.Where(y => y.TargetId == x.Id).Select(x => x.Id).ToList()))))));

            InMemoryAzureDigitalTwinReader = GetServiceReaderInstance();
            InMemoryAzureDigitalTwinWriter = GetServiceWriterInstance();
        }

        protected InMemorySettings Settings { get; }

        public Mock<IAzureDigitalTwinModelParser> AzureDigitalTwinModelParserMock { get; private set; }

        public Mock<IAzureDigitalTwinCacheProvider> AzureDigitalTwinCacheProviderMock { get; private set; }

        public IAzureDigitalTwinReader InMemoryAzureDigitalTwinReader { get; private set; }

        public IAzureDigitalTwinWriter InMemoryAzureDigitalTwinWriter { get; private set; }

        public List<BasicDigitalTwin> Twins { get; private set; }

        public List<BasicRelationship> Relationships { get; private set; }

        public List<DigitalTwinsModelBasicData> Models { get; private set; }

        public BasicDigitalTwin TwinWithRelationship { get; private set; }

        protected virtual IAzureDigitalTwinReader GetServiceReaderInstance()
        {
            return new InMemoryTwinReader(AzureDigitalTwinModelParserMock.Object, AzureDigitalTwinCacheProviderMock.Object);
        }

        protected virtual IAzureDigitalTwinWriter GetServiceWriterInstance()
        {
            return new InMemoryTwinWriter(AzureDigitalTwinModelParserMock.Object, AzureDigitalTwinCacheProviderMock.Object);
        }

        public void Dispose()
        {
        }
    }
}
