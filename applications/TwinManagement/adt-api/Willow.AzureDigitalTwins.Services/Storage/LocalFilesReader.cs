using System.Collections.Concurrent;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Storage;

public class LocalFilesReader : IStorageReader
{
    private readonly LocalSystemSettings _settings;

    public LocalFilesReader(LocalSystemSettings settings)
    {
        _settings = settings;
    }

    public async Task<ConcurrentDictionary<string, T>> ReadFiles<T>(string folder, SearchOption searchOption, Func<string, List<T>> parseFile, Func<T, string> getKey)
    {
        var folderPath = Path.Combine(_settings.Path, folder);

        var entities = new ConcurrentDictionary<string, T>();

        if (!Directory.Exists(folderPath))
            return entities;

        var files = Directory.GetFiles(folderPath, "*.json", searchOption).ToList();

        await Parallel.ForEachAsync(files, async (x, cancelationToken) =>
        {
            var fileContent = await File.ReadAllTextAsync(x, cancelationToken);
            parseFile(fileContent).ForEach(y => entities.TryAdd(getKey(y), y));
        });

        return entities;
    }
}
