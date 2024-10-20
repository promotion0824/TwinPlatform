using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Cache.Providers;

public class EmptyCacheProvider : BaseCacheProvider<EmptyCacheProvider>
{
    protected IAzureDigitalTwinReader AzureDigitalTwinReader { get; }

    public EmptyCacheProvider(InMemorySettings settings,
        IMemoryCache memoryCache,
        ILogger<EmptyCacheProvider> logger,
        IAzureDigitalTwinReader azureDigitalTwinReader) : base(settings, memoryCache, logger)
    {
        AzureDigitalTwinReader = azureDigitalTwinReader;
    }

    protected override Task ProcessTwinsAsync(ConcurrentDictionary<string, BasicDigitalTwin> twins, ConcurrentDictionary<string, HashSet<string>> twinsByModel)
    {
        return Task.CompletedTask;
    }

    protected override Task ProcessRelationshipsAsync(ConcurrentDictionary<string, BasicRelationship> relationships, ConcurrentDictionary<string, List<string>> twinRelationships, ConcurrentDictionary<string, List<string>> twinIncomingRelationships)
    {
        return Task.CompletedTask;
    }

    protected async override Task<ConcurrentDictionary<string, DigitalTwinsModelBasicData>> GetModelsAsync()
    {
        var instanceModels = await AzureDigitalTwinReader.GetModelsAsync();
        return new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(instanceModels.ToDictionary(x => x.Id, x => x));
    }
}
