namespace Willow.AzureDigitalTwins.Services.UnitTests.Cache
{
    using System.Collections.Generic;
    using MemoryCache.Testing.Moq;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Willow.AzureDigitalTwins.Services.Cache.Providers;
    using Willow.AzureDigitalTwins.Services.Configuration;
    using Willow.AzureDigitalTwins.Services.Extensions;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.Model.Requests;

    public class InstanceCacheProviderTests : BaseCacheProvidersTests<InstanceCacheProvider>
    {
        protected override IAzureDigitalTwinCacheProvider GetCacheProvider(InMemorySettings inMemorySettings, ILogger<InstanceCacheProvider> logger)
        {
            var azureDigitalTwinReaderMock = new Mock<IAzureDigitalTwinReader>();
            var request = new GetTwinsInfoRequest();
            azureDigitalTwinReaderMock.Setup(x => x.GetTwinsAsync(request,
                                /*twinIds:*/  It.IsAny<string[]>(),
                                /*pageSize:*/ It.IsAny<int>(),
                                /*IncludeCountQuery*/ It.IsAny<bool>(),
                                /*continuationToken:*/ It.IsAny<string>()))
                    .ReturnsAsync(Twins.ToPageModel(1, 500));

            azureDigitalTwinReaderMock.Setup(x => x.GetRelationshipsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(Relationships);
            azureDigitalTwinReaderMock.Setup(x => x.GetModelsAsync(It.IsAny<string>())).ReturnsAsync(Models);

            return new InstanceCacheProvider(inMemorySettings, Create.MockedMemoryCache(), logger, azureDigitalTwinReaderMock.Object);
        }
    }
}
