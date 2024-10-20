namespace Willow.AzureDigitalTwins.Services.UnitTests.Cache
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Azure.DigitalTwins.Core;
    using MemoryCache.Testing.Moq;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Willow.AzureDigitalTwins.Services.Cache.Providers;
    using Willow.AzureDigitalTwins.Services.Configuration;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.Model.Adt;
    using Willow.Model.Requests;

    public class LocalFilesCacheProviderTests : BaseCacheProvidersTests<LocalFilesCacheProvider>
    {
        protected override IAzureDigitalTwinCacheProvider GetCacheProvider(InMemorySettings inMemorySettings, ILogger<LocalFilesCacheProvider> logger)
        {
            var storageReaderMock = new Mock<IStorageReader>();

            storageReaderMock.Setup(x => x.ReadFiles<BasicDigitalTwin>(It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<Func<string, List<BasicDigitalTwin>>>(), It.IsAny<Func<BasicDigitalTwin, string>>()))
                .ReturnsAsync(new ConcurrentDictionary<string, BasicDigitalTwin>(Twins.ToDictionary(x => x.Id, x => x)));
            storageReaderMock.Setup(x => x.ReadFiles<BasicRelationship>(It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<Func<string, List<BasicRelationship>>>(), It.IsAny<Func<BasicRelationship, string>>()))
                .ReturnsAsync(new ConcurrentDictionary<string, BasicRelationship>(Relationships.ToDictionary(x => x.Id, x => x)));
            storageReaderMock.Setup(x => x.ReadFiles<DigitalTwinsModelBasicData>(It.IsAny<string>(), It.IsAny<SearchOption>(), It.IsAny<Func<string, List<DigitalTwinsModelBasicData>>>(), It.IsAny<Func<DigitalTwinsModelBasicData, string>>()))
                .ReturnsAsync(new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(Models.ToDictionary(x => x.Id, x => x)));

            return new LocalFilesCacheProvider(inMemorySettings, storageReaderMock.Object, Create.MockedMemoryCache(), logger);
        }
    }
}
