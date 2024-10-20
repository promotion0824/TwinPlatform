using Willow.Model.Adt;

namespace Willow.Model.Async;

public class AsyncJob
{
    protected AsyncJob(string jobId) : this()
    {
        JobId = jobId;
    }

    protected AsyncJob()
    {
        Details = new AsyncJobDetails { Status = AsyncJobStatus.Queued };
        CreateTime = DateTime.UtcNow;
        Target = new List<EntityType>();
    }

    public string? JobId { get; set; }

    public AsyncJobDetails Details { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public string? UserId { get; set; }

    public string? UserData { get; set; }

    public List<EntityType> Target { get; set; }
}
