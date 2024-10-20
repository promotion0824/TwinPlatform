namespace Willow.CommandAndControl.Application.Services.Abstractions;

/// <summary>
/// Provides command management functionality for Requested/Resolved Commands.
/// </summary>
public interface ICommandManager
{
    /// <summary>
    /// Approve Request Command to be resolved for execution.
    /// </summary>
    /// <param name="id">Id of the Requested Command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation success.</returns>
    Task<bool> ApproveRequestedCommandAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Reject Requested Command. Rejected commands cannot be actioned.
    /// </summary>
    /// <param name="id">Id of the Requested Command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Operation success.</returns>
    Task<bool> RejectRequestedCommandAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Get overlapping commands for the given External Id.
    /// </summary>
    /// <param name="id">The ID of the command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of requested commands.</returns>
    Task<List<RequestedCommand>> GetOverlappingRequestedCommandsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send command to Mapped Gateway.
    /// </summary>
    /// <param name="id">The ID of the command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task ExecuteResolvedCommandAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Get all Resolved Command which are due for execution.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of requested commands.</returns>
    Task<List<ResolvedCommand>> GetDueResolvedCommandsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Resolve commands.
    /// </summary>
    /// <param name="externalId">The asset external ID.</param>
    /// <param name="valueChanged">Whether or not the value has changed.</param>
    /// <param name="commands">The list of requested commands.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task.</returns>
    Task ResolveAsync(string externalId, bool valueChanged, List<RequestedCommand> commands, CancellationToken cancellationToken = default);
}
