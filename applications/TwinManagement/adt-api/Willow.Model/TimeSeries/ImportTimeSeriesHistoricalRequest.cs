using System.Collections.Generic;

namespace Willow.Model.TimeSeries;

public class ImportTimeSeriesHistoricalRequest
{
    /// <summary>
    /// Time series files to import
    /// </summary>
    public required List<string> FileNames { get; set; }
}
