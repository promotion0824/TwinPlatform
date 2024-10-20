using System.Collections.Concurrent;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IStorageReader
{
    Task<ConcurrentDictionary<string, T>> ReadFiles<T>(string folder, SearchOption searchOption, Func<string, List<T>> parseFile, Func<T, string> getKey);
}
