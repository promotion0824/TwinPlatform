namespace Willow.IoTService.Deployment.DbSync.Application.Infrastructure;

using ConnectorCore.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.DataAccess.Services;

/// <inheritdoc />
public class UpdateModuleService(
    IModuleDataService moduleDataService,
    IConfiguration config,
    ILogger<UpdateModuleService> logger)
    : IUpdateModuleService
{
    /// <inheritdoc />
    public async Task<ModuleDto> UpdateModuleAsync(
        IConnectorMessage connectorConfig,
        CancellationToken cancellationToken)
    {
        var moduleInput = new ModuleUpsertInput(connectorConfig.Name,
                                                connectorConfig.ConnectorType,
                                                connectorConfig.Archived,
                                                Id: connectorConfig.ConnectorId);

        logger.LogInformation("Updating module: {Input}", moduleInput);
        var module = await moduleDataService.UpsertAsync(moduleInput, cancellationToken);

        return module;
    }

    /// <inheritdoc />
    public async Task<ModuleDto> UpdateModuleConfigAsync(IConnectorMessage connectorConfig, CancellationToken cancellationToken)
    {
        var iotHubName = config.GetSection("IoTHubName").Value;
        var moduleUpdateInput = new ModuleUpdateConfigurationInput(connectorConfig.ConnectorId,
                                                                   false,
                                                                   null,
                                                                   iotHubName,
                                                                   null);

        logger.LogInformation("Updating module config: {Input}", moduleUpdateInput);
        var module = await moduleDataService.UpdateConfigurationAsync(moduleUpdateInput, cancellationToken);

        return module;
    }
}
