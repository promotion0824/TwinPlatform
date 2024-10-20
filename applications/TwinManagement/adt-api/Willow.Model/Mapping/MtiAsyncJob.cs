using Willow.Model.Async;

namespace Willow.Model.Mapping;

public class MtiAsyncJob : AsyncJob
{
    public MtiAsyncJob(string jobId) : base(jobId)
    {
        JobId = jobId;
    }

    public MtiAsyncJob() : base() { }

    /// <summary>
    /// The type of MTI Async job.
    /// </summary>
    public MtiAsyncJobType JobType { get; set; }

    /// <summary>
    /// The Mapped building identifier.
    /// </summary>
    public string? BuildingId { get; set; }

    /// <summary>
    /// The Mapped connector identifier.
    /// </summary>
    public string? ConnectorId { get; set; }    

}


public enum MtiAsyncJobType
{
    SyncOrganization,
    SyncSpatial,
    SyncConnectors,
    SyncAssets,
    SyncCapabilities,
    PushToMapped,
    Ingest
    
}
