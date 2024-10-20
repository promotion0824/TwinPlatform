using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Authorization.TwinPlatform.Web.Constants;
using Authorization.TwinPlatform.Web.HealthChecks;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Security.Claims;
using Willow.Telemetry;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Service implementation to manage user management authorization permissions
/// </summary>
public class UserAuthorizationManager(ILogger<UserAuthorizationManager> logger,
    IUserAuthorizationService userAuthorizationService,
    IAdminService adminService,
    IHttpContextAccessor httpContextAccessor,
    HealthCheckAuthorizationPermissionApi healthCheckPermissionApi,
    Meter meter,
    MetricsAttributesHelper attributeHelper) : IUserAuthorizationManager
{
    private readonly Counter<long> _failedAuthorizationCounter = meter.CreateCounter<long>(TelemetryMeterConstants.FailedAuthorization);

    public string CurrentEmail => GetCurrentUserEmailClaim()?.Value ?? string.Empty;

    /// <summary>
    /// Method to get the authorization data for the current user from Permission API
    /// </summary>
    /// <param name="userEmail">Email address of the principal user</param>
    /// <returns>AuthorizationResponseDto instance</returns>
    public async Task<AuthorizationResponseDto> GetAuthorizationPermissions(string userEmail)
    {
        try
        {
            logger.LogTrace("Getting Authorization Permission for {user}", userEmail);

            var authResponse = await userAuthorizationService.GetAuthorizationResponse(userEmail);

            logger.LogTrace("Retrieved {count} Authorization Permissions for {user}", authResponse.Permissions?.Count() ?? 0, userEmail);

            healthCheckPermissionApi.Current = HealthCheckAuthorizationPermissionApi.Healthy;

            return new AuthorizationResponseDto()
            {
                IsAdminUser = await IsCurrentUserAnAdminAsync(),
                Permissions = authResponse.Permissions?.Select(s => s.Name) ?? new List<string>()
            };
        }
        catch (SocketException)
        {
            healthCheckPermissionApi.Current = HealthCheckAuthorizationPermissionApi.NotConfigured;
            throw;
        }
        catch (Exception)
        {
            logger.LogError("Failed to get response from Authorization API for user:{user}", userEmail);
            _failedAuthorizationCounter.Add(1, attributeHelper.GetValues(new KeyValuePair<string, object?>(TelemetryMeterConstants.FailedAuthorization, userEmail)));
            healthCheckPermissionApi.Current = HealthCheckAuthorizationPermissionApi.FailingCalls;
            throw;
        }
    }

    /// <summary>
    /// Get current user email claim from the http context.
    /// </summary>
    /// <returns>Email claim if found; else null.</returns>
    private Claim? GetCurrentUserEmailClaim()
    {
        return httpContextAccessor.HttpContext?
                .User?.FindFirst(claim => claim.Type == "emails" || claim.Type == ClaimTypes.Email);
    }

    /// <summary>
    /// Check if the current user has any permission that matches the input permission name.
    /// </summary>
    /// <returns>True if current user has access;else false.</returns>
    public async Task<bool> CheckCurrentUserHasPermission(string permissionName)
    {
        var currentUserEmailClaim = GetCurrentUserEmailClaim();
        if (currentUserEmailClaim is null)
            return false;

        var authorizationData = await GetAuthorizationPermissions(currentUserEmailClaim.Value);

        return authorizationData.IsAdminUser ||
            authorizationData.Permissions.Any(a => string.Compare(permissionName, a, StringComparison.InvariantCultureIgnoreCase) == 0);
    }

    /// <summary>
    /// Check if the current user is admin by querying permission api.
    /// </summary>
    /// <returns>True if admin; false if not.</returns>
    public async Task<bool> IsCurrentUserAnAdminAsync()
    {
        var currentUserEmailClaim = GetCurrentUserEmailClaim();
        if (currentUserEmailClaim is null)
            return false;

        var allAdmins = await adminService.GetAdminEmails();
        return allAdmins.Any(a => string.Equals(a, currentUserEmailClaim.Value, StringComparison.InvariantCultureIgnoreCase));
    }
}
