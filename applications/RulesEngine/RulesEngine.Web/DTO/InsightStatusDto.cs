using System;
using Willow.Rules.Model;

namespace RulesEngine.Web.DTO;

/// <summary>
/// Dto for <see cref="InsightStatusDto" />
/// </summary>
public class InsightStatusDto
{
    /// <summary>
    /// Creates a <see cref="InsightStatusDto" /> from an <see cref="InsightChange" />
    /// </summary>
    public InsightStatusDto(InsightChange insightChange)
    {
        this.Timestamp = insightChange.Timestamp;
        this.Status = insightChange.Status;
    }

    /// <summary>
    /// The time of the change
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// The status of the insight at the time of the change
    /// </summary>
    public InsightStatus Status { get; init; }
}
