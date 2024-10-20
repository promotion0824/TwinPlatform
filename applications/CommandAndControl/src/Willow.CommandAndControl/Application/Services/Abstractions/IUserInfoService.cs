namespace Willow.CommandAndControl.Application.Services.Abstractions;

/// <summary>
/// Provides information about the current user.
/// </summary>
public interface IUserInfoService
{
    /// <summary>
    /// Gets Username if user is logged in.
    /// </summary>
    /// <returns>A user.</returns>
    User? GetUser();
}
