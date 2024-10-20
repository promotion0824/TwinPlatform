using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Cache.Providers;

public class LocalFilesCacheProvider : BaseCacheProvider<LocalFilesCacheProvider>
{
    private readonly IStorageReader _localFilesReader;

    public LocalFilesCacheProvider(InMemorySettings settings, IStorageReader localFilesReader, IMemoryCache memoryCache, ILogger<LocalFilesCacheProvider> logger)
        : base(settings, memoryCache, logger)
    {
        _localFilesReader = localFilesReader;
    }

    private List<DigitalTwinsModelBasicData> ParseModelFile(string content)
    {
        return JsonSerializer.Deserialize<List<DigitalTwinsModelBasicData>>(content);
    }

    private List<DigitalTwinsModelBasicData> ParseStructuredModelFile(string content)
    {
        return new List<DigitalTwinsModelBasicData> { JsonSerializer.Deserialize<DigitalTwinsModelBasicData>(content) };
    }

    protected async override Task ProcessTwinsAsync(ConcurrentDictionary<string, BasicDigitalTwin> twins, ConcurrentDictionary<string, HashSet<string>> twinsByModel)
    {
        var sourceTwins = await _localFilesReader.ReadFiles("Twins", SearchOption.AllDirectories, x => JsonSerializer.Deserialize<List<BasicDigitalTwin>>(x), x => x.Id);

        Parallel.ForEach(sourceTwins, x =>
        {
            twins.TryAdd(x.Key, x.Value);
            if (!twinsByModel.ContainsKey(x.Value.Metadata.ModelId))
                twinsByModel.TryAdd(x.Value.Metadata.ModelId, new HashSet<string>());

            twinsByModel[x.Value.Metadata.ModelId].Add(x.Key);
        });
    }

    protected async override Task<ConcurrentDictionary<string, DigitalTwinsModelBasicData>> GetModelsAsync()
    {
        return await _localFilesReader.ReadFiles<DigitalTwinsModelBasicData>("Models", SearchOption.AllDirectories, Settings.LocalSystem.Structured ? ParseStructuredModelFile : ParseModelFile, x => x.Id);
    }

    protected async override Task ProcessRelationshipsAsync(ConcurrentDictionary<string, BasicRelationship> relationships, ConcurrentDictionary<string, List<string>> twinRelationships, ConcurrentDictionary<string, List<string>> twinIncomingRelationships)
    {
        var sourceRelationships = await _localFilesReader.ReadFiles("Relationships", SearchOption.AllDirectories, x => JsonSerializer.Deserialize<List<BasicRelationship>>(x), x => x.Id);

        ProcessRelationshipsMapAsync(relationships, twinRelationships, twinIncomingRelationships, sourceRelationships.Select(x => x.Value));
    }
}
