using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Permission.Api.Constants;
using System.Diagnostics.Metrics;
using Willow.Telemetry;

namespace Authorization.TwinPlatform.Permission.Api.Services;

/// <summary>
/// Manager interface for importing roles and permissions
/// </summary>
public interface IImportManager
{
    /// <summary>
    /// Import configuration data for the supplied extension
    /// </summary>
    /// <param name="extensionName">Name of your Extension; This name must be unique across all names in Willow.</param>
    /// <param name="importModel">Instance of <see cref="ImportModel"/></param>
    Task ImportConfigurationData(string extensionName, ImportModel importModel);
}

/// <summary>
/// Class for IImport Manager implementation
/// </summary>
public class ImportManager : IImportManager
{
    private readonly ILogger<ImportManager> _logger;
    private readonly IImportService _importService;
    private readonly IApplicationService _applicationService;

    // Metrics
    private readonly Counter<long> _autoImportPermissionRoleCounter;
    private readonly MetricsAttributesHelper _attributeHelper;

    /// <summary>
    /// Initialize a new instance of <see cref="ImportManager"/>
    /// </summary>
    /// <param name="logger">ILogger Instance.</param>
    /// <param name="importService">Instance of <see cref="IImportService"/></param>
    /// <param name="meter">Instance of <see cref="Meter"/></param>
    /// <param name="attributeHelper">Instance of <see cref="MetricsAttributesHelper"/></param>
    public ImportManager(ILogger<ImportManager> logger,
        IImportService importService,
        Meter meter,
        MetricsAttributesHelper attributeHelper,
        IApplicationService applicationService)
    {
        _logger = logger;
        _importService = importService;

        _autoImportPermissionRoleCounter = meter.CreateCounter<long>(TelemetryMeterConstants.AutoImportPermissionRoles);
        _attributeHelper = attributeHelper;
        _applicationService = applicationService;
    }

    /// <summary>
    /// Import configuration data for the supplied extension
    /// </summary>
    /// <param name="extensionName">Name of your Extension; This name must be unique across all names in Willow.</param>
    /// <param name="importModel">Instance of <see cref="ImportModel"/></param>
    /// <returns>Awaitable task.</returns>
    public async Task ImportConfigurationData(string extensionName, ImportModel importModel)
    {
        _logger.LogInformation("Importing roles and permissions for extension: {extension}", extensionName);

        try
        {
            // Get or Create Application
            var applicationModel = await _applicationService.GetOrCreateApplicationByName(extensionName);
            if(importModel.ApplicationOption is not null)
            {
                await _importService.UpdateApplication(applicationModel, importModel.ApplicationOption);
            }
           
            // Import Permission
            if (importModel.Permissions is not null)
                await _importService.ImportPermissionsAsync(applicationModel, importModel.Permissions);

            if (importModel.Roles is not null)
            {
                // Import Roles
                await _importService.ImportRolesAsync(applicationModel, importModel.Roles);

                // Import Role Permission
                await _importService.UpdateRolePermissions(applicationModel, importModel.Roles);
            }

            // Import Permission
            if (importModel.Groups is not null)
                await _importService.ImportGroups(applicationModel, importModel.Groups);

            // Import Group Assignments
            if (importModel.GroupAssignments is not null)
                await _importService.ImportGroupAssignments(applicationModel, importModel.GroupAssignments);

            //Notify Metrics
            _autoImportPermissionRoleCounter.Add(1, _attributeHelper.GetValues(new KeyValuePair<string, object?>(nameof(extensionName),extensionName)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to Auto import Permissions and Role for extension: {Extension}", extensionName);
            throw;
        }
    }
}
