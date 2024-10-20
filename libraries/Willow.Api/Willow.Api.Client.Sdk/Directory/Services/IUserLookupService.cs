namespace Willow.Api.Client.Sdk.Directory.Services;

using Willow.Api.Client.Sdk.Directory.Dto;

/// <summary>
/// A service for looking up information about the current user.
/// </summary>
public interface IUserLookupService
{
    /// <summary>
    /// Get the current user.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<CurrentUserDto?> GetCurrentUser(CancellationToken cancellationToken = default);
}
