namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetRequestedCommandsCount;

/// <summary>
/// Get count of Requested Commands.
/// </summary>
public class GetRequestedCommandsCountDto
{
    /// <summary>
    /// Gets or sets the status to filter by.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequestedCommandStatus? Status { get; set; }

    /// <summary>
    /// Gets the filter specifications.
    /// </summary>
    public FilterSpecificationDto[] FilterSpecifications { get; init; } = [];
}
