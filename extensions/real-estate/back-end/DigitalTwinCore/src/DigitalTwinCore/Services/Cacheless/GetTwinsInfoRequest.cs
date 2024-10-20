using System;

namespace DigitalTwinCore.Services.Cacheless;

public class GetTwinsInfoRequest
{
    public GetTwinsInfoRequest()
    {
        ModelId = new string[] { };
        LocationId = null;
        ExactModelMatch = false;
        IncludeRelationships = false;
        IncludeIncomingRelationships = false;
        SourceType = SourceType.Adx;
        RelationshipsToTraverse = new string[] { };
        SearchString = string.Empty;
        StartTime = null;
        EndTime = null;
    }
    public string[] ModelId { get; set; }

    public string? LocationId { get; set; }

    public bool ExactModelMatch { get; set; }

    public bool IncludeRelationships { get; set; }

    public bool IncludeIncomingRelationships { get; set; }

    public SourceType SourceType { get; set; }

    public string[] RelationshipsToTraverse { get; set; }

    public string SearchString { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }
}

public enum SourceType
{
    Adx,
    AdtQuery,
    AdtMemory
}
