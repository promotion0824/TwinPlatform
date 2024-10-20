using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Helpers;
using Authorization.TwinPlatform.Common.Model;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Authorization.TwinPlatform.Common.Services;
public class ClientAuthorizationService(ILogger<ClientAuthorizationService> logger,
    IHttpClientFactory httpClientFactory,
    IAuthorizationApiTokenService tokenService,
    IImportService importService,
    IOptions<AuthorizationAPIOption> authorizationAPIOption,
    IMemoryCache memoryCache) : IClientAuthorizationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(AuthorizationAPIOption.APIName);
    private readonly AuthorizationAPIOption _authorizationApiConfig = authorizationAPIOption.Value;
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Get authorized permissions for a client from authorization api.
    /// </summary>
    /// <param name="clientId">Id of the client.</param>
    /// <returns>List of Authorized Permission.</returns>
    public async Task<List<AuthorizedPermission>> GetClientAuthorizedPermissions(string clientId)
    {
        var requestKeyName = $"ClientPermissions_{clientId}";

        return await OnceOnly<List<AuthorizedPermission>>.Execute(requestKeyName, async () =>
        {
            await importService.ImportDataFromConfigLazy();

            return await memoryCache.GetOrCreateAsync(requestKeyName, async (cacheEntry) =>
            {
                logger.LogTrace("Sending request to get all the permission authorized for the {client}", clientId);

                await tokenService.AuthorizeClient(_httpClient);

                var queryParams = new Dictionary<string, string?>
                                    {
                                    {"clientId",clientId },
                                    {"application",_authorizationApiConfig.ExtensionName }
                                    };

                var httpResponseMessage = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"api/permission/client", queryParams));

                httpResponseMessage.EnsureSuccessStatusCode();

                logger.LogTrace("Request Complete: Successfully retrieved the permissions from permission api.");

                var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

                var response = JsonSerializer.Deserialize<List<AuthorizedPermission>>(responseString, jsonSerializerOptions)
                    ?? [];

                cacheEntry.AbsoluteExpirationRelativeToNow = _authorizationApiConfig.CacheExpiration;

                return response;
            }) ?? [];
        });
    }
}
