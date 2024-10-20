using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Cache.Providers;

public class InstanceCacheProvider : BaseCacheProvider<InstanceCacheProvider>
{
    protected IAzureDigitalTwinReader AzureDigitalTwinReader { get; }

    public InstanceCacheProvider(InMemorySettings settings,
        IMemoryCache memoryCache,
        ILogger<InstanceCacheProvider> logger,
        IAzureDigitalTwinReader azureDigitalTwinReader) : base(settings, memoryCache, logger)
    {
        AzureDigitalTwinReader = azureDigitalTwinReader;
    }

    protected async override Task ProcessTwinsAsync(ConcurrentDictionary<string, BasicDigitalTwin> twins, ConcurrentDictionary<string, HashSet<string>> twinsByModel)
    {
        var page = await AzureDigitalTwinReader.GetTwinsAsync();
        var instanceTwins = await page.FetchAll(x => AzureDigitalTwinReader.GetTwinsAsync(continuationToken: x.ContinuationToken));

        Parallel.ForEach(instanceTwins, x =>
        {
            twins.TryAdd(x.Id, x);
            if (!twinsByModel.ContainsKey(x.Metadata.ModelId))
                twinsByModel.TryAdd(x.Metadata.ModelId, new HashSet<string>());

            twinsByModel[x.Metadata.ModelId].Add(x.Id);
        });
    }

    protected async override Task ProcessRelationshipsAsync(ConcurrentDictionary<string, BasicRelationship> relationships, ConcurrentDictionary<string, List<string>> twinRelationships, ConcurrentDictionary<string, List<string>> twinIncomingRelationships)
    {
        var instanceRelationships = await AzureDigitalTwinReader.GetRelationshipsAsync(Enumerable.Empty<string>());

        ProcessRelationshipsMapAsync(relationships, twinRelationships, twinIncomingRelationships, instanceRelationships);
    }

    protected async override Task<ConcurrentDictionary<string, DigitalTwinsModelBasicData>> GetModelsAsync()
    {
        var instanceModels = await AzureDigitalTwinReader.GetModelsAsync();
        return new ConcurrentDictionary<string, DigitalTwinsModelBasicData>(instanceModels.ToDictionary(x => x.Id, x => x));
    }
}
