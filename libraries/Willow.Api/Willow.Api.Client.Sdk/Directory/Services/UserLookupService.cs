namespace Willow.Api.Client.Sdk.Directory.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Willow.Api.Client.Sdk.Directory.Dto;
using Willow.Api.Common.Runtime;

/// <summary>
/// A service for looking up information about the current user.
/// </summary>
public class UserLookupService : IUserLookupService
{
    private readonly ICurrentHttpContext currentHttpContext;
    private readonly ILogger<UserLookupService> logger;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLookupService"/> class.
    /// </summary>
    /// <param name="currentHttpContext">The current http context.</param>
    /// <param name="httpClient">An instance of an httpClient.</param>
    /// <param name="logger">An ILogger instance.</param>
    public UserLookupService(
        ICurrentHttpContext currentHttpContext,
        HttpClient httpClient,
        ILogger<UserLookupService> logger)
    {
        this.currentHttpContext = currentHttpContext;
        this.httpClient = httpClient;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CurrentUserDto?> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        if (!currentHttpContext.IsAuthenticatedRequest)
        {
            logger.LogTrace("Current http context is not authenticated");
            return null;
        }

        var authorizationHeader = new AuthenticationHeaderValue("Bearer", currentHttpContext.BearerToken);
        httpClient.DefaultRequestHeaders.Authorization = authorizationHeader;

        using var response = await httpClient.GetAsync("users/current", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogTrace("Response was not successful: {Reason}", response.ReasonPhrase);
            return null;
        }

        var user = await response.Content.ReadFromJsonAsync<CurrentUserDto>(cancellationToken: cancellationToken);
        return user;
    }
}
