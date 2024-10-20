namespace Willow.AzureDigitalTwins.Services.UnitTests.Cache
{
    using AutoFixture;
    using Azure.DigitalTwins.Core;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Willow.AzureDigitalTwins.Services.Configuration;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.Model.Adt;

    public abstract class BaseCacheProvidersTests<T>
        where T : IAzureDigitalTwinCacheProvider
    {
        private readonly IAzureDigitalTwinCacheProvider cacheProvider;
        private readonly Fixture fixture;
        private readonly Mock<ILogger<T>> logger;

        public BaseCacheProvidersTests()
        {
            fixture = new Fixture();
            logger = new Mock<ILogger<T>>();
            Twins = new List<BasicDigitalTwin>();
            Relationships = new List<BasicRelationship>();

            Models = fixture.CreateMany<DigitalTwinsModelBasicData>(2).ToList();

            for (int i = 0; i < 5; i++)
            {
                Twins.Add(fixture.Build<BasicDigitalTwin>().With(x => x.Metadata, new DigitalTwinMetadata { ModelId = Models[0].Id }).Create());
            }

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.SourceId, Twins[0].Id)
                .With(x => x.TargetId, Twins[1].Id)
                .Create());

            Relationships.Add(fixture.Build<BasicRelationship>()
                .With(x => x.SourceId, Twins[2].Id)
                .With(x => x.TargetId, Twins[3].Id)
                .Create());

            cacheProvider = GetCacheProvider(fixture.Create<InMemorySettings>(), logger.Object);
        }

        public List<BasicDigitalTwin> Twins { get; private set; }

        public List<BasicRelationship> Relationships { get; private set; }

        public List<DigitalTwinsModelBasicData> Models { get; private set; }

        protected abstract IAzureDigitalTwinCacheProvider GetCacheProvider(InMemorySettings inMemorySettings, ILogger<T> logger);

        protected virtual void CustomAsserts(IAzureDigitalTwinCache cache) { }
    }
}
