using Willow.Model.Adt;

namespace Willow.Model.Async;

public class TwinsValidationJob : AsyncJob
{
    public TwinsValidationJob(string jobId, EntityType type) : base(jobId)
    {
        if (Target == null)
            Target = new List<EntityType>();
        Target.Add(type);
        SummaryDetails = new TwinValidationJobSummaryDetails()
        {
            ProcessedEntities = 0,
            ErrorsByModel = new Dictionary<string, TwinValidationJobSummaryDetailErrors>(),
            ModelsQueried = new List<string>()
        };
    }

    public TwinsValidationJob() : base() { }

    public List<string> ModelIds { get; set; } = new List<string>();

    public bool? ExactModelMatch { get; set; }

    public string? LocationId { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public TwinValidationJobSummaryDetails? SummaryDetails { get; set; }
}

public class TwinValidationJobSummaryDetails
{
    public int ProcessedEntities { get; set; }

    public List<string> ModelsQueried { get; set; } = new List<string>();

    public IDictionary<string, TwinValidationJobSummaryDetailErrors> ErrorsByModel { get; set; } = new Dictionary<string, TwinValidationJobSummaryDetailErrors>();
    // We could just expose this dict and have the client compute the roll-ups and remove TVJSDE below
    // public Dictionary<string, Dictionary<Result, Dictionary<CheckType, int>>> ResultDetails { get; set; }
}

public class TwinValidationJobSummaryDetailErrors
{
    public int NumOK { get; set; }
    public int NumPropertyOK { get; set; }
    public int NumRelationshipOK { get; set; }
    public int NumErrors { get; set; }
    public int NumPropertyErrors { get; set; }
    public int NumRelationshipErrors { get; set; }
    public int NumUnitErrors { get; set; }
}
