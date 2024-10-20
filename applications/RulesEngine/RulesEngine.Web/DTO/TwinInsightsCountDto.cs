namespace RulesEngine.Web.DTO;

/// <summary>
/// Twin's insight count
/// </summary>
public class TwinInsightsCountDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public TwinInsightsCountDto(string twinId, int insightsCount)
    {
        TwinId = twinId;
        InsightsCount = insightsCount;
    }

    /// <summary>
    /// The twin Id
    /// </summary>
    public string TwinId { get; init; }

    /// <summary>
    /// The insights count for the twin
    /// </summary>
    public int InsightsCount { get; init; }
}
