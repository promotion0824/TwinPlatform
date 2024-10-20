namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.UpdateRequestedCommandsStatus;

/// <summary>
/// Update RequestedCommand Status as Approved or Rejected.
/// </summary>
/// <param name="ApproveIds">The ids of the approved commands.</param>
/// /// <param name="RejectIds">The ids of the rejected commands.</param>
public record UpdateRequestedCommandsStatusDto(IEnumerable<string> ApproveIds, IEnumerable<string> RejectIds);
