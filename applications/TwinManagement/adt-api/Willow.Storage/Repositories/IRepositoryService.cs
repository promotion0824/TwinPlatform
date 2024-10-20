namespace Willow.Storage.Repositories;

public interface IRepositoryService
{
    Dictionary<string, string> ReadContent(Stream content, string? path = null, string? extension = null);
    Task<IDictionary<string, string>> GetRepositoryContent(string owner,
        string repository,
        string? @ref = null,
        IEnumerable<string>? submodules = null,
        string? path = null,
        string? extension = null);
}
