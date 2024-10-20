using System.Collections.Generic;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Contains summary info on insights
/// </summary>
public class InsightsSummaryDto
{
    /// <summary>
    /// Insights marked not to sync but already pushed to Command
    /// </summary>
    public int TotalNotSynced { get; init; }

    /// <summary>
    /// Insights linked to a command insight
    /// </summary>
    public int TotalLinked { get; init; }

    /// <summary>
    /// Total insights
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// Total insights enabled to sync
    /// </summary>
    public int TotalEnabled { get; init; }

    /// <summary>
    /// Faulted
    /// </summary>
    public int TotalFaulted { get; init; }

    /// <summary>
    /// Invalid
    /// </summary>
    public int TotalInvalid { get; init; }

    /// <summary>
    /// Not faulted
    /// </summary>
    public int TotalValidNotFaulted { get; init; }

    /// <summary>
    /// Insights by model type
    /// </summary>
    public Dictionary<string, int> InsightsByModel { get; set; }
}
