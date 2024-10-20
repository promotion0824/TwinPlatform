namespace Willow.IoTService.Deployment.DbSync.Application;

using System.Text.Json;
using ConnectorCore.Contracts;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.DbSync.Application.HealthChecks;
using Willow.IoTService.Deployment.DbSync.Application.Infrastructure;

/// <summary>
///     Service Bus message consumer class.
/// </summary>
/// <param name="logger">Logger.</param>
/// <param name="validator">Fluent validator implementation.</param>
/// <param name="updateModuleConfig">Handle for service that updates the module in the database.</param>
/// <param name="healthCheckSql">Healthcheck service for SQL.</param>
/// <param name="healthCheckServiceBus">Healthcheck service for Service Bus.</param>
public sealed class ConnectorSyncConsumer(
    ILogger<ConnectorSyncConsumer> logger,
    IValidator<IConnectorMessage> validator,
    IUpdateModuleService updateModuleConfig,
    HealthCheckSql healthCheckSql,
    HealthCheckServiceBus healthCheckServiceBus)
    : IConsumer<IConnectorMessage>
{
    /// <summary>
    ///     Consumes the service bus message and updates the module configuration.
    /// </summary>
    /// <param name="context">Incoming service bus message.</param>
    /// <exception cref="InvalidDataException">Throws exception when there is mismatched data.</exception>
    /// <returns>A <see cref="Task" /> representing consuming the service bus message.</returns>
    public async Task Consume(ConsumeContext<IConnectorMessage> context)
    {
        // init
        var cancellationToken = context.CancellationToken;
        var request = context.Message;

        healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;

        if (!await ValidateRequestAsync(request, cancellationToken))
        {
            return;
        }

        // update module and environment config
        try
        {
            var module = await updateModuleConfig.UpdateModuleAsync(request, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;
            if (module.Id != request.ConnectorId)
            {
                throw new InvalidDataException($"ModuleId {module.Id} does not match with ConnectorId {request.ConnectorId} for module update");
            }

            var updatedModule = await updateModuleConfig.UpdateModuleConfigAsync(request, cancellationToken);

            if (updatedModule.Id != request.ConnectorId)
            {
                throw new InvalidDataException($"ModuleId {updatedModule.Id} does not match with ConnectorId {request.ConnectorId} for configuration update");
            }
        }
        catch (Exception e)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            logger.LogError(e, "Error while updating module and environment config: {Message}", e.Message);
        }
    }

    private async Task<bool> ValidateRequestAsync(IConnectorMessage request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid)
        {
            return true;
        }

        var errorMessage = JsonSerializer.Serialize(validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage.Replace("\'", string.Empty)));
        logger.LogError("Validation failed: {ValidationError}", errorMessage);
        return false;
    }
}
