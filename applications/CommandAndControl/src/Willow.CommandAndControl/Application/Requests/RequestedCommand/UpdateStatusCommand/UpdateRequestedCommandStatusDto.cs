namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.UpdateStatusCommand;

/// <summary>
/// Update RequestedCommand Status as Approved or Rejected.
/// </summary>
/// <param name="Action">Requested Command can only be Approve or Reject.</param>
public record UpdateRequestedCommandStatusDto(string Action);
