namespace Willow.Api.Client.Sdk.Marketplace.Services;

using Willow.Api.Client.Sdk.Marketplace.Dto;

/// <summary>
/// A service for calling the Marketplace API.
/// </summary>
public interface IMarketplaceApiService
{
    /// <summary>
    /// Get the extension details.
    /// </summary>
    /// <param name="extensionId">The id of the extension to get.</param>
    /// <param name="extensionVersion">The version of the extension to get.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ExtensionDetailDto> GetExtension(
        Guid extensionId,
        string extensionVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the extension details.
    /// </summary>
    /// <param name="extensionName">The name of the extension to get.</param>
    /// <param name="extensionVersion">The version of the extension to get.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ExtensionDetailDto> GetExtension(
        string extensionName,
        string extensionVersion,
        CancellationToken cancellationToken = default);
}
