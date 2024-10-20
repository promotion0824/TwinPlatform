namespace ConnectorCore.Data.Models;

/// <summary>
/// A log entry.
/// </summary>
public class Log
{
    /// <summary>
    /// Gets or sets the unique identifier for the log.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the start time of the log entry.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the log entry.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the associated connector.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the count of points processed in the log entry.
    /// </summary>
    public int PointCount { get; set; }

    /// <summary>
    /// Gets or sets the count of errors encountered in the log entry.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the count of retries attempted in the log entry.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the error messages encountered in the log entry.
    /// </summary>
    public string Errors { get; set; }

    /// <summary>
    /// Gets or sets the source of the log entry.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the log entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the associated connector for the log entry.
    /// </summary>
    public Connector Connector { get; set; }
}
