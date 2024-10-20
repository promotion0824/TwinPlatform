namespace Willow.Model.TimeSeries;

public class ImportTimeSeriesHistoricalFromBlobRequest
{
    /// <summary>
    /// Uri to blob storage with sas token to import time series from
    /// </summary>
    public required string SasUri { get; set; }
}
