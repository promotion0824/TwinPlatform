namespace Willow.LiveData.Core.Domain;

using System.Collections.Generic;

/// <summary>
/// Represents time series multistate data.
/// </summary>
internal class TimeSeriesMultiStateData : TimeSeriesData
{
    public Dictionary<string, int> State { get; init; }
}
