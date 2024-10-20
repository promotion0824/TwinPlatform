namespace Willow.AzureDigitalTwins.Services.UnitTests.Cache
{
    using System.Collections.Generic;
    using System.Linq;
    using Azure.DigitalTwins.Core;
    using MemoryCache.Testing.Moq;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Willow.AzureDigitalTwins.Services.Cache.Providers;
    using Willow.AzureDigitalTwins.Services.Configuration;
    using Willow.AzureDigitalTwins.Services.Extensions;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Xunit;

    public class LazyInstanceCacheProviderTests : BaseCacheProvidersTests<LazyInstanceCacheProvider>
    {
        protected override IAzureDigitalTwinCacheProvider GetCacheProvider(InMemorySettings inMemorySettings, ILogger<LazyInstanceCacheProvider> logger)
        {
            var azureDigitalTwinReaderMock = new Mock<IAzureDigitalTwinReader>();

            var twinsDictionary = new List<Dictionary<string, string>>();
            Twins.ForEach(x =>
            {
                var dictionary = new Dictionary<string, string>();
                dictionary.Add("$dtId", x.Id);
                dictionary.Add("$model", x.Metadata.ModelId);

                twinsDictionary.Add(dictionary);
            });

            azureDigitalTwinReaderMock.Setup(x => x.QueryAsync<BasicRelationship>(It.IsAny<string>()))
                .Returns(Relationships.ToAsyncPageable());
            azureDigitalTwinReaderMock.Setup(x => x.QueryAsync<Dictionary<string, string>>(It.IsAny<string>()))
                .Returns(twinsDictionary.ToAsyncPageable());
            azureDigitalTwinReaderMock.Setup(x => x.GetModelsAsync(It.IsAny<string>())).ReturnsAsync(Models);

            return new LazyInstanceCacheProvider(inMemorySettings, Create.MockedMemoryCache(), logger, azureDigitalTwinReaderMock.Object);
        }

        protected override void CustomAsserts(IAzureDigitalTwinCache cache)
        {
            Assert.True(cache.TwinCache.Twins.All(x => x.Value == null));
            Assert.True(cache.TwinCache.Relationships.All(x => string.IsNullOrEmpty(x.Value.Id) && x.Value.Properties == null));
        }
    }
}
