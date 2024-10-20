namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.DownloadActivityLogs;

using CsvHelper.Configuration;

/// <summary>
/// Get Activity Log Feeds.
/// </summary>
public class DownloadActivityLogsResponseDto
{
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
    /// Gets or sets the activity log description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the twin ID.
    /// </summary>
    public required string TwinId { get; set; }

    /// <summary>
    /// Gets or sets the location of the twin.
    /// </summary>
    public required string Location { get; set; }

    /// <summary>
    /// Gets or sets the asset that hosts the capability or twin.
    /// </summary>
    public string? Parent { get; set; }

    /// <summary>
    /// Gets or sets the user who carried out the activity.
    /// </summary>
    public User? UpdatedBy { get; set; }
}

internal class DownloadActivityLogsResponseDtoMap
    : ClassMap<DownloadActivityLogsResponseDto>
{
    public DownloadActivityLogsResponseDtoMap()
    {
        Map(m => m.Timestamp).Index(0);
        Map(m => m.Type).Index(1);
        Map(m => m.Description).Index(2);
        Map(m => m.CommandName).Index(3);
        Map(m => m.Parent).Index(4);
        Map(m => m.TwinId).Index(5);
        Map(m => m.Location).Index(7);
    }
}
