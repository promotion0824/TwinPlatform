namespace Willow.Model.Async;

// TODO Should be referenced (this is a duplicate type)
public class AsyncJobDetails
{
    /// <summary>
    /// Set when processing is triggered
    /// </summary>
    public DateTime? StartTime { get; set; }
    /// <summary>
    /// Set when process finished
    /// </summary>
    public DateTime? EndTime { get; set; }
    /// <summary>
    /// Current status for the process
    /// </summary>
    public AsyncJobStatus Status { get; set; }
    /// <summary>
    /// Additional information
    /// </summary>
    public string? StatusMessage { get; set; }
}
