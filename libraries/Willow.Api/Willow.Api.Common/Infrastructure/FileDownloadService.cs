namespace Willow.Api.Common.Infrastructure;

using System.Text.Json;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the File Download Service.
/// </summary>
public class FileDownloadService : IFileDownloadService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<FileDownloadService> logger;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="FileDownloadService"/> class.
    /// </summary>
    /// <param name="httpClient">An instance of an Http Client.</param>
    /// <param name="logger">An ILogger Instance.</param>
    public FileDownloadService(HttpClient httpClient, ILogger<FileDownloadService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<T?> DownloadJsonFile<T>(string uri, CancellationToken cancellationToken = default)
    {
        using (logger.BeginScope("{FileUri}", uri.Split("?")[0]))
        {
            using var response = await httpClient.GetAsync(uri, cancellationToken);
            logger.LogTrace("HttpClient returned {HttpStatusCode}", response.StatusCode);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream, leaveOpen: true);
            var result = await JsonSerializer.DeserializeAsync<T>(reader.BaseStream, JsonSerializerOptions, cancellationToken);

            logger.LogTrace("Deserialized file contents");
            return result;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> DownloadFile(string uri, CancellationToken cancellationToken)
    {
        using (logger.BeginScope("{FileUri}", uri.Split("?")[0]))
        {
            using var response = await httpClient.GetAsync(uri, cancellationToken);
            logger.LogTrace("HttpClient returned {HttpStatusCode}", response.StatusCode);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }
    }
}
