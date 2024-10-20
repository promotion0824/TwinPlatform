namespace Willow.CommandAndControl.Application.Services.Abstractions;

/// <summary>
/// Interacts with Mapped API.
/// </summary>
public interface IMappedGatewayService
{
    /// <summary>
    /// Sends a command to the Mapped API to set the value of a point.
    /// </summary>
    /// <param name="pointId">The ID of the point.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>An response object.</returns>
    Task<SetValueResponse> SendSetValueCommandAsync(string pointId, double value);
}
