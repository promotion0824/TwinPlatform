using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Abstracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Hosted service to register user management permissions and roles from configuration.
/// </summary>
/// <remarks>
/// Registering user management configuration can take a while, hence we do it in a background service.
/// The permissions and roles seen in the User Management portal are configured in the user-management.import.*env*.json
/// file. The call to ImportDataFromConfigLazy() will read the configuration and register the permissions and roles,
/// if they are edited in the portal they will be overwritten by the configuration the next time this service runs.
/// Groups and assignments can be managed in the portal, permissions and roles should not.
/// </remarks>
public class UserManagementImportHostedService : BackgroundService
{
    private readonly IImportService _userManagement;
    private readonly ILogger<UserManagementImportHostedService> _logger;

    public UserManagementImportHostedService(
        IImportService userManagement,
        ILogger<UserManagementImportHostedService> logger)
    {
        _userManagement = userManagement;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _userManagement.ImportDataFromConfigLazy();
            _logger.LogInformation("Registered user management permissions and roles");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to register user management permissions and roles");
        }
    }
}
