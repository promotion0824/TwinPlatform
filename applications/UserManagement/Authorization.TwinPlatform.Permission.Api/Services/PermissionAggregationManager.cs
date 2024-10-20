using Authorization.Common.Enums;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Authorization.TwinPlatform.Permission.Api.DTO;
using Authorization.TwinPlatform.Permission.Api.Options;
using Authorization.TwinPlatform.Permission.Api.Requests;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Authorization.TwinPlatform.Permission.Api.Services;

/// <summary>
/// Class for caching and managing Permission Aggregation
/// </summary>
public class PermissionAggregationManager(ILogger<PermissionAggregationManager> logger,
        IMemoryCache memoryCache,
        IPermissionAggregatorService permissionAggregator,
        IOptions<AdminOption> adminOptions,
        IOptions<PermissionCacheOption> appCacheOptions,
        IAuthorizationGraphService authorizationGraphService,
        IUserManager userManager,
        IApplicationService applicationService,
        ICacheInvalidationService cacheInvalidationService,
        HealthCheckSqlServer sqlHealthCheck) : IPermissionAggregationManager
{

    /// <summary>
    /// Get allowed permission for a client.
    /// </summary>
    /// <param name="clientPermissionRequest">Client Permission Request.</param>
    /// <returns>List of permission response.</returns>
    public async Task<List<PermissionResponse>> GetAllowedPermissionForClient(ClientPermissionRequest clientPermissionRequest)
    {
        try
        {
            // Check if the incoming application support client authentication
            var app = await applicationService.GetApplicationByName(clientPermissionRequest.Application);
            if (app == null)
            {
                logger.LogError("Application:{Application} does not exist", clientPermissionRequest.Application);
                return [];
            }
            if (!app.SupportClientAuthentication)
            {
                logger.LogError("Application:{Application} does not support client authentication", clientPermissionRequest.Application);
                return [];
            }

            var response = await memoryCache.GetOrCreateAsync(clientPermissionRequest.GetUniqueKey(), async (cacheEntry) =>
            {
                var permissions = await permissionAggregator.GetClientPermissions(clientPermissionRequest.ClientId, app.Name);

                // set cache expiry to app cache timeout
                cacheEntry.AbsoluteExpirationRelativeToNow = appCacheOptions.Value.AppPermissionCachePeriod;

                return permissions.Select(s => new PermissionResponse(s)).ToList();
            });

            return response!;
        }
        catch (SqlException sqlException)
        {
            sqlHealthCheck.Current = HealthCheckSqlServer.FailingCalls;
            foreach (SqlError sqlError in sqlException.Errors)
            {
                logger.LogInformation("Encountered Sql Exception with Error Number:{Number}", sqlError.Number);
            }
            throw;
        }
        catch (Exception)
        {
            sqlHealthCheck.Current = HealthCheckSqlServer.FailingCalls;
            logger.LogError("Error getting permissions for Client: {ClientId}, Application: {App}", clientPermissionRequest.ClientId, clientPermissionRequest.Application);
            throw;
        }
    }

    /// <summary>
    /// Method to get all allowed permission for a user based on assignment
    /// </summary>
    /// <param name="userPermissionRequest">ListPermission request instance</param>
    /// <returns>Enumerable of permission response</returns>
    public async Task<AuthorizationResponse> GetAllowedPermissionForUser(UserPermissionRequest userPermissionRequest)
    {
        try
        {
            logger.LogTrace("Permission request received for {Application}:{UserName}", userPermissionRequest.Extension, userPermissionRequest.UserEmail);

            //Get the User Record from DB
            var user = await userManager.GetByEmailAsync(userPermissionRequest.UserEmail);

            // Skip Permission check for inactive User and return Is Admin as false
            if (user is not null && user.Status == UserStatus.Inactive)
            {
                logger.LogError("Failed Permission request for {Application}. User Profile {email} is in active.", userPermissionRequest.Extension, userPermissionRequest.UserEmail);
                return new AuthorizationResponse() { Permissions = new List<PermissionResponse>(), IsAdminUser = false };
            }

            List<PermissionResponse> result = [];

            // if user record exist in the database
            if(user is not null)
            {
                result = (await GetPermissionByUMAssignments(userPermissionRequest))!.ToList();
            }

            if (IsInternalUser(userPermissionRequest.UserEmail))
            {
                var adPermissions = (await GetPermissionByADGroup(userPermissionRequest))!;
                result = result.Union(adPermissions).ToList();
            }

            sqlHealthCheck.Current = HealthCheckSqlServer.Healthy;
            logger.LogTrace("Permission request processed successfully with {count} response(s)", result.Count);
            var IsAdminUser = IsUserAnAdmin(userPermissionRequest.UserEmail);

            return new AuthorizationResponse() { Permissions = result, IsAdminUser = IsAdminUser };
        }
        catch (SqlException sqlException)
        {
            sqlHealthCheck.Current = HealthCheckSqlServer.FailingCalls;
            foreach(SqlError sqlError in sqlException.Errors)
            {
                logger.LogInformation("Encountered Sql Exception with Error Number:{Number}", sqlError.Number);
            }
            throw;
        }
        catch (Exception)
        {
            sqlHealthCheck.Current = HealthCheckSqlServer.FailingCalls;
            logger.LogError("Error getting permissions for User: {Email}, Application: {App}", userPermissionRequest.UserEmail, userPermissionRequest.Extension);
            throw;
        }

    }

    /// <summary>
    /// Checks if the users email address is configured to be one of the administrator
    /// </summary>
    /// <param name="adminEmail">Email Address of the admin</param>
    /// <returns>True if admin; false if not</returns>
    private bool IsUserAnAdmin(string adminEmail)
    {
        foreach (var configuredEmail in adminOptions.Value.Admins)
        {
            if (!MailAddress.TryCreate(configuredEmail, out var mailAddress))
                continue;

            // Check if the email address matches the configured admin email
            if ((!string.IsNullOrWhiteSpace(configuredEmail) &&
                string.Equals(mailAddress.Address, adminEmail, StringComparison.InvariantCultureIgnoreCase)))
                return true;
        }

        return false;
    }

    private bool IsInternalUser(string userEmail)
    {
        if (MailAddress.TryCreate(userEmail, out var mailAddress))
        {
            return string.Compare(mailAddress.Host, AdminOption.InternalMailAddressHost, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
        else
        {
            logger.LogError("Unable to parse the user email address {Email}", userEmail);
        }

        return false;
    }

    private async Task<IEnumerable<PermissionResponse>?> GetPermissionByUMAssignments(UserPermissionRequest userPermissionRequest)
    {
        var allUserPermissions = await memoryCache.GetOrCreateAsync(userPermissionRequest.GetUMCacheKey, async (cacheEntry) =>
        {
            //Expire cache after 2 minutes
            cacheEntry.AbsoluteExpirationRelativeToNow = appCacheOptions.Value.AppPermissionCachePeriod;
            cacheEntry.AddExpirationToken(cacheInvalidationService.GetOrCreateChangeToken(CacheStoreType.PermissionByUMAssignment));

            // We get all application permission for the user to cache it and filter only what is needed
            var response = await permissionAggregator.GetUserPermissions(userPermissionRequest.UserEmail);
            return response.Select(s => new PermissionResponse(s)).ToList();
        });
        return allUserPermissions?.Where(w => string.Compare(w.Extension, userPermissionRequest.Extension, StringComparison.InvariantCultureIgnoreCase) == 0) ?? [];
    }

    private async Task<IEnumerable<PermissionResponse>> GetPermissionByADGroup(UserPermissionRequest userPermissionRequest)
    {
        // If user email is empty return empty list
        if (string.IsNullOrWhiteSpace(userPermissionRequest.UserEmail))
        {
            return [];
        }

        var allUserPermissions = await memoryCache.GetOrCreateAsync(userPermissionRequest.GetADCacheKey, async (cacheEntry) =>
        {
            //Expire cache after 2 minutes
            cacheEntry.AbsoluteExpirationRelativeToNow = appCacheOptions.Value.GraphPermissionCachePeriod;
            cacheEntry.AddExpirationToken(cacheInvalidationService.GetOrCreateChangeToken(CacheStoreType.PermissionByADGroup));

            logger.LogTrace("Retrieving permission from Graph API for request {key}", userPermissionRequest.UserEmail + "__PermissionByADGroup");

            var response = await authorizationGraphService.GetAllPermissionByEmail(userPermissionRequest.UserEmail);
            return response.Select(s => new PermissionResponse(s)).ToList();
        });

        return allUserPermissions?.Where(w => string.Compare(w.Extension, userPermissionRequest.Extension, StringComparison.InvariantCultureIgnoreCase) == 0) ?? [];
    }
}
