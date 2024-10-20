using Microsoft.Extensions.Logging;
using Octokit;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

namespace Willow.Storage.Repositories;

public class GitHubRepositoryService : IRepositoryService
{
    private const string GithubApiDomain = "api.github.com";
    private const string GithubReposUrl = $"https://{GithubApiDomain}/repos/";
    private readonly ILogger<GitHubRepositoryService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubRepositoryService(ILogger<GitHubRepositoryService> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _httpClientFactory = clientFactory;
    }

    private async Task<Stream> GetRepositoryStream(string owner, string repository, string? @ref = null)
    {
        var client = _httpClientFactory.CreateClient(nameof(GitHubRepositoryService));
        var url = $"{GithubReposUrl}{owner}/{repository}/zipball";
        if (@ref is not null)
            url += $"/{@ref}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<IDictionary<string, string>> GetRepositoryContent(string owner,
        string repository,
        string? @ref = null,
        IEnumerable<string>? submodules = null,
        string? path = null,
        string? extension = null)
    {
        var repos = new List<(string, string, string?)>();
        repos.Add((owner, repository, @ref));

        if (submodules != null && submodules.Any())
        {
            var repositoryClient = new GitHubClient(new ProductHeaderValue("Other"));
            foreach (var submodule in submodules)
            {
                try
                {
                    var submoduleInfo = await repositoryClient.Repository.Content.GetAllContents(owner, repository, submodule);
                    foreach (var info in submoduleInfo)
                    {
                        if (info.Type.Value == ContentType.Submodule)
                        {
                            var source = info.GitUrl.Replace(GithubReposUrl, string.Empty).Split("/");
                            repos.Add((source[0], source[1], info.Sha));
                        }
                    }
                }
                catch (NotFoundException ex)
                {
                    _logger.LogError(ex, "Error retrieving submodule, owner {Owner}, repository {Repository}, submodule {Submodule}", owner, repository, submodule);
                    throw new Exceptions.Exceptions.NotFoundException($"Submodule {submodule} not found");
                }
            }
        }

        var files = new ConcurrentDictionary<string, string>();

        var loadFiles = repos.Select(async x =>
        {
            var repoContent = await GetRepositoryStream(x.Item1, x.Item2, x.Item3);
            foreach (var file in ReadContent(repoContent, path, extension))
                files.TryAdd(file.Key, file.Value);
        });

        await Task.WhenAll(loadFiles);

        return files;

    }

    public Dictionary<string, string> ReadContent(Stream content, string? path = null, string? extension = null)
    {
        var files = new Dictionary<string, string>();
        using (var rawFileStream = content)
        {
            byte[] zippedtoTextBuffer = new byte[rawFileStream.Length];
            rawFileStream.Read(zippedtoTextBuffer, 0, (int)rawFileStream.Length);

            using (var zippedStream = new MemoryStream(zippedtoTextBuffer))
            using (var archive = new ZipArchive(zippedStream))
            {
                foreach (var entry in archive.Entries)
                {
                    using (var unzippedEntryStream = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        unzippedEntryStream.CopyTo(ms);
                        var unzippedArray = ms.ToArray();

                        var matchesPath = path is null || entry.FullName.Contains(path, StringComparison.InvariantCultureIgnoreCase);
                        var matchesExtensions = extension is null || entry.Name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase);
                        if (matchesPath && matchesExtensions)
                        {
                            // Encoding UTF8 BOM breaks json parsing
                            var data = RemoveBom(unzippedArray);
                            files.Add(entry.FullName, Encoding.UTF8.GetString(data));
                        }
                    }
                }
            }
        }
        return files;
    }

    private static ReadOnlySpan<byte> RemoveBom(byte[] stream)
    {
        var data = new ReadOnlySpan<byte>(stream);
        var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

        if (!data.StartsWith(utf8Bom))
            return data;

        return data.Slice(utf8Bom.Length);
    }
}
