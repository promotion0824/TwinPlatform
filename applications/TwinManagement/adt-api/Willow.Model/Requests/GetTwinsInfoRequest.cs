using Willow.Model.Adt;

namespace Willow.Model.Requests;

public class GetTwinsInfoRequest 
{
    public GetTwinsInfoRequest()
    {
        ModelId = [];
        LocationId = null;
        ExactModelMatch = false;
        IncludeRelationships = false;
        IncludeIncomingRelationships = false;
        OrphanOnly = false;
        SourceType = SourceType.Adx;
        RelationshipsToTraverse = [];
        SearchString = string.Empty;
        StartTime = null;
        EndTime = null;
        QueryFilter = new();

    }
    public string[] ModelId { get; set; }

    public string? LocationId { get; set; }

    public bool ExactModelMatch { get; set; }

    public bool IncludeRelationships { get; set; }

    public bool IncludeIncomingRelationships { get; set; }

    public bool OrphanOnly { get; set; }

    public SourceType SourceType { get; set; }

    public string[] RelationshipsToTraverse { get; set; }

    public string SearchString { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }
    public QueryFilter QueryFilter { get; set; }
}

// Filter that adds to the Where clause
public class QueryFilter
{
    public enum QueryFilterType { Direct = 0 };

    public QueryFilterType Type { get; set; } = QueryFilterType.Direct;
    public string? Filter { get; set; } = string.Empty; // supports exact filter condition for ADX or ADT
                                                        // (select the correct source type in GetTwinsInfoRequest)
}
