namespace ConnectorCore.Data.Models;

/// <summary>
/// Represents a scan.
/// </summary>
public class Scan
{
    /// <summary>
    /// Gets or sets the unique identifier for the scan.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the associated connector.
    /// </summary>
    public Guid ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the status of the scan.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the message associated with the scan.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the user who created the scan.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the scan was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the start time of the scan.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the scan.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the devices to be scanned.
    /// </summary>
    public string DevicesToScan { get; set; }

    /// <summary>
    /// Gets or sets the count of errors encountered during the scan.
    /// </summary>
    public int? ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the error message encountered during the scan.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the configuration details for the scan.
    /// </summary>
    public string Configuration { get; set; }

    /// <summary>
    /// Gets or sets the associated connector for the scan.
    /// </summary>
    public Connector Connector { get; set; }
}
