using RulesEngine.Web;
using System;
using System.Collections.Generic;
using Willow.Rules.Model;

namespace WillowRules.DTO;

/// <summary>
/// Represents a status for a time range
/// </summary>
public class TrendlineStatusDto
{
    /// <summary>
    /// Values for this status
    /// </summary>
    public TrendlineDto Values { get; init; }

    /// <summary>
    /// The status
    /// </summary>
    public TimeSeriesStatus Status { get; init; }
}
