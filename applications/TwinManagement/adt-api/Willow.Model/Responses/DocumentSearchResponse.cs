namespace Willow.Model.Response;

public record DocumentSearchResponse
{
    /// <summary>
    /// Document Search Response.
    /// </summary>
    /// <param name="skip">Number of results to skip.</param>
    /// <param name="take">Number of results to take.</param>
    /// <param name="totalCount">Total matched results. Client may send this back to server to avoid performance overhead with recounting.</param>
    public DocumentSearchResponse(int skip, int take, long totalCount)
    {
        Skip = skip;
        Take = take;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Gets or sets the number of results to skip.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Gets or sets the number of results to take.
    /// </summary>
    public int Take { get; set; }

    /// <summary>
    /// Gets or sets the total matched results. Client may send this back to server to avoid performance overhead with recounting.
    /// </summary>
    public long TotalCount { get; set; } = 0;

    /// <summary>
    /// Document Search Result dictionary.
    /// </summary>
    public Dictionary<string, DocumentSearchResult> Results { get; set; } = [];

    /// <summary>
    /// Adds a document search result to the response.
    /// </summary>
    /// <param name="result"></param>
    public void AddResult(DocumentSearchResult result)
    {
        if (Results.TryGetValue(result.Id, out var existingResult))
        {

            existingResult.Chunks.AddRange(result.Chunks);
        }
        else
        {
            Results.Add(result.Id, result);
        }
    }

}

/// <summary>
/// Document Search Result.
/// </summary>
public record DocumentSearchResult
{
    /// <summary>
    /// Average score calculated from the individual chunks relevance score.
    /// </summary>
    public double Score
    {
        get
        {
            return Chunks?.Max(x => x.Score) ?? 0;
        }
    }

    /// <summary>
    /// Gets or sets the unique Id of the document.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of chunked document text.
    /// </summary>
    public List<ScoredDocumentChunk> Chunks { get; set; } = [];

    /// <summary>
    /// Gets or sets the File name of the document.
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Absolute url of url of the document assigned by the storage account.
    /// </summary>
    public string Path { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Last Modified date and time of the document.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }
}

/// <summary>
/// Record to hold the document chunk and its relevance score.
/// </summary>
/// <param name="Chunk">Document chunk text.</param>
/// <param name="Score">Relevance score.</param>
public record ScoredDocumentChunk(string Chunk, double Score);
