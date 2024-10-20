namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetActivityLogs;

/// <summary>
/// Get Activity Log Feeds.
/// </summary>
public class ActivityLogsResponseDto
{
    /// <summary>
    /// Gets or sets the Id of the activity log entry.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Id of the resolved command.
    /// </summary>
    public Guid RequestedCommandId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the resolved command.
    /// </summary>
    public Guid? ResolvedCommandId { get; set; }

    /// <summary>
    /// Gets or sets the requested command's name.
    /// </summary>
    public required string CommandName { get; set; }

    /// <summary>
    /// Gets or sets the date when the activity was logged.
    /// </summary>
    public required DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the activity type.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ActivityType Type { get; set; }

    /// <summary>
    /// Gets or sets the set value of the command.
    /// </summary>
    public double? Value { get; set; }

    /// <summary>
    /// Gets or sets the unit of the value.
    /// </summary>
    public required string Unit { get; set; }

    /// <summary>
    /// Gets or sets the requested command's start time.
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the requested command's end time.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the site ID.
    /// </summary>
    public required string SiteId { get; set; }

    /// <summary>
    /// Gets or sets the rule ID.
    /// </summary>
    public required string RuleId { get; set; }

    /// <summary>
    /// Gets or sets the twin ID.
    /// </summary>
    public required string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public required string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the connector ID.
    /// </summary>
    public required string ConnectorId { get; set; }

    /// <summary>
    /// Gets or sets the location of the twin.
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// Gets or sets the asset that hosts the capability.
    /// </summary>
    public string? IsCapabilityOf { get; set; }

    /// <summary>
    /// Gets or sets the asset that hosts the twin.
    /// </summary>
    public string? IsHostedBy { get; set; }

    /// <summary>
    /// Gets or sets the user who carried out the activity.
    /// </summary>
    public User? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets any extra information logged as part of the activity.
    /// </summary>
    public string? ExtraInfo { get; set; }
}
