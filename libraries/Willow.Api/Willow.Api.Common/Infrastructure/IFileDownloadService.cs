namespace Willow.Api.Common.Infrastructure;

/// <summary>
/// Defines the File Download Service.
/// </summary>
public interface IFileDownloadService
{
    /// <summary>
    /// Downloads a JSON file from the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of object to return from the file.</typeparam>
    /// <param name="uri">The URI of the file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<T?> DownloadJsonFile<T>(string uri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from the specified URI.
    /// </summary>
    /// <param name="uri">The URI of the file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<byte[]> DownloadFile(string uri, CancellationToken cancellationToken);
}
