using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Helpers;
using Authorization.TwinPlatform.Common.Model;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Common.Services;

/// <summary>
/// Authorization Service used by the Client Application (Extension) to make calls to the permission api
/// </summary>
public class UserAuthorizationService(ILogger<UserAuthorizationService> logger,
    IHttpClientFactory httpClientFactory,
    IAuthorizationApiTokenService tokenService,
    IImportService importService,
    IOptions<AuthorizationAPIOption> authorizationAPIOption,
    IMemoryCache memoryCache) : IUserAuthorizationService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(AuthorizationAPIOption.APIName);
    private readonly AuthorizationAPIOption _authorizationApiConfig = authorizationAPIOption.Value;
    private static readonly JsonSerializerOptions jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Method to get the list of authorized permission for the supplied user email.
    /// <param name="userName">Email Address of the User</param>
    /// <returns>Authorization Response model</returns>
    public Task<AuthorizationResponse> GetAuthorizationResponse(string userName)
        => GetUserAuthorizedPermissions(userName);

    /// <summary>
    /// Get batch of groups (Group Type: Application)
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <returns>Batch DTO of GroupModel</returns>
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsAsync(BatchRequestDto batchRequest)
    {
        await tokenService.AuthorizeClient(_httpClient);

        var json = JsonSerializer.Serialize(batchRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpResponseMessage = await _httpClient.PostAsync("api/group/batch",content);
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

        var response = JsonSerializer.Deserialize<BatchDto<GroupModel>>(responseString, jsonSerializerOptions)
            ?? new BatchDto<GroupModel>() { Items = [] };

        return response;
    }

    /// <summary>
    /// Get batch of groups (Group Type: Application) for a User (: UserId)
    /// </summary>
    /// <param name="userId">Guid of the User record.</param>
    /// <param name="batchRequest">Batch Request DTO.</param>
    /// <returns>Batch DTO of GroupModel</returns>
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsByUserAsync(string userId, BatchRequestDto batchRequest)
    {
        await tokenService.AuthorizeClient(_httpClient);

        var json = JsonSerializer.Serialize(batchRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpResponseMessage = await _httpClient.PostAsync($"api/group/batch/user?userId={userId}", content);
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

        var response = JsonSerializer.Deserialize<BatchDto<GroupModel>>(responseString, jsonSerializerOptions)
            ?? new BatchDto<GroupModel>() { Items = [] };

        return response;
    }

    /// <summary>
    /// Get list of users based on the filter property model.
    /// </summary>
    /// <param name="filterModel">Filter Property Model.</param>
    /// <returns>ListResponse<UserModel></returns>
    public async Task<ListResponse<UserModel>> GetUsersAsync(FilterPropertyModel filterModel)
    {
        await tokenService.AuthorizeClient(_httpClient);
        var httpResponseMessage = await _httpClient.GetAsync(QueryHelpers.AddQueryString("api/user", filterModel?.ToQueryParams() ?? []));
        httpResponseMessage.EnsureSuccessStatusCode();
        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ListResponse<UserModel>>(responseString, jsonSerializerOptions)
            ?? new ListResponse<UserModel>([]);
        return response;
    }

    /// <summary>
    /// Get user by email address.
    /// </summary>
    /// <param name="email">Email address of the user.</param>
    /// <returns>UserModel if found; else null.</returns>
    public async Task<UserModel?> GetUserByEmailAsync(string email)
    {
        await tokenService.AuthorizeClient(_httpClient);
        var httpResponseMessage = await _httpClient.GetAsync($"api/user/email/{email}");
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<UserModel?>(responseString, jsonSerializerOptions);
            return response;
        }
        else
            return null;
    }

    /// <summary>
    /// Get List of User By Ids.
    /// </summary>
    /// <param name="userIds">Ids of the user.</param>
    /// <returns>List of UserModel.</returns>
    public async Task<ListResponse<UserModel>> GetListOfUserByIds(string[] userIds)
    {
        await tokenService.AuthorizeClient(_httpClient);
        var httpResponseMessage = await _httpClient.GetAsync(QueryHelpers.AddQueryString("api/user/byIds",userIds.Select(s=>new KeyValuePair<string,string?>("userIds",s))));
        httpResponseMessage.EnsureSuccessStatusCode();

        var responseString = await httpResponseMessage.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<ListResponse<UserModel>>(responseString, jsonSerializerOptions)
                        ?? new ListResponse<UserModel>([]);
        return response;

    }

    /// <summary>
    /// Invalidate cache for the requested cache store types.
    /// </summary>
    /// <param name="cacheStoreTypes">Types of cache stores.</param>
    /// <returns>Awaitable Task.</returns>
    public async Task InvalidateCache(CacheStoreType[] cacheStoreTypes)
    {
        await tokenService.AuthorizeClient(_httpClient);
        var httpResponseMessage = await _httpClient.PostAsJsonAsync("api/cache/invalidate", cacheStoreTypes);
        httpResponseMessage.EnsureSuccessStatusCode();
    }
    private async Task<AuthorizationResponse> GetUserAuthorizedPermissions(string userName)
    {
        var requestKeyName = $"AuthPermissions_{userName}";

#pragma warning disable CS8603 // Possible null reference return.
        return await OnceOnly<AuthorizationResponse>.Execute(requestKeyName, async () =>
        {
            await importService.ImportDataFromConfigLazy();

            return await memoryCache.GetOrCreateAsync(requestKeyName, async (cacheEntry) =>
            {
                logger.LogTrace("Sending request to get all the permission authorized for the {user}", userName);

                await tokenService.AuthorizeClient(_httpClient);

                var queryParams = new Dictionary<string, string?>
                                    {
                                    {"userEmail",userName },
                                    {"extension",_authorizationApiConfig.ExtensionName }
                                    };

                var httpResponseMessage = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"api/permission", queryParams));

                httpResponseMessage.EnsureSuccessStatusCode();

                logger.LogTrace("Request Complete: Successfully retrieved the permissions from permission api.");

                var responseString = await httpResponseMessage.Content.ReadAsStringAsync();

                var response = JsonSerializer.Deserialize<AuthorizationResponse>(responseString, jsonSerializerOptions)
                    ?? new AuthorizationResponse();

                cacheEntry.AbsoluteExpirationRelativeToNow = _authorizationApiConfig.CacheExpiration;

                return response;
            });
        });
#pragma warning restore CS8603 // Possible null reference return.
    }
}

