using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RulesEngine.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Willow.Rules.Web;
using WillowRules.Extensions;

namespace RulesEngine.Web;

/// <summary>
/// A service for getting information about the logged in user
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get information about the logged in user
    /// </summary>
    Task<AuthenticatedUserDto> GetUser(ClaimsPrincipal user);

    /// <summary>
    /// Get the permissions for the user
    /// </summary>
    Task<List<string>> GetPermissions(ClaimsPrincipal user);
}

/// <summary>
/// A service for getting information about the logged in user
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> logger;
    private readonly IMemoryCache memoryCache;
    private readonly IWebHostEnvironment environment;
    private readonly IUserAuthorizationService userAuthorizationService;
    private readonly IImportService importService;
    private readonly HealthCheckAuthorizationService healthCheckAuthorizationService;
    private readonly IEnumerable<IWillowRole> roles;
    private readonly IEnumerable<IWillowAuthorizationRequirement> authorizationRequirements;

    /// <summary>
    /// Creates a new UserService
    /// </summary>
    public UserService(
        ILogger<UserService> logger,
        IMemoryCache memoryCache,
        IWebHostEnvironment environment,
        IEnumerable<IWillowRole> roles,
        IEnumerable<IWillowAuthorizationRequirement> authorizationRequirements,
        HealthCheckAuthorizationService healthCheckAuthorizationService,
        IUserAuthorizationService userAuthorizationService = null,
        IImportService importService = null
        )
    {
        this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        this.memoryCache = memoryCache ?? throw new System.ArgumentNullException(nameof(memoryCache));
        this.environment = environment ?? throw new System.ArgumentNullException(nameof(environment));
        this.authorizationRequirements = authorizationRequirements ?? throw new System.ArgumentNullException(nameof(authorizationRequirements));
        //dont throw null exception. The service is optional if it's not configured correctly
        this.userAuthorizationService = userAuthorizationService;
        this.importService = importService;
        this.healthCheckAuthorizationService = healthCheckAuthorizationService ?? throw new ArgumentNullException(nameof(healthCheckAuthorizationService));
        this.roles = roles ?? throw new System.ArgumentNullException(nameof(roles));
    }

    /// <summary>
    /// Null value object for a not logged in user
    /// </summary>
    private static readonly AuthenticatedUserDto NOTLOGGEDIN = new AuthenticatedUserDto
    {
        Id = "",
        DisplayName = "Not logged in"
    };

    /// <summary>
    /// Get user information for the logged in user
    /// </summary>
    public Task<AuthenticatedUserDto> GetUser(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);  // A guid

        if (userId is null) return Task.FromResult(NOTLOGGEDIN);

        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException("Empty NameIdentifier guid for user");

        string cacheKey = "User" + userId;

        var authenticatedUser = memoryCache.GetOrCreate(cacheKey, (c) =>
        {
            var u = user;
            // Assume users don't change groups or names too often
            c.SetSlidingExpiration(TimeSpan.FromMinutes(5));

            return GetUserInternal(u);
        });

        return Task.FromResult(authenticatedUser);
    }

    private DateTimeOffset DisabledUntil = DateTimeOffset.MinValue;

    /// <summary>
    /// Gets the permission set for a user
    /// </summary>
    public async Task<List<string>> GetPermissions(ClaimsPrincipal user)
    {
        var email = user.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            return new List<string>();
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);  // A guid

        string key = $"AUTH_{userId}_permissions";

        var result = await OnceOnly<List<string>>.Execute(key, async () =>
        {
            var cachedResult = await memoryCache.GetOrCreateAsync(key, async (c) =>
            {
                var response = await GetPermissionsInternal(user);

                var permissions = new HashSet<string>();

                c.SetAbsoluteExpiration(TimeSpan.FromSeconds(30));

                if (response is not null)
                {
                    c.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                    permissions.UnionWith(response.Permissions.Select(v => v.Name));
                }
                else if (userAuthorizationService is null && environment.IsDevelopment())
                {
                    //usually for local dev is service not configured
                    permissions.UnionWith(AuthPolicy.AdminPermissionSet.Select(v => v.Name));
                }

                logger.LogInformation("Willow Auth {user} : permissions {permissions}", email, string.Join(", ", permissions));

                return permissions.ToList();
            });

            return cachedResult;
        });

        return result;
    }

    private static AsyncRetryPolicy retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );

    private async Task<AuthorizationResponse> GetPermissionsInternal(ClaimsPrincipal user)
    {
        var email = user.FindFirstValue(ClaimTypes.Email);

        if (userAuthorizationService is not null && DateTimeOffset.Now > DisabledUntil)
        {
            await memoryCache.GetOrCreateAsync("Permissions_Import", async (c) =>
            {
                await OnceOnly<bool>.Execute("Permissions_Import", async () =>
                {
                    try
                    {
                        await ImportPermissionsAndRoles();

                        healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.Healthy;
                    }
                    catch (HttpRequestException hex) when (hex.Message.Contains("Name or service not known"))
                    {
                        c.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                        logger.LogWarning(hex, "Could not contact authorization service to import roles - service not found");
                        DisabledUntil = DateTimeOffset.Now.AddHours(1);
                        healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.NotConfigured;
                    }
                    catch (Exception ex)
                    {
                        c.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
                        logger.LogWarning(ex, "Could not contact authorization service to import roles");
                        DisabledUntil = DateTimeOffset.Now.AddHours(1);
                        healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.FailingRequests;
                    }

                    return true;
                });

                return true;
            });

            try
            {
                logger.LogInformation("Willow Auth calling API");

                var response = await retryPolicy.ExecuteAsync(async () =>
                {
                    return await userAuthorizationService.GetAuthorizationResponse(email);
                });

                healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.Healthy;

                return response;
            }
            catch (AuthenticationFailedException aex) when (aex.Message.Contains("scope is not in expected format"))
            {
                logger.LogWarning(aex, "Willow Auth is not configured, disabling it for 5 hours");
                DisabledUntil = DateTimeOffset.Now.AddHours(5);
                healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.FailingRequests;
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Willow Auth is not working, disabling it for 15 minutes");
                DisabledUntil = DateTimeOffset.Now.AddMinutes(15);
                healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.FailingRequests;
            }
            catch (HttpRequestException hex) when (hex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Normal failed auth, user is not in Auth service
                logger.LogInformation("Unauthorized (401) {email} is not authenticated", email);
            }
            catch (HttpRequestException hex) when (hex.StatusCode == HttpStatusCode.Forbidden)
            {
                // Normal failed auth, user is not authorized in Auth service
                logger.LogInformation("Forbidden (403) {email} is not authorized", email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Willow Auth API failed for user {email}. Check Auth API is running.", email);
                logger.LogInformation("Willow Auth API failed for user {email}. Check Auth API is running.", email);
                healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.FailingRequests;
            }
        }
        else
        {
            healthCheckAuthorizationService.Current = HealthCheckAuthorizationService.NotConfigured;
        }

        return null;
    }

    /// <summary>
    /// Get user information for the logged in user
    /// </summary>
    private AuthenticatedUserDto GetUserInternal(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);  // A guid
        if (string.IsNullOrEmpty(userId))
        {
            return NOTLOGGEDIN;
        }

        using var disp = logger.BeginScope(new Dictionary<string, object> { ["UserId"] = userId });

        var userEmail = user.FindFirstValue(ClaimTypes.Email);
        var displayName = user.UserName();

        // Issuer is always B2C
        //  https://willowdevb2c.b2clogin.com/a80618f8-f5e9-43bf-a98f-107dd8f54aa9/v2.0/
        //  https://willowidentity.b2clogin.com/540c8929-ab7e-478f-b401-cbd037da66bd/v2.0/
        var issuer = user.FindFirstValue("iss");

        //  IdentityProvider for microsoft login using Willow email address (same on DEV/PROD):
        //      https://login.microsoftonline.com/d43166d1-c2a1-4f26-a213-f620dba13ab8/v2.0
        //
        //  IdentityProvider for microsoft login using non-Willow email address:
        //      ??? don't have one to test against ???
        //
        //  IdentityProvider for non-federated login (B2C user):
        //      null
        //
        //  IdentityProvider for federated login:
        //      ??? don't have one to test against ???
        var identityProvider = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/identityprovider");

        logger.LogDebug("Login issuer {issuer} identityProvider {identityProvider}", issuer, identityProvider);

        logger.LogInformation("Loaded user {email} {name}", userEmail, displayName);

        return new AuthenticatedUserDto
        {
            Id = userId,
            DisplayName = displayName,
            Email = userEmail
        };
    }

    private async Task<bool> ImportPermissionsAndRoles()
    {
        string key = $"Import_Permissions";

        var result = await memoryCache.GetOrCreateAsync(key, async (c) =>
        {
            var result = await OnceOnly<bool>.Execute(key, async () =>
            {
                var importModel = new ImportModel()
                {
                    Roles = new List<ImportRole>(),
                    Permissions = new List<ImportPermission>()
                };

                foreach (var role in roles)
                {
                    importModel.Roles.Add(new ImportRole()
                    {
                        Name = role.Name,
                        Permissions = role.Permissions.Select(v => v.Name).ToArray()
                    });
                }

                foreach (var permission in authorizationRequirements)
                {
                    importModel.Permissions.Add(new ImportPermission()
                    {
                        Description = permission.Description,
                        Name = permission.Name
                    });
                }

                if (importService is not null)
                {
                    try
                    {
                        logger.LogInformation("Authorization: Importing roles: {roles}", string.Join(",", importModel.Roles.Select(v => v.Name)));

                        await importService.ImportDataFromConfiguration(importModel);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.InnerException.Message.Equals("Name or service not known"))
                        {
                            logger.LogWarning("Authorization: Failed to import roles and permissions. Name or service not known.");

                            DisabledUntil = DateTimeOffset.Now.AddHours(1);
                        }
                        else
                        {
                            logger.LogError(ex, "Authorization: Failed to import roles and permissions");

                            DisabledUntil = DateTimeOffset.Now.AddMinutes(15);
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to import roles and permissions, unknown error");
                        return false;
                    }
                }

                return true;
            });

            if (!result)
            {
                c.SetAbsoluteExpiration(TimeSpan.FromMinutes(2));  //force an expiry to retry failed imports
            }

            return result;
        });

        return result;
    }
}
