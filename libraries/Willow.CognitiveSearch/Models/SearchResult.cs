namespace Willow.CognitiveSearch;

/// <summary>
/// A search document with a score.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SearchResult{T}"/> class.
/// </remarks>
/// <typeparam name="T">Type of class representing an Index structure.</typeparam>
/// <param name="searchDto">The document.</param>
/// <param name="score">The score from Azure Cognitive Search.</param>
public class SearchResult<T>(T searchDto, double score)
    where T : new()
{
    /// <summary>
    /// Gets or sets the score from Azure Cognitive Search.
    /// </summary>
    public double Score { get; set; } = score;

    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    public T Document { get; set; } = searchDto;
}
