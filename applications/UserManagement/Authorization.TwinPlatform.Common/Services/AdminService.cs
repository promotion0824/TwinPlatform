using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization.TwinPlatform.Common.Services;

/// <summary>
/// Admin Service Client Implementation
/// </summary>
public class AdminService(ILogger<AdminService> logger,
    IHttpClientFactory httpClientFactory,
    IAuthorizationApiTokenService tokenService,
     IMemoryCache memoryCache) : IAdminService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(AuthorizationAPIOption.APIName);

    /// <summary>
    /// Get List of configured admin emails from permission api.
    /// </summary>
    /// <returns>List of emails.</returns>
    public async Task<IEnumerable<string>> GetAdminEmails()
    {
        var response =  await memoryCache.GetOrCreateAsync("Authorization_Admins", async (cacheEntry) =>
        {
            logger.LogTrace("Sending request to get all admins.");

            await tokenService.AuthorizeClient(_httpClient);

            var httpResponseMessage = await _httpClient.GetAsync("api/admin");

            httpResponseMessage.EnsureSuccessStatusCode();

            logger.LogTrace("Request Complete: Successfully retrieved list of admins from permission api.");

            var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<IEnumerable<string>>(responseString);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

            return response ?? Enumerable.Empty<string>();
        });
        return response!;
    }
}
