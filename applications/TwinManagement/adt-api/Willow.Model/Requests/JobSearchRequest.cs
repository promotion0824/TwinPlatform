using Willow.Model.Async;

namespace Willow.Model.Requests;

public class JobSearchRequest
{
    public string[] JobTypes { get; set; } = [];

    public string? JobSubType { get; set; } = null;

    public AsyncJobStatus[]? JobStatuses { get; set; } = null;

    public string? UserId { get; set; } = null;

    public int pageSize { get; set; } = 100;

    public int offset { get; set; } = 0;

    public bool? IsDeleted { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? EndDate { get; set; }
}
