namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// Class to hold search filter properties while fetching all records
/// </summary>
public class FilterPropertyModel
{

    /// <summary>
    /// Text to search while retrieving records
    /// </summary>
    public string? SearchText { get; set; } = string.Empty;

    /// <summary>
    /// Filter query for advanced filtering logic
    /// </summary>
    public string? FilterQuery { get; set; } = string.Empty;

    /// <summary>
    /// Count of row to skip before fetch
    /// </summary>
    public int? Skip { get; set; } = null!;

    /// <summary>
    /// Count to rows to fetch after optional skip rows
    /// </summary>
    public int? Take { get; set; } = null!;

    /// <summary>
    /// Transform Filter Property Model object in to query parameters
    /// </summary>
    /// <returns>Dictionary of string, string?</returns>
    internal Dictionary<string,string?> ToQueryParams()
    {
        var queryParams = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            queryParams.Add(nameof(SearchText), SearchText);
        }
        if (!string.IsNullOrWhiteSpace(FilterQuery))
        {
            queryParams.Add(nameof(FilterQuery), FilterQuery);
        }
        if (Skip.HasValue)
        {
            queryParams.Add(nameof(Skip), Skip.ToString());
        }
        if (Take.HasValue)
        {
            queryParams.Add(nameof(Take), Take.ToString());
        }
        return queryParams; 
    }
}


