namespace Willow.Api.Client.Sdk.Marketplace.Services;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Willow.Api.Authentication;
using Willow.Api.Client.Sdk.Marketplace.Dto;
using Willow.Api.Common.Runtime;

/// <summary>
/// A service for calling the Marketplace API.
/// </summary>
public class MarketplaceApiService : IMarketplaceApiService
{
    private readonly HttpClient httpClient;
    private readonly IClientCredentialTokenService clientCredentialTokenService;
    private readonly ILogger<MarketplaceApiService> logger;
    private readonly ICurrentHttpContext? currentHttpContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarketplaceApiService"/> class.
    /// </summary>
    /// <param name="httpClient">An instance of an httpClient.</param>
    /// <param name="clientCredentialTokenService">An instance of the client credential token service.</param>
    /// <param name="logger">A ILogger instance.</param>
    /// <param name="currentHttpContext">The current httpContext of the request.</param>
    public MarketplaceApiService(
        HttpClient httpClient,
        IClientCredentialTokenService clientCredentialTokenService,
        ILogger<MarketplaceApiService> logger,
        ICurrentHttpContext? currentHttpContext = null)
    {
        this.httpClient = httpClient;
        this.clientCredentialTokenService = clientCredentialTokenService;
        this.logger = logger;
        this.currentHttpContext = currentHttpContext;
    }

    /// <inheritdoc/>
    public async Task<ExtensionDetailDto> GetExtension(
        Guid extensionId,
        string extensionVersion,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"extensions/{extensionId}/versions/{extensionVersion}";

        using (logger.BeginScope("{ExtensionId}", extensionId))
        using (logger.BeginScope("{ExtensionVersion}", extensionVersion))
        using (logger.BeginScope("{Request}", $"GET {requestUri}"))
        {
            httpClient.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            logger.LogTrace("HttpClient returned {HttpStatusCode}", response.StatusCode);

            response.EnsureSuccessStatusCode();

            var marketplaceExtensionDto =
                await response.Content.ReadFromJsonAsync<ExtensionDetailDto>(cancellationToken: cancellationToken);

            if (marketplaceExtensionDto is null)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new JsonException($"HttpResponse could not be parsed {responseJson}");
            }

            return marketplaceExtensionDto;
        }
    }

    /// <inheritdoc/>
    public async Task<ExtensionDetailDto> GetExtension(
        string extensionName,
        string extensionVersion,
        CancellationToken cancellationToken = default)
    {
        var requestUri = $"extensions/{extensionName}/versions/{extensionVersion}";

        using (logger.BeginScope("{ExtensionName}", extensionName))
        using (logger.BeginScope("{ExtensionVersion}", extensionVersion))
        using (logger.BeginScope("{Request}", $"GET {httpClient.BaseAddress}/{requestUri}"))
        {
            httpClient.DefaultRequestHeaders.Authorization = GetAuthenticationHeader();

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            logger.LogTrace("HttpClient returned {HttpStatusCode}", response.StatusCode);

            response.EnsureSuccessStatusCode();

            var marketplaceExtensionDto =
                await response.Content.ReadFromJsonAsync<ExtensionDetailDto>(cancellationToken: cancellationToken);

            if (marketplaceExtensionDto is null)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new JsonException($"HttpResponse could not be parsed {responseJson}");
            }

            return marketplaceExtensionDto;
        }
    }

    private AuthenticationHeaderValue GetAuthenticationHeader()
    {
        string? token;
        if (currentHttpContext?.BearerToken is not null)
        {
            logger.LogTrace("Using User token to call marketplace api");
            token = currentHttpContext.BearerToken;
        }
        else
        {
            logger.LogTrace("Using ClientCredential token to call marketplace api");
            token = clientCredentialTokenService.GetClientCredentialToken();
        }

        return new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, token);
    }
}
