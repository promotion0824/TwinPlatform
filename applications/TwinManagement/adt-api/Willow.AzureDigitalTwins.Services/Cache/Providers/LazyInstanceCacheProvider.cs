using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Cache.Providers;

public class LazyInstanceCacheProvider : InstanceCacheProvider
{
    private const string TwinModelPropertyName = "$model";
    private const string TwinIdPropertyName = "$dtId";
    private const string TwinMetadataPropertyName = "$metadata";

    public LazyInstanceCacheProvider(InMemorySettings settings,
        IMemoryCache memoryCache,
        ILogger<LazyInstanceCacheProvider> logger,
        IAzureDigitalTwinReader azureDigitalTwinReader) : base(settings, memoryCache, logger, azureDigitalTwinReader)
    {
    }

    protected async override Task ProcessRelationshipsAsync(ConcurrentDictionary<string, BasicRelationship> relationships, ConcurrentDictionary<string, List<string>> twinRelationships, ConcurrentDictionary<string, List<string>> twinIncomingRelationships)
    {
        var relationshipsQuery = AzureDigitalTwinReader.QueryAsync<BasicRelationship>("select r.$relationshipId, r.$sourceId, r.$targetId from relationships r");
        var pages = await relationshipsQuery.AsPages().ToListAsync();
        var instanceRelationships = pages.SelectMany(x => x.Values).ToList();

        ProcessRelationshipsMapAsync(relationships, twinRelationships, twinIncomingRelationships, instanceRelationships, x =>
        {
            // We clear data to reduce memory and indicate not loaded entity
            x.Properties = null;
            x.Id = null;
        });
    }

    protected async override Task ProcessTwinsAsync(ConcurrentDictionary<string, BasicDigitalTwin> twins, ConcurrentDictionary<string, HashSet<string>> twinsByModel)
    {
        var relationshipsQuery = AzureDigitalTwinReader.QueryAsync<Dictionary<string, string>>($"select dt.{TwinIdPropertyName}, dt.{TwinMetadataPropertyName}.{TwinModelPropertyName} from digitaltwins dt");
        var pages = await relationshipsQuery.AsPages().ToListAsync();
        var instanceTwins = pages.SelectMany(x => x.Values).ToList();

        Parallel.ForEach(instanceTwins, x =>
        {

            if (!twinsByModel.ContainsKey(x[TwinModelPropertyName]))
                twinsByModel.TryAdd(x[TwinModelPropertyName], new HashSet<string>());

            twinsByModel[x[TwinModelPropertyName]].Add(x[TwinIdPropertyName]);

            twins.TryAdd(x[TwinIdPropertyName], null);
        });
    }
}
