using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Services.Storage;

public class ZipFilesReader : IStorageReader
{
    private readonly LocalSystemSettings _settings;
    private readonly IDictionary<string, string> _zipArchiveEntries;

    public ZipFilesReader(LocalSystemSettings settings)
    {
        _settings = settings;
        _zipArchiveEntries = GetZipContent();
    }

    public async Task<ConcurrentDictionary<string, T>> ReadFiles<T>(string folder, SearchOption searchOption, Func<string, List<T>> parseFile, Func<T, string> getKey)
    {
        var files = _zipArchiveEntries.Where(x => x.Key.Contains(folder)).ToList();

        var entities = new ConcurrentDictionary<string, T>();
        var read = Task.Run(() =>
        Parallel.ForEach(files,
            new ParallelOptions { MaxDegreeOfParallelism = 20 },
            x =>
            {
                parseFile(x.Value).ForEach(y => entities.TryAdd(getKey(y), y));
            }));

        await read;

        return entities;
    }

    private Dictionary<string, string> GetZipContent()
    {
        var rawFileStream = File.OpenRead(_settings.Path);
        byte[] zippedtoTextBuffer = new byte[rawFileStream.Length];
        rawFileStream.Read(zippedtoTextBuffer, 0, (int)rawFileStream.Length);

        var files = new Dictionary<string, string>();

        using (var zippedStream = new MemoryStream(zippedtoTextBuffer))
        {
            using (var archive = new ZipArchive(zippedStream))
            {
                foreach (var entry in archive.Entries)
                    using (var unzippedEntryStream = entry.Open())
                    {
                        using (var ms = new MemoryStream())
                        {
                            unzippedEntryStream.CopyTo(ms);
                            var unzippedArray = ms.ToArray();

                            files.Add(entry.FullName, Encoding.Default.GetString(unzippedArray));
                        }
                    }
            }
        }
        return files;
    }
}
