namespace Willow.PublicApi.Expressions;

using Willow.Model.Requests;

internal record QueryResult
{
    public QueryResult(GetTwinsInfoRequest getTwinsInfoRequest)
    {
        Success = true;
        Request = getTwinsInfoRequest;
    }

    public QueryResult(string query)
    {
        Success = true;
        Query = query;
    }

    private QueryResult()
    {
    }

    public static QueryResult Failed => new();

    public bool Success { get; }

    public GetTwinsInfoRequest? Request { get; }

    /// <summary>
    /// Gets the query string.
    /// </summary>
    /// <remarks>
    /// Not currently supported.
    /// </remarks>
    public string? Query { get; }

    public override string ToString()
    {
        if (Request != null)
        {
            return $"request:{Request.LocationId}";
        }

        if (Query != null)
        {
            return $"query:{Query}";
        }

        return "failed";
    }
}
