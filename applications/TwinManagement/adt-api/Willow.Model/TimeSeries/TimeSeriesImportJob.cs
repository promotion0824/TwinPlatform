using Willow.Model.Async;

namespace Willow.Model.TimeSeries;

public class TimeSeriesImportJob : AsyncJob
{
    public TimeSeriesImportJob() : base() { }

    public TimeSeriesImportJob(string jobId) : base(jobId) { }

    /// <summary>
    /// Error messages for entities that failed to import
    /// </summary>
    public IDictionary<string, string> EntitiesError { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The time series import job request path in blob storage container
    /// </summary>
    public string RequestPath { get; set; } = null!;

    /// <summary>
    /// Number of entities that have been processed
    /// </summary>
    public int ProcessedEntities { get; set; }

    /// <summary>
    /// Total number of entities to be processed
    /// </summary>
    public int TotalEntities { get; set; }

    /// <summary>
    /// True if import time series from blob sas url
    /// </summary>
    public bool isSasUrlImport { get; set; } = false;
}
