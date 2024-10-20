namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.DownloadActivityLogs;

/// <summary>
/// Request to download activity logs.
/// </summary>
public class DownloadActivityLogsRequestDto
{
    /// <summary>
    /// Gets the specifications on how to sort the batch.
    /// </summary>
    public SortSpecificationDto[] SortSpecifications { get; init; } = [];

    /// <summary>
    /// Gets the specifications on how to filter the batch.
    /// </summary>
    public FilterSpecificationDto[] FilterSpecifications { get; init; } = [];
}
